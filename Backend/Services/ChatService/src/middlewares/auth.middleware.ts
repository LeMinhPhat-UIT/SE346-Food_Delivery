import crypto from "node:crypto";
import { NextFunction, Request, Response } from "express";
import { env } from "../config/env.config";
import { HTTP_STATUS } from "../constants/httpStatus";
import { ROLES } from "../constants/roles";
import { ApiError } from "../utils/apiError";

const DOTNET_ROLE_CLAIM =
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

type JwtPayload = Record<string, unknown> & {
  sub?: string;
  userId?: string;
  email?: string;
  iss?: string;
  aud?: string | string[];
  exp?: number;
  nbf?: number;
  role?: string | string[];
  merchantId?: string;
  shipperId?: string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string | string[];
};

const decodeBase64Url = (value: string) => {
  const normalized = value.replace(/-/g, "+").replace(/_/g, "/");
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "=");
  return Buffer.from(padded, "base64");
};

const verifyJwt = (token: string): JwtPayload => {
  const [encodedHeader, encodedPayload, encodedSignature] = token.split(".");

  if (!encodedHeader || !encodedPayload || !encodedSignature) {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid access token");
  }

  const header = JSON.parse(decodeBase64Url(encodedHeader).toString("utf8")) as { alg?: string };
  if (header.alg !== "HS256") {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Unsupported token algorithm");
  }

  const signingInput = `${encodedHeader}.${encodedPayload}`;
  const expectedSignature = crypto
    .createHmac("sha256", env.JWT_SECRET)
    .update(signingInput)
    .digest("base64url");

  const expectedBuffer = Buffer.from(expectedSignature);
  const actualBuffer = Buffer.from(encodedSignature);

  if (
    expectedBuffer.length !== actualBuffer.length ||
    !crypto.timingSafeEqual(expectedBuffer, actualBuffer)
  ) {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid access token signature");
  }

  const payload = JSON.parse(decodeBase64Url(encodedPayload).toString("utf8")) as JwtPayload;
  const now = Math.floor(Date.now() / 1000);

  if (payload.exp && payload.exp <= now) {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Access token has expired");
  }

  if (payload.nbf && payload.nbf > now) {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Access token is not active yet");
  }

  if (env.JWT_ISSUER && payload.iss !== env.JWT_ISSUER) {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid token issuer");
  }

  if (env.JWT_AUDIENCE) {
    const audiences = Array.isArray(payload.aud) ? payload.aud : [payload.aud];
    if (!audiences.includes(env.JWT_AUDIENCE)) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid token audience");
    }
  }

  return payload;
};

const extractRoles = (payload: JwtPayload) => {
  const roles = new Set<string>();
  const directRole = payload.role;
  const dotNetRoles = payload[DOTNET_ROLE_CLAIM];

  const pushRole = (value: unknown) => {
    if (typeof value === "string") {
      roles.add(value.toUpperCase());
    }
  };

  if (Array.isArray(directRole)) {
    directRole.forEach(pushRole);
  } else {
    pushRole(directRole);
  }

  if (Array.isArray(dotNetRoles)) {
    dotNetRoles.forEach(pushRole);
  } else {
    pushRole(dotNetRoles);
  }

  return Array.from(roles);
};

export const authenticate = (req: Request, _res: Response, next: NextFunction) => {
  try {
    const authorization = req.headers.authorization;
    if (!authorization?.startsWith("Bearer ")) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Bearer token is required");
    }

    const token = authorization.slice("Bearer ".length).trim();
    const payload = verifyJwt(token);
    const userId =
      typeof payload.sub === "string"
        ? payload.sub
        : typeof payload.userId === "string"
          ? payload.userId
          : undefined;

    if (!userId) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    req.auth = {
      userId,
      email: typeof payload.email === "string" ? payload.email : undefined,
      roles: extractRoles(payload),
      token,
      claims: payload,
      merchantId: typeof payload.merchantId === "string" ? payload.merchantId : undefined,
      shipperId: typeof payload.shipperId === "string" ? payload.shipperId : undefined,
    };

    next();
  } catch (error) {
    next(error);
  }
};

export const requireRoles = (...roles: string[]) => {
  const normalized = roles.map((role) => role.toUpperCase());

  return (req: Request, _res: Response, next: NextFunction) => {
    try {
      if (!req.auth) {
        throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Authentication required");
      }

      const hasRole = req.auth.roles.some((role) => normalized.includes(role.toUpperCase()));
      if (!hasRole) {
        throw new ApiError(HTTP_STATUS.FORBIDDEN, "You do not have permission");
      }

      next();
    } catch (error) {
      next(error);
    }
  };
};

export const isAdmin = (req: Request) => req.auth?.roles.includes(ROLES.ADMIN);
