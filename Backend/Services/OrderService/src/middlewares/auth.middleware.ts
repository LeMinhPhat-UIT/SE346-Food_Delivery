import crypto from "crypto";
import { NextFunction, Request, Response } from "express";
import { HTTP_STATUS } from "../constants/httpStatus";
import { ROLES } from "../constants/roles";
import { env } from "../config/env.config";
import { UserServiceClient } from "../integrations/user.service";
import { ApiError } from "../utils/apiError";

const DOTNET_ROLE_CLAIM =
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

const userServiceClient = new UserServiceClient();

type JwtPayload = Record<string, unknown> & {
  sub?: string;
  email?: string;
  iss?: string;
  aud?: string | string[];
  exp?: number;
  role?: string | string[];
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

  const header = JSON.parse(decodeBase64Url(encodedHeader).toString("utf8")) as {
    alg?: string;
    typ?: string;
  };

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

  const payload = JSON.parse(
    decodeBase64Url(encodedPayload).toString("utf8"),
  ) as JwtPayload;

  const nowInSeconds = Math.floor(Date.now() / 1000);

  if (payload.exp && payload.exp <= nowInSeconds) {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Access token has expired");
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

  if (typeof directRole === "string") {
    roles.add(directRole);
  } else if (Array.isArray(directRole)) {
    directRole.forEach((role) => typeof role === "string" && roles.add(role));
  }

  if (typeof dotNetRoles === "string") {
    roles.add(dotNetRoles);
  } else if (Array.isArray(dotNetRoles)) {
    dotNetRoles.forEach((role) => typeof role === "string" && roles.add(role));
  }

  return Array.from(roles);
};

export const authenticate = (
  req: Request,
  _res: Response,
  next: NextFunction,
) => {
  try {
    const authorization = req.headers.authorization;

    if (!authorization?.startsWith("Bearer ")) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Bearer token is required");
    }

    const token = authorization.slice("Bearer ".length).trim();
    const payload = verifyJwt(token);
    const userId = typeof payload.sub === "string" ? payload.sub : undefined;

    if (!userId) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    req.auth = {
      userId,
      email: typeof payload.email === "string" ? payload.email : undefined,
      roles: extractRoles(payload),
      token,
      claims: payload,
    };

    next();
  } catch (error) {
    next(error);
  }
};

export const requireRoles = (...allowedRoles: string[]) => {
  return (req: Request, _res: Response, next: NextFunction) => {
    const roles = req.auth?.roles ?? [];
    const allowedRoleSet = new Set(allowedRoles.map((role) => role.toLowerCase()));

    const isAllowed = roles.some((role) =>
      allowedRoleSet.has(role.toLowerCase()),
    );

    if (!isAllowed) {
      return next(new ApiError(HTTP_STATUS.FORBIDDEN, "You are not allowed to access this resource"));
    }

    return next();
  };
};

export const attachMerchantContext = async (
  req: Request,
  _res: Response,
  next: NextFunction,
) => {
  try {
    const auth = req.auth;

    if (!auth) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    const isAdmin = auth.roles.some(
      (role) => role.toLowerCase() === ROLES.ADMIN.toLowerCase(),
    );

    if (isAdmin) {
      return next();
    }

    const isMerchant = auth.roles.some(
      (role) => role.toLowerCase() === ROLES.MERCHANT.toLowerCase(),
    );

    if (!isMerchant) {
      throw new ApiError(
        HTTP_STATUS.FORBIDDEN,
        "Merchant role is required for this action",
      );
    }

    const merchant = await userServiceClient.getMerchantByUserId(
      auth.userId,
      auth.token,
    );

    if (!merchant) {
      throw new ApiError(HTTP_STATUS.FORBIDDEN, "Merchant profile was not found");
    }

    req.auth = {
      ...auth,
      merchantId: merchant.id,
    };

    next();
  } catch (error) {
    next(error);
  }
};
