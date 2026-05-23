export type OrderCompletedEventPayload = {
  OrderId: string;
  OrderNumber: string;
  OrderStatus: string;
  MerchantId: string;
  MerchantStoreName: string;
  MerchantAddress: {
    AddressLine: string;
    Lat: number;
    Lng: number;
  };
  UserId: string;
  CustomerName: string;
  CustomerPhone: string;
  DeliveryAddress: {
    AddressLine: string;
    Lat: number;
    Lng: number;
  };
  TotalAmount: number;
  PaymentMethod: string;
  Note?: string | null;
};

export type DeliveryMilestoneEventPayload = {
  OrderId?: string;
  OrderNumber?: string;
  CustomerId?: string;
  ShipperId?: string;
  Milestone?: "PickedUp" | "Delivered" | string;
  ProofFileKey?: string | null;
  Note?: string | null;
  orderId?: string;
  orderNumber?: string;
  customerId?: string;
  shipperId?: string;
  milestone?: "PickedUp" | "Delivered" | string;
  proofFileKey?: string | null;
  note?: string | null;
};
