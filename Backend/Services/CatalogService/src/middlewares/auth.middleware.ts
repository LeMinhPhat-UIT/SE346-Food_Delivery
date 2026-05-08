import crypto from "node:crypto";
import { NextFunction, Request, Response } from "express";
import { env } from "../config/env.config";
import { HTTP_STATUS } from "../constants/httpStatus";
import { ROLES } from "../constants/roles";
import { UserServiceClient } from "../integrations/user.service";
import { ApiError } from "../utils/apiError";

const userServiceClient = new UserServiceClient();
const roleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

type JwtPayload = {
  sub?: string;
  userId?: string;
  email?: string;
  exp?: number;
  nbf?: number;
  iss?: string;
  aud?: string | string[];
  role?: string | string[];
  [roleClaimType]?: string | string[];
  [key: string]: unknown;
};

const getBearerToken = (req: Request) => {
  const authHeader = req.headers.authorization;

  if (!authHeader?.startsWith("Bearer ")) {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Missing bearer token");
  }

  return authHeader.slice("Bearer ".length).trim();
};

const decodeBase64Url = (value: string) => {
  const normalized = value.replace(/-/g, "+").replace(/_/g, "/");
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "=");
  return Buffer.from(padded, "base64");
};

const encodeBase64Url = (value: Buffer) => {
  return value
    .toString("base64")
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/g, "");
};

const verifyJwt = (token: string) => {
  if (!env.JWT_SECRET) {
    throw new ApiError(
      HTTP_STATUS.INTERNAL_SERVER_ERROR,
      "JWT_SECRET is missing in Catalog Service environment"
    );
  }

  const [encodedHeader, encodedPayload, signature] = token.split(".");

  if (!encodedHeader || !encodedPayload || !signature) {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid token format");
  }

  const header = JSON.parse(decodeBase64Url(encodedHeader).toString("utf8")) as {
    alg?: string;
    typ?: string;
  };

  if (header.alg !== "HS256") {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Unsupported token algorithm");
  }

  const expectedSignature = encodeBase64Url(
    crypto
      .createHmac("sha256", env.JWT_SECRET)
      .update(`${encodedHeader}.${encodedPayload}`)
      .digest()
  );

  const isValidSignature = crypto.timingSafeEqual(
    Buffer.from(signature),
    Buffer.from(expectedSignature)
  );

  if (!isValidSignature) {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid token signature");
  }

  const payload = JSON.parse(
    decodeBase64Url(encodedPayload).toString("utf8")
  ) as JwtPayload;

  const now = Math.floor(Date.now() / 1000);

  if (payload.exp && payload.exp < now) {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Token has expired");
  }

  if (payload.nbf && payload.nbf > now) {
    throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Token is not active yet");
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
  const claimRoles = payload[roleClaimType];
  const roles = [payload.role, claimRoles].flatMap((value) =>
    Array.isArray(value) ? value : value ? [value] : []
  );

  return [...new Set(roles.map((role) => String(role).toUpperCase()))];
};

export const authenticate = (
  req: Request,
  _res: Response,
  next: NextFunction
) => {
  try {
    const token = getBearerToken(req);
    const payload = verifyJwt(token);
    const userId = payload.sub ?? payload.userId;

    if (!userId) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "User id claim is missing");
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

export const requireRoles = (...roles: string[]) => {
  const normalizedRequiredRoles = roles.map((role) => role.toUpperCase());

  return (req: Request, _res: Response, next: NextFunction) => {
    try {
      if (!req.auth) {
        throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Authentication required");
      }

      const hasRole = req.auth.roles.some((role) =>
        normalizedRequiredRoles.includes(role)
      );

      if (!hasRole) {
        throw new ApiError(HTTP_STATUS.FORBIDDEN, "You do not have permission");
      }

      next();
    } catch (error) {
      next(error);
    }
  };
};

export const attachMerchantContext = async (
  req: Request,
  _res: Response,
  next: NextFunction
) => {
  try {
    if (!req.auth) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Authentication required");
    }

    if (req.auth.roles.includes(ROLES.ADMIN)) {
      return next();
    }

    if (!req.auth.roles.includes(ROLES.MERCHANT)) {
      throw new ApiError(
        HTTP_STATUS.FORBIDDEN,
        "Merchant role is required for this action"
      );
    }

    const merchant = await userServiceClient.getMerchantByUserId(
      req.auth.userId,
      req.auth.token
    );

    req.auth.merchantId = merchant.id;

    next();
  } catch (error) {
    next(error);
  }
};
