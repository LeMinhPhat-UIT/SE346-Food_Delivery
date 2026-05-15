export const ROLES = {
  ADMIN: "Admin",
  MERCHANT: "Merchant",
  CUSTOMER: "Customer",
} as const;

export type Role = (typeof ROLES)[keyof typeof ROLES];
