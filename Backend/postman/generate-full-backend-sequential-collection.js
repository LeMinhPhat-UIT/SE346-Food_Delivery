const fs = require("fs");
const path = require("path");

const collectionName = "Food Delivery - Full Backend Sequential Flow";
const outputFile = path.join(__dirname, "full-backend-sequential.collection.json");

function script(code) {
  return {
    type: "text/javascript",
    exec: code.trim().split(/\r?\n/),
  };
}

function event(listen, code) {
  return { listen, script: script(code) };
}

function variable(key, value, description) {
  return { key, value, description };
}

function folder(name, items, description) {
  return { name, description, item: items };
}

function request(name, method, url, options = {}) {
  const headers = [];

  if (options.auth) {
    headers.push({
      key: "Authorization",
      value: `Bearer {{${options.auth}}}`,
      type: "text",
    });
  }

  if (options.json !== undefined || options.raw !== undefined) {
    headers.push({ key: "Content-Type", value: "application/json", type: "text" });
  }

  if (options.headers) {
    headers.push(...options.headers.map(([key, value]) => ({ key, value, type: "text" })));
  }

  const req = {
    method,
    header: headers,
    url,
  };

  if (options.json !== undefined) {
    req.body = {
      mode: "raw",
      raw: JSON.stringify(options.json, null, 2),
    };
  }

  if (options.raw !== undefined) {
    req.body = {
      mode: "raw",
      raw: options.raw,
    };
  }

  if (options.formdata) {
    req.body = {
      mode: "formdata",
      formdata: options.formdata,
    };
  }

  const item = {
    name,
    request: req,
  };

  if (options.description) {
    item.description = options.description;
  }

  if (options.tests || options.prerequest) {
    item.event = [];
    if (options.prerequest) item.event.push(event("prerequest", options.prerequest));
    if (options.tests) item.event.push(event("test", options.tests));
  }

  return item;
}

function capture(customCode) {
  return `
function readJson() {
  try { return pm.response.json(); } catch (error) { return null; }
}
function prop(obj, key) {
  if (!obj || typeof obj !== "object") return undefined;
  if (Object.prototype.hasOwnProperty.call(obj, key)) return obj[key];
  const pascal = key.charAt(0).toUpperCase() + key.slice(1);
  if (Object.prototype.hasOwnProperty.call(obj, pascal)) return obj[pascal];
  const camel = key.charAt(0).toLowerCase() + key.slice(1);
  if (Object.prototype.hasOwnProperty.call(obj, camel)) return obj[camel];
  return undefined;
}
function get(obj, path) {
  return String(path).split(".").reduce((current, key) => {
    if (current === undefined || current === null) return undefined;
    if (/^\\d+$/.test(key)) return current[Number(key)];
    return prop(current, key);
  }, obj);
}
function dataOf(body) {
  if (!body) return null;
  return prop(body, "data") ?? body;
}
function itemsOf(body) {
  const data = dataOf(body);
  return prop(data, "items") ?? prop(data, "Items") ?? [];
}
function setVar(name, value) {
  if (value !== undefined && value !== null && value !== "") {
    pm.collectionVariables.set(name, String(value));
  }
}
if (pm.response.code >= 200 && pm.response.code < 300) {
  const body = readJson();
  const data = dataOf(body);
${customCode.split(/\r?\n/).map((line) => `  ${line}`).join("\n")}
}
`;
}

function captureToken(prefix) {
  return capture(`
setVar("${prefix}Token", get(data, "accessToken"));
setVar("${prefix}RefreshToken", get(data, "refreshToken"));
setVar("${prefix}UserIdFromLogin", get(data, "userId"));
`);
}

function captureId(varName) {
  return capture(`setVar("${varName}", get(data, "id"));`);
}

function captureListItemId(varName, predicateCode) {
  return capture(`
const items = itemsOf(body);
const match = items.find((item) => {
${predicateCode.split(/\r?\n/).map((line) => `  ${line}`).join("\n")}
}) || items[0];
setVar("${varName}", get(match, "id"));
`);
}

function requireVariables(...names) {
  return `
const missing = ${JSON.stringify(names)}.filter((name) => !pm.collectionVariables.get(name));
if (missing.length > 0) {
  throw new Error("Missing required collection variable(s): " + missing.join(", "));
}
`;
}

function waitForAsyncProcessing(milliseconds) {
  return `
const waitUntil = Date.now() + ${milliseconds};
while (Date.now() < waitUntil) {}
`;
}

const collectionPrerequest = `
const shouldReset = pm.info.requestName === "Initialize generated run data";
if (shouldReset || !pm.collectionVariables.get("runId")) {
  const now = Date.now();
  const suffix = String(now).slice(-8);
  pm.collectionVariables.set("runId", String(now));
  pm.collectionVariables.set("reviewId", "");
  pm.collectionVariables.set("activeOfferAssignmentId", "");
  pm.collectionVariables.set("activeOfferOrderId", "");
  pm.collectionVariables.set("tempCustomerEmail", "fullflow." + now + "@example.local");
  pm.collectionVariables.set("tempCustomerPhone", "09" + suffix);
  pm.collectionVariables.set("categoryName", "Full Flow Category " + now);
  pm.collectionVariables.set("productName", "Full Flow Product " + now);
  pm.collectionVariables.set("voucherCode", "FF" + suffix);
  pm.collectionVariables.set("tempUserAddressLabel", "Full Flow Temp User Address " + now);
  pm.collectionVariables.set("tempMerchantAddressLine", "Full Flow Merchant Address " + now);
  pm.collectionVariables.set("tempIncidentDescription", "Full Flow temp incident " + now);
  pm.collectionVariables.set("tempMerchantStoreName", "Full Flow Temp Store " + now);
  pm.collectionVariables.set("tempBusinessLicense", "BL-FF-" + suffix);
  pm.collectionVariables.set("tempShipperLicense", "DL-FF-" + suffix);
  pm.collectionVariables.set("voucherStartDate", new Date(now - 86400000).toISOString());
  pm.collectionVariables.set("voucherEndDate", new Date(now + 7 * 86400000).toISOString());
}
`;

const collectionTest = `
const strict2xx = pm.collectionVariables.get("strict2xx") === "true";
pm.test(strict2xx ? "HTTP status is 2xx" : "HTTP status is below 500", function () {
  if (strict2xx) {
    pm.expect(pm.response.code).to.be.within(200, 299);
  } else {
    pm.expect(pm.response.code).to.be.below(500);
  }
});

const contentType = pm.response.headers.get("content-type") || "";
if (pm.response.code >= 200 && pm.response.code < 300 && contentType.includes("application/json")) {
  const body = pm.response.json();
  const envelopeSuccess =
    body.ok !== undefined ? body.ok :
    body.success !== undefined ? body.success :
    body.Success !== undefined ? body.Success :
    undefined;

  if (envelopeSuccess !== undefined) {
    pm.test("Success envelope is true on 2xx", function () {
      pm.expect(envelopeSuccess).to.eql(true);
    });
  }
}
`;

const variables = [
  variable("strict2xx", "false", "Set true after filling OTP, upload, VNPay, realtime, and temp-resource variables."),
  variable("gatewayBaseUrl", "http://localhost:8080", "ApiGateway base URL."),
  variable("authBaseUrl", "http://localhost:8081", "Direct AuthenticationService URL."),
  variable("userBaseUrl", "http://localhost:8082", "Direct UserService URL."),
  variable("notificationBaseUrl", "http://localhost:8083", "Direct NotificationService URL."),
  variable("deliveryBaseUrl", "http://localhost:8084", "Direct DeliveryService URL."),
  variable("catalogBaseUrl", "http://localhost:8085", "Direct CatalogService URL."),
  variable("orderBaseUrl", "http://localhost:8086", "Direct OrderService URL."),
  variable("fileBaseUrl", "http://localhost:8087", "Direct FileService URL."),
  variable("reportBaseUrl", "http://localhost:8088", "Direct ReportService URL."),
  variable("walletBaseUrl", "http://localhost:8089", "Direct WalletService URL."),
  variable("chatBaseUrl", "http://localhost:8090", "Direct ChatService URL."),
  variable("addressBaseUrl", "http://localhost:8091", "Direct AddressService URL."),
  variable("rabbitmqManagementUrl", "http://localhost:15672", "RabbitMQ management UI."),
  variable("deviceId", "postman-full-backend-device", "Login device id prefix."),
  variable("deviceName", "postman-full-backend", "Login device name."),
  variable("seedPassword", "Admin@123", "Password for seeded accounts."),
  variable("adminEmail", "admin@fooddelivery.local"),
  variable("customerEmail", "customer@fooddelivery.local"),
  variable("merchantEmail", "merchant@fooddelivery.local"),
  variable("shipperEmail", "shipper@fooddelivery.local"),
  variable("adminUserId", "55555555-5555-4555-8555-555555555555"),
  variable("customerUserId", "aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa"),
  variable("merchantUserId", "bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb"),
  variable("shipperUserId", "99999999-9999-4999-9999-999999999999"),
  variable("merchantId", "cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
  variable("shipperId", "56565656-5656-4656-8656-565656565656"),
  variable("customerAddressId", "dddddddd-dddd-4ddd-8ddd-dddddddddddd"),
  variable("merchantAddressId", "ffffffff-ffff-4fff-8fff-ffffffffffff"),
  variable("orderId", "62222222-2222-4222-8222-222222222222"),
  variable("deliveredOrderId", "75555555-5555-4555-8555-555555555555"),
  variable("assignmentId", "64444444-4444-4444-8444-444444444444"),
  variable("incidentId", "77777777-7777-4777-8777-777777777777"),
  variable("feePolicyId", "81111111-1111-4111-8111-111111111111"),
  variable("provinceCode", "01"),
  variable("wardCode", "00004"),
  variable("adminToken", ""),
  variable("customerToken", ""),
  variable("merchantToken", ""),
  variable("shipperToken", ""),
  variable("tempUserToken", ""),
  variable("adminRefreshToken", ""),
  variable("customerRefreshToken", ""),
  variable("merchantRefreshToken", ""),
  variable("shipperRefreshToken", ""),
  variable("runId", ""),
  variable("tempCustomerEmail", ""),
  variable("tempCustomerPhone", ""),
  variable("tempOtp", "paste-otp-from-db-email-or-log"),
  variable("resetOtp", "paste-reset-otp-from-db-email-or-log"),
  variable("resetToken", "paste-reset-token-from-verify-reset-otp"),
  variable("tempCustomerUserId", ""),
  variable("tempMerchantRequestId", ""),
  variable("tempMerchantId", ""),
  variable("tempShipperRequestId", ""),
  variable("tempShipperId", ""),
  variable("tempAddressId", ""),
  variable("tempMerchantAddressId", ""),
  variable("tempFeePolicyId", ""),
  variable("tempIncidentId", ""),
  variable("categoryId", ""),
  variable("productId", ""),
  variable("productOptionId", ""),
  variable("productOptionValueId", ""),
  variable("voucherId", ""),
  variable("cartItemId", ""),
  variable("cancelOrderId", ""),
  variable("vnpayOrderId", ""),
  variable("vnpayOrderNumber", ""),
  variable("paymentId", ""),
  variable("vnpayPaymentUrl", ""),
  variable("reviewId", ""),
  variable("conversationId", ""),
  variable("messageId", ""),
  variable("walletId", ""),
  variable("walletTopupId", ""),
  variable("walletTopupRequestCode", ""),
  variable("walletTopupPaymentUrl", ""),
  variable("activeOfferAssignmentId", ""),
  variable("activeOfferOrderId", ""),
  variable("userFileKey", "users/{{customerUserId}}/paste-uploaded-file-name.png"),
  variable("deliveryFileKey", "deliveries/{{orderId}}/{{shipperId}}/pickup/paste-uploaded-file-name.jpg"),
  variable("catalogUploadPath", "product/general/paste-uploaded-file.png"),
  variable("catalogUploadPublicUrl", "https://example.com/full-flow-upload.png"),
  variable("categoryName", ""),
  variable("productName", ""),
  variable("voucherCode", ""),
  variable("voucherStartDate", ""),
  variable("voucherEndDate", ""),
  variable("tempUserAddressLabel", ""),
  variable("tempMerchantAddressLine", ""),
  variable("tempIncidentDescription", ""),
  variable("tempMerchantStoreName", ""),
  variable("tempBusinessLicense", ""),
  variable("tempShipperLicense", ""),
];

const setup = folder("00 - Setup, Health, OpenAPI", [
  request("Initialize generated run data", "GET", "{{gatewayBaseUrl}}/openapi/v1.json", {
    description: "Resets generated names/emails/codes for this collection run. A 404 here usually means gateway OpenAPI is disabled; variables are still initialized by the pre-request script.",
  }),
  request("Gateway OpenAPI", "GET", "{{gatewayBaseUrl}}/openapi/v1.json"),
  request("Catalog Health", "GET", "{{catalogBaseUrl}}/health"),
  request("Order Health", "GET", "{{orderBaseUrl}}/health"),
  request("Report Health Direct", "GET", "{{reportBaseUrl}}/health"),
  request("Report Health Via Gateway", "GET", "{{gatewayBaseUrl}}/api/reports/health"),
  request("Wallet Health Direct", "GET", "{{walletBaseUrl}}/health"),
  request("Wallet Health Via Gateway", "GET", "{{gatewayBaseUrl}}/api/wallets/health"),
  request("Chat Health", "GET", "{{chatBaseUrl}}/health"),
  request("Catalog OpenAPI", "GET", "{{catalogBaseUrl}}/openapi.json"),
  request("Order OpenAPI", "GET", "{{orderBaseUrl}}/openapi.json"),
  request("Report OpenAPI", "GET", "{{reportBaseUrl}}/openapi.json"),
  request("Wallet OpenAPI", "GET", "{{walletBaseUrl}}/openapi.json"),
  request("Chat OpenAPI", "GET", "{{chatBaseUrl}}/openapi.json"),
], "Service health and documentation endpoints. Most business API requests below use the gateway URL.");

const auth = folder("01 - Authentication", [
  request("Login Admin", "POST", "{{gatewayBaseUrl}}/api/auth/login", {
    headers: [["X-Device-Id", "{{deviceId}}-admin"], ["X-Device-Name", "{{deviceName}}"]],
    json: { email: "{{adminEmail}}", password: "{{seedPassword}}" },
    tests: captureToken("admin"),
  }),
  request("Login Customer", "POST", "{{gatewayBaseUrl}}/api/auth/login", {
    headers: [["X-Device-Id", "{{deviceId}}-customer"], ["X-Device-Name", "{{deviceName}}"]],
    json: { email: "{{customerEmail}}", password: "{{seedPassword}}" },
    tests: captureToken("customer"),
  }),
  request("Login Merchant", "POST", "{{gatewayBaseUrl}}/api/auth/login", {
    headers: [["X-Device-Id", "{{deviceId}}-merchant"], ["X-Device-Name", "{{deviceName}}"]],
    json: { email: "{{merchantEmail}}", password: "{{seedPassword}}" },
    tests: captureToken("merchant"),
  }),
  request("Login Shipper", "POST", "{{gatewayBaseUrl}}/api/auth/login", {
    headers: [["X-Device-Id", "{{deviceId}}-shipper"], ["X-Device-Name", "{{deviceName}}"]],
    json: { email: "{{shipperEmail}}", password: "{{seedPassword}}" },
    tests: captureToken("shipper"),
  }),
  request("Get Customer Roles", "GET", "{{gatewayBaseUrl}}/api/auth/users/{{customerUserId}}/roles", {
    auth: "customerToken",
  }),
  request("Get Merchant Roles As Admin", "GET", "{{gatewayBaseUrl}}/api/auth/users/{{merchantUserId}}/roles", {
    auth: "adminToken",
  }),
  request("Refresh Customer Token", "POST", "{{gatewayBaseUrl}}/api/auth/refresh-token", {
    headers: [["X-Device-Id", "{{deviceId}}-customer"]],
    json: { refreshToken: "{{customerRefreshToken}}" },
    tests: captureToken("customer"),
  }),
  request("Register Temp Customer", "POST", "{{gatewayBaseUrl}}/api/auth/register", {
    json: {
      email: "{{tempCustomerEmail}}",
      password: "{{seedPassword}}",
      fullName: "Full Flow Temp Customer",
      phoneNumber: "{{tempCustomerPhone}}",
    },
    tests: capture(`
setVar("tempCustomerUserId", get(data, "userId"));
`),
  }),
  request("Resend Temp Customer OTP", "POST", "{{gatewayBaseUrl}}/api/auth/resend-otp", {
    json: { email: "{{tempCustomerEmail}}" },
  }),
  request("Verify Temp Customer OTP", "POST", "{{gatewayBaseUrl}}/api/auth/verify-otp", {
    json: { email: "{{tempCustomerEmail}}", otp: "{{tempOtp}}" },
    description: "Requires the real OTP from email, auth DB, or service logs.",
  }),
  request("Login Temp Customer", "POST", "{{gatewayBaseUrl}}/api/auth/login", {
    headers: [["X-Device-Id", "{{deviceId}}-temp"], ["X-Device-Name", "{{deviceName}}"]],
    json: { email: "{{tempCustomerEmail}}", password: "{{seedPassword}}" },
    tests: captureToken("tempUser"),
    description: "Runs after temp OTP verification succeeds.",
  }),
  request("Forgot Password", "POST", "{{gatewayBaseUrl}}/api/auth/forgot-password", {
    json: { email: "{{tempCustomerEmail}}" },
  }),
  request("Verify Reset OTP", "POST", "{{gatewayBaseUrl}}/api/auth/verify-reset-otp", {
    json: { email: "{{tempCustomerEmail}}", otp: "{{resetOtp}}" },
    tests: capture(`
setVar("resetToken", get(data, "resetToken"));
`),
    description: "Requires the real reset OTP from email, auth DB, or service logs.",
  }),
  request("Reset Password", "POST", "{{gatewayBaseUrl}}/api/auth/reset-password", {
    json: {
      email: "{{tempCustomerEmail}}",
      resetToken: "{{resetToken}}",
      newPassword: "{{seedPassword}}",
      confirmPassword: "{{seedPassword}}",
    },
  }),
  request("Change Password Temp User", "POST", "{{gatewayBaseUrl}}/api/auth/change-password", {
    auth: "tempUserToken",
    json: {
      currentPassword: "{{seedPassword}}",
      newPassword: "{{seedPassword}}",
      confirmPassword: "{{seedPassword}}",
    },
    description: "No-op style request for route coverage; use a temp user, not a seeded user.",
  }),
], "Authentication setup plus OTP/reset routes. OTP values cannot be automated from HTTP alone.");

const address = folder("02 - Address Service", [
  request("Get Provinces", "GET", "{{gatewayBaseUrl}}/api/addresses/provinces?pageIndex=1&pageSize=10"),
  request("Get Provinces Search Query", "GET", "{{gatewayBaseUrl}}/api/addresses/provinces?search=Ha&pageIndex=1&pageSize=10"),
  request("Search Provinces", "GET", "{{gatewayBaseUrl}}/api/addresses/provinces/search?key=Ha&pageIndex=1&pageSize=10"),
  request("Get Province By Code", "GET", "{{gatewayBaseUrl}}/api/addresses/provinces/{{provinceCode}}"),
  request("Get Wards By Province", "GET", "{{gatewayBaseUrl}}/api/addresses/provinces/{{provinceCode}}/wards?pageIndex=1&pageSize=10"),
  request("Get Wards By Province Search", "GET", "{{gatewayBaseUrl}}/api/addresses/provinces/{{provinceCode}}/wards?search=Ba&pageIndex=1&pageSize=10"),
  request("Search Wards", "GET", "{{gatewayBaseUrl}}/api/addresses/wards/search?key=Ba&pageIndex=1&pageSize=10"),
  request("Get Ward By Code", "GET", "{{gatewayBaseUrl}}/api/addresses/wards/{{wardCode}}"),
  request("Get Province By Ward", "GET", "{{gatewayBaseUrl}}/api/addresses/wards/{{wardCode}}/province"),
  request("Resolve Address", "POST", "{{gatewayBaseUrl}}/api/addresses/resolve", {
    json: {
      provinceCode: "{{provinceCode}}",
      wardCode: "{{wardCode}}",
      addressLine: "1 Full Flow Street",
    },
  }),
]);

const users = folder("03 - User, Merchant, Shipper Service", [
  request("Admin Get All Users", "GET", "{{gatewayBaseUrl}}/api/users?pageIndex=1&pageSize=10", { auth: "adminToken" }),
  request("Customer Get Own Profile", "GET", "{{gatewayBaseUrl}}/api/users/{{customerUserId}}", { auth: "customerToken" }),
  request("Customer Update Own Profile", "PUT", "{{gatewayBaseUrl}}/api/users/{{customerUserId}}", {
    auth: "customerToken",
    json: {
      fullName: "Seeded Customer Full Flow",
      avatarUrl: "https://example.com/avatars/full-flow-customer.png",
      phoneNumber: "0900000001",
    },
  }),
  request("Customer Get Addresses", "GET", "{{gatewayBaseUrl}}/api/users/{{customerUserId}}/addresses?pageIndex=1&pageSize=50", { auth: "customerToken" }),
  request("Customer Get Seed Address", "GET", "{{gatewayBaseUrl}}/api/users/{{customerUserId}}/addresses/{{customerAddressId}}", { auth: "customerToken" }),
  request("Customer Add Temp Address", "POST", "{{gatewayBaseUrl}}/api/users/{{customerUserId}}/addresses", {
    auth: "customerToken",
    json: {
      addressLine: "10 Full Flow User Address",
      ward: "Ben Nghe",
      city: "Ho Chi Minh City",
      lat: 10.7769,
      lng: 106.7009,
      label: "{{tempUserAddressLabel}}",
      recipientName: "Seeded Customer",
      phone: "0900000001",
      isDefault: false,
    },
  }),
  request("Customer Capture Temp Address From List", "GET", "{{gatewayBaseUrl}}/api/users/{{customerUserId}}/addresses?pageIndex=1&pageSize=100", {
    auth: "customerToken",
    tests: captureListItemId("tempAddressId", `
const expected = pm.collectionVariables.get("tempUserAddressLabel");
return get(item, "label") === expected;
`),
  }),
  request("Customer Update Temp Address", "PUT", "{{gatewayBaseUrl}}/api/users/{{customerUserId}}/addresses/{{tempAddressId}}", {
    auth: "customerToken",
    json: {
      addressLine: "10 Full Flow User Address Updated",
      ward: "Ben Nghe",
      city: "Ho Chi Minh City",
      lat: 10.777,
      lng: 106.701,
      label: "{{tempUserAddressLabel}} Updated",
      recipientName: "Seeded Customer",
      phone: "0900000001",
      isDefault: false,
    },
  }),
  request("Temp User Request Merchant Role", "POST", "{{gatewayBaseUrl}}/api/merchants/requests", {
    auth: "tempUserToken",
    json: {
      storeName: "{{tempMerchantStoreName}}",
      storeDescription: "Store created by the full backend Postman flow.",
      businessLicense: "{{tempBusinessLicense}}",
      businessLicenseUrl: "https://example.com/licenses/full-flow.jpg",
      taxId: "TAX-{{runId}}",
    },
    description: "Requires tempUserToken from the OTP-verified temp account.",
  }),
  request("Admin Get Merchant Requests", "GET", "{{gatewayBaseUrl}}/api/merchants/requests?pageIndex=1&pageSize=100", {
    auth: "adminToken",
    tests: captureListItemId("tempMerchantRequestId", `
const expected = pm.collectionVariables.get("tempBusinessLicense");
return get(item, "businessLicense") === expected || get(item, "storeName") === pm.collectionVariables.get("tempMerchantStoreName");
`),
  }),
  request("Admin Review Temp Merchant Request", "PATCH", "{{gatewayBaseUrl}}/api/merchants/requests/{{tempMerchantRequestId}}/review", {
    auth: "adminToken",
    json: { verificationStatus: "Approved", rejectedReason: null },
  }),
  request("Get Merchants", "GET", "{{gatewayBaseUrl}}/api/merchants?pageIndex=1&pageSize=20", { auth: "customerToken" }),
  request("Get Seed Merchant By Id", "GET", "{{gatewayBaseUrl}}/api/merchants/{{merchantId}}", { auth: "customerToken" }),
  request("Get Merchant By User Alias", "GET", "{{gatewayBaseUrl}}/api/merchants/by-user/{{merchantUserId}}", { auth: "merchantToken" }),
  request("Get User Merchant Alias", "GET", "{{gatewayBaseUrl}}/api/users/{{merchantUserId}}/merchant", { auth: "merchantToken" }),
  request("Get Temp Merchant By Temp User", "GET", "{{gatewayBaseUrl}}/api/users/{{tempCustomerUserId}}/merchant", {
    auth: "adminToken",
    tests: captureId("tempMerchantId"),
  }),
  request("Get Merchant Location", "GET", "{{gatewayBaseUrl}}/api/merchants/{{merchantId}}/location?pageIndex=1&pageSize=10", { auth: "merchantToken" }),
  request("Get Merchant Addresses", "GET", "{{gatewayBaseUrl}}/api/merchants/{{merchantId}}/addresses?pageIndex=1&pageSize=50", { auth: "merchantToken" }),
  request("Get Seed Merchant Address", "GET", "{{gatewayBaseUrl}}/api/merchants/{{merchantId}}/addresses/{{merchantAddressId}}", { auth: "merchantToken" }),
  request("Add Temp Merchant Address", "POST", "{{gatewayBaseUrl}}/api/merchants/{{merchantId}}/addresses", {
    auth: "merchantToken",
    json: {
      addressLine: "{{tempMerchantAddressLine}}",
      ward: "Ben Thanh",
      district: "District 1",
      city: "Ho Chi Minh City",
      lat: 10.7722,
      lng: 106.6983,
    },
  }),
  request("Capture Temp Merchant Address From List", "GET", "{{gatewayBaseUrl}}/api/merchants/{{merchantId}}/addresses?pageIndex=1&pageSize=100", {
    auth: "merchantToken",
    tests: captureListItemId("tempMerchantAddressId", `
return get(item, "addressLine") === pm.collectionVariables.get("tempMerchantAddressLine");
`),
  }),
  request("Update Temp Merchant Address", "PUT", "{{gatewayBaseUrl}}/api/merchants/{{merchantId}}/addresses/{{tempMerchantAddressId}}", {
    auth: "merchantToken",
    json: {
      addressLine: "{{tempMerchantAddressLine}} Updated",
      ward: "Ben Thanh",
      district: "District 1",
      city: "Ho Chi Minh City",
      lat: 10.7724,
      lng: 106.6985,
    },
  }),
  request("Update Seed Merchant", "PUT", "{{gatewayBaseUrl}}/api/merchants/{{merchantId}}", {
    auth: "merchantToken",
    json: {
      storeName: "Seeded Merchant Store",
      storeDescription: "Default merchant store for local development.",
      storeLogoUrl: "https://example.com/stores/full-flow-logo.png",
      storeBannerUrl: "https://example.com/stores/full-flow-banner.png",
      businessLicense: "BL-SEED-0001",
      taxId: "TAX-SEED-0001",
      isOpen: true,
      openingTime: "08:00:00",
      closingTime: "22:00:00",
      minOrderAmount: 30000,
      avgPrepTime: 20,
      status: "Approved",
    },
  }),
  request("Temp User Request Shipper Role", "POST", "{{gatewayBaseUrl}}/api/shippers/requests", {
    auth: "tempUserToken",
    json: {
      licenseNumber: "{{tempShipperLicense}}",
      licenseFrontUrl: "https://example.com/shipper/license-front.jpg",
      licenseBackUrl: "https://example.com/shipper/license-back.jpg",
      idCardFrontUrl: "https://example.com/shipper/id-front.jpg",
      idCardBackUrl: "https://example.com/shipper/id-back.jpg",
      selfieUrl: "https://example.com/shipper/selfie.jpg",
      idNumber: "079202600099",
      fullName: "Full Flow Temp Shipper",
      dateOfBirth: "1998-04-12T00:00:00Z",
    },
    description: "Requires tempUserToken from the OTP-verified temp account.",
  }),
  request("Admin Get Shipper Requests", "GET", "{{gatewayBaseUrl}}/api/shippers/requests?pageIndex=1&pageSize=100", {
    auth: "adminToken",
    tests: captureListItemId("tempShipperRequestId", `
return get(item, "licenseNumber") === pm.collectionVariables.get("tempShipperLicense");
`),
  }),
  request("Admin Review Temp Shipper Request", "PATCH", "{{gatewayBaseUrl}}/api/shippers/requests/{{tempShipperRequestId}}/review", {
    auth: "adminToken",
    json: { verificationStatus: "Approved", rejectedReason: null },
  }),
  request("Get Shippers", "GET", "{{gatewayBaseUrl}}/api/shippers?pageIndex=1&pageSize=20"),
  request("Get Seed Shipper By Id", "GET", "{{gatewayBaseUrl}}/api/shippers/{{shipperId}}"),
  request("Get Shipper By User Alias", "GET", "{{gatewayBaseUrl}}/api/shippers/by-user/{{shipperUserId}}", { auth: "shipperToken" }),
  request("Get User Shipper Alias", "GET", "{{gatewayBaseUrl}}/api/users/{{shipperUserId}}/shipper", { auth: "shipperToken" }),
  request("Get Temp Shipper By Temp User", "GET", "{{gatewayBaseUrl}}/api/users/{{tempCustomerUserId}}/shipper", {
    auth: "adminToken",
    tests: captureId("tempShipperId"),
  }),
  request("Update Seed Shipper", "PUT", "{{gatewayBaseUrl}}/api/shippers/{{shipperId}}", {
    auth: "shipperToken",
    json: { vehiclePlate: "59A-123.45" },
  }),
], "Includes alias routes. Temp onboarding requires manual OTP completion first.");

const filesNotifications = folder("04 - Files And Notifications", [
  request("User File Upload URL", "GET", "{{gatewayBaseUrl}}/api/files/get-upload-url?fileName=avatar.png&contentType=image/png", {
    auth: "customerToken",
    tests: capture(`
setVar("userFileKey", get(data, "fileKey"));
setVar("userUploadUrl", get(data, "uploadUrl"));
`),
  }),
  request("User File Read URL", "GET", "{{gatewayBaseUrl}}/api/files/get-read-url?fileKey={{userFileKey}}", {
    auth: "customerToken",
    tests: capture(`setVar("userReadUrl", get(data, "readUrl"));`),
  }),
  request("Delivery File Upload URL", "GET", "{{gatewayBaseUrl}}/api/deliveries/files/upload-url?orderId={{orderId}}&shipperId={{shipperId}}&stage=pickup&fileName=proof.jpg&contentType=image/jpeg", {
    auth: "shipperToken",
    tests: capture(`
setVar("deliveryFileKey", get(data, "fileKey"));
setVar("deliveryUploadUrl", get(data, "uploadUrl"));
`),
  }),
  request("Delivery File Read URL", "GET", "{{gatewayBaseUrl}}/api/deliveries/files/read-url?fileKey={{deliveryFileKey}}", {
    auth: "shipperToken",
    tests: capture(`setVar("deliveryReadUrl", get(data, "readUrl"));`),
  }),
  request("Register Notification Device", "POST", "{{gatewayBaseUrl}}/api/notifications/devices", {
    auth: "customerToken",
    json: { deviceToken: "full-flow-device-token-{{runId}}", deviceType: "Web" },
  }),
  request("Admin Get All Notification Devices", "GET", "{{gatewayBaseUrl}}/api/notifications/devices?pageIndex=1&pageSize=20", { auth: "adminToken" }),
  request("Customer Get Own Devices", "GET", "{{gatewayBaseUrl}}/api/notifications/users/{{customerUserId}}/devices?pageIndex=1&pageSize=20", { auth: "customerToken" }),
  request("Unregister Notification Device", "DELETE", "{{gatewayBaseUrl}}/api/notifications/devices", {
    auth: "customerToken",
    json: { deviceToken: "full-flow-device-token-{{runId}}" },
  }),
]);

const catalog = folder("05 - Catalog Service", [
  request("List Categories", "GET", "{{gatewayBaseUrl}}/api/catalog/categories?page=1&limit=20&sortBy=sortOrder&sortOrder=asc"),
  request("Get Category Tree", "GET", "{{gatewayBaseUrl}}/api/catalog/categories/tree"),
  request("Get Root Categories", "GET", "{{gatewayBaseUrl}}/api/catalog/categories/root"),
  request("Create Category", "POST", "{{gatewayBaseUrl}}/api/catalog/categories", {
    auth: "adminToken",
    json: {
      name: "{{categoryName}}",
      description: "Generated by the full backend Postman flow.",
      iconUrl: "https://example.com/icons/full-flow-category.png",
      parentId: null,
      sortOrder: 10,
      isActive: true,
    },
    tests: captureId("categoryId"),
  }),
  request("Get Category By Id", "GET", "{{gatewayBaseUrl}}/api/catalog/categories/{{categoryId}}"),
  request("Update Category PUT", "PUT", "{{gatewayBaseUrl}}/api/catalog/categories/{{categoryId}}", {
    auth: "adminToken",
    json: {
      name: "{{categoryName}} Updated",
      description: "Updated by the full backend Postman flow.",
      iconUrl: "https://example.com/icons/full-flow-category-updated.png",
      sortOrder: 11,
      isActive: true,
    },
  }),
  request("Update Category PATCH", "PATCH", "{{gatewayBaseUrl}}/api/catalog/categories/{{categoryId}}", {
    auth: "adminToken",
    json: { description: "Patched by the full backend Postman flow." },
  }),
  request("Update Category Status", "PATCH", "{{gatewayBaseUrl}}/api/catalog/categories/{{categoryId}}/status", {
    auth: "adminToken",
    json: { isActive: true },
  }),
  request("Create Product", "POST", "{{gatewayBaseUrl}}/api/catalog/products", {
    auth: "merchantToken",
    json: {
      merchantId: "{{merchantId}}",
      categoryId: "{{categoryId}}",
      name: "{{productName}}",
      description: "Generated product for full backend flow.",
      imageUrl: "https://example.com/products/full-flow.png",
      basePrice: 45000,
      taxonomy: "FOOD",
      discountPrice: null,
      isAvailable: true,
      isFeatured: false,
      prepTime: 15,
      options: [],
    },
    tests: capture(`
setVar("productId", get(data, "id"));
const options = get(data, "options") || [];
setVar("productOptionId", get(options[0], "id"));
setVar("productOptionValueId", get(options[0], "values.0.id"));
`),
  }),
  request("List Products", "GET", "{{gatewayBaseUrl}}/api/catalog/products?page=1&limit=20&merchantId={{merchantId}}&sortBy=createdAt&sortOrder=desc"),
  request("Get Product By Id", "GET", "{{gatewayBaseUrl}}/api/catalog/products/{{productId}}"),
  request("Get Product Detail", "GET", "{{gatewayBaseUrl}}/api/catalog/products/{{productId}}/detail"),
  request("Get My Merchant Products", "GET", "{{gatewayBaseUrl}}/api/catalog/products/merchant/me?page=1&limit=20&sortBy=createdAt&sortOrder=desc", {
    auth: "merchantToken",
  }),
  request("Create Product Option", "POST", "{{gatewayBaseUrl}}/api/catalog/products/{{productId}}/options", {
    auth: "merchantToken",
    json: {
      name: "Full Flow Size",
      isRequired: false,
      maxSelections: 1,
      values: [
        { name: "Regular", additionalPrice: 0, isAvailable: true },
        { name: "Large", additionalPrice: 8000, isAvailable: true },
      ],
    },
    tests: capture(`
setVar("productOptionId", get(data, "id"));
setVar("productOptionValueId", get(data, "values.0.id"));
`),
  }),
  request("Update Product Option", "PUT", "{{gatewayBaseUrl}}/api/catalog/products/options/{{productOptionId}}", {
    auth: "merchantToken",
    json: {
      name: "Full Flow Size Updated",
      isRequired: false,
      maxSelections: 1,
      values: [
        { name: "Regular", additionalPrice: 0, isAvailable: true },
        { name: "Large", additionalPrice: 9000, isAvailable: true },
      ],
    },
  }),
  request("Update Product Availability", "PATCH", "{{gatewayBaseUrl}}/api/catalog/products/{{productId}}/availability", {
    auth: "merchantToken",
    json: { isAvailable: true },
  }),
  request("Batch Update Product Availability", "PATCH", "{{gatewayBaseUrl}}/api/catalog/products/batch/availability", {
    auth: "merchantToken",
    json: { productIds: ["{{productId}}"], isAvailable: true },
  }),
  request("Update Product Featured", "PATCH", "{{gatewayBaseUrl}}/api/catalog/products/{{productId}}/featured", {
    auth: "merchantToken",
    json: { isFeatured: true },
  }),
  request("Update Product PUT", "PUT", "{{gatewayBaseUrl}}/api/catalog/products/{{productId}}", {
    auth: "merchantToken",
    json: {
      merchantId: "{{merchantId}}",
      categoryId: "{{categoryId}}",
      name: "{{productName}} Updated",
      description: "Updated product for full backend flow.",
      imageUrl: "https://example.com/products/full-flow-updated.png",
      basePrice: 47000,
      taxonomy: "FOOD",
      discountPrice: null,
      isAvailable: true,
      isFeatured: true,
      prepTime: 16,
    },
  }),
  request("Update Product PATCH", "PATCH", "{{gatewayBaseUrl}}/api/catalog/products/{{productId}}", {
    auth: "merchantToken",
    json: { description: "Patched product for full backend flow." },
  }),
  request("Catalog Upload Files", "POST", "{{gatewayBaseUrl}}/api/catalog/uploads", {
    auth: "merchantToken",
    formdata: [
      { key: "entityType", value: "product", type: "text" },
      { key: "entityId", value: "{{productId}}", type: "text" },
      { key: "files", type: "file", src: "" },
    ],
    tests: capture(`
const first = Array.isArray(data) ? data[0] : undefined;
setVar("catalogUploadPath", get(first, "path"));
setVar("catalogUploadPublicUrl", get(first, "publicUrl"));
`),
    description: "Choose a local file for the form-data 'files' field before running strictly.",
  }),
  request("Delete Catalog Uploaded File", "DELETE", "{{gatewayBaseUrl}}/api/catalog/uploads", {
    auth: "merchantToken",
    json: { paths: ["{{catalogUploadPath}}"] },
  }),
], "Creates a category and product first, then exercises all catalog read/write routes.");

const order = folder("06 - Order, Cart, Voucher, Payment Service", [
  request("List Vouchers", "GET", "{{gatewayBaseUrl}}/api/orders/vouchers?page=1&limit=20&sortBy=createdAt&sortOrder=desc", { auth: "customerToken" }),
  request("Create Voucher", "POST", "{{gatewayBaseUrl}}/api/orders/vouchers", {
    auth: "merchantToken",
    json: {
      code: "{{voucherCode}}",
      name: "Full Flow Voucher",
      description: "Generated by the full backend Postman flow.",
      discountType: "PERCENTAGE",
      discountValue: 10,
      maxDiscount: 15000,
      minOrderAmount: 0,
      discountTarget: "SUBTOTAL",
      merchantId: "{{merchantId}}",
      usageLimit: 50,
      perUserLimit: 1,
      startDate: "{{voucherStartDate}}",
      endDate: "{{voucherEndDate}}",
      isActive: true,
    },
    tests: captureId("voucherId"),
  }),
  request("Get Voucher By Id", "GET", "{{gatewayBaseUrl}}/api/orders/vouchers/{{voucherId}}", { auth: "customerToken" }),
  request("Get Voucher By Code", "GET", "{{gatewayBaseUrl}}/api/orders/vouchers/code/{{voucherCode}}", { auth: "customerToken" }),
  request("Validate Voucher", "POST", "{{gatewayBaseUrl}}/api/orders/vouchers/validate", {
    auth: "customerToken",
    json: {
      code: "{{voucherCode}}",
      userId: "{{customerUserId}}",
      merchantId: "{{merchantId}}",
      subtotal: 47000,
      deliveryFee: 15000,
    },
  }),
  request("Update Voucher Status", "PATCH", "{{gatewayBaseUrl}}/api/orders/vouchers/{{voucherId}}/status", {
    auth: "merchantToken",
    json: { isActive: true },
  }),
  request("Update Voucher PUT", "PUT", "{{gatewayBaseUrl}}/api/orders/vouchers/{{voucherId}}", {
    auth: "merchantToken",
    json: {
      name: "Full Flow Voucher Updated",
      description: "Updated by the full backend Postman flow.",
      discountType: "PERCENTAGE",
      discountValue: 12,
      maxDiscount: 20000,
      minOrderAmount: 0,
      discountTarget: "SUBTOTAL",
      usageLimit: 50,
      perUserLimit: 1,
      startDate: "{{voucherStartDate}}",
      endDate: "{{voucherEndDate}}",
      isActive: true,
    },
  }),
  request("Update Voucher PATCH", "PATCH", "{{gatewayBaseUrl}}/api/orders/vouchers/{{voucherId}}", {
    auth: "merchantToken",
    json: { description: "Patched by the full backend Postman flow." },
  }),
  request("Clear Cart By Merchant", "DELETE", "{{gatewayBaseUrl}}/api/orders/cart/merchant/{{merchantId}}", { auth: "customerToken" }),
  request("Get My Carts", "GET", "{{gatewayBaseUrl}}/api/orders/cart", { auth: "customerToken" }),
  request("Get Cart By Merchant", "GET", "{{gatewayBaseUrl}}/api/orders/cart/merchant/{{merchantId}}", { auth: "customerToken" }),
  request("Add Cart Item For Remove", "POST", "{{gatewayBaseUrl}}/api/orders/cart/items", {
    auth: "customerToken",
    json: { productId: "{{productId}}", quantity: 1, note: "Full flow remove item", selectedOptions: [] },
    tests: capture(`
const items = get(data, "items") || [];
const match = items.find((item) => get(item, "productId") === pm.collectionVariables.get("productId")) || items[0];
setVar("cartItemId", get(match, "id"));
`),
  }),
  request("Update Cart Item", "PATCH", "{{gatewayBaseUrl}}/api/orders/cart/items/{{cartItemId}}", {
    auth: "customerToken",
    json: { quantity: 2, note: "Full flow updated cart item", selectedOptions: [] },
  }),
  request("Remove Cart Item", "DELETE", "{{gatewayBaseUrl}}/api/orders/cart/items/{{cartItemId}}", { auth: "customerToken" }),
  request("Add Cart Item For COD Order", "POST", "{{gatewayBaseUrl}}/api/orders/cart/items", {
    auth: "customerToken",
    json: { productId: "{{productId}}", quantity: 1, note: "Full flow COD order", selectedOptions: [] },
    tests: capture(`
const items = get(data, "items") || [];
const match = items.find((item) => get(item, "productId") === pm.collectionVariables.get("productId")) || items[0];
setVar("cartItemId", get(match, "id"));
`),
  }),
  request("Checkout Preview COD", "POST", "{{gatewayBaseUrl}}/api/orders/checkout/preview", {
    auth: "customerToken",
    json: {
      merchantId: "{{merchantId}}",
      addressId: "{{customerAddressId}}",
      voucherCode: "{{voucherCode}}",
      paymentMethod: "COD",
    },
  }),
  request("Create COD Order", "POST", "{{gatewayBaseUrl}}/api/orders", {
    auth: "customerToken",
    json: {
      merchantId: "{{merchantId}}",
      addressId: "{{customerAddressId}}",
      voucherCode: "{{voucherCode}}",
      paymentMethod: "COD",
      note: "Full flow COD order",
    },
    tests: captureId("orderId"),
  }),
  request("Get My Orders", "GET", "{{gatewayBaseUrl}}/api/orders/my?page=1&limit=20&sortBy=createdAt&sortOrder=desc", { auth: "customerToken" }),
  request("Get Order By Id", "GET", "{{gatewayBaseUrl}}/api/orders/{{orderId}}", { auth: "customerToken" }),
  request("Get Merchant Orders", "GET", "{{gatewayBaseUrl}}/api/orders/merchant/my?page=1&limit=20&sortBy=createdAt&sortOrder=desc", { auth: "merchantToken" }),
  request("Get Merchant Order By Id", "GET", "{{gatewayBaseUrl}}/api/orders/merchant/my/{{orderId}}", { auth: "merchantToken" }),
  request("Merchant Confirm Order", "PATCH", "{{gatewayBaseUrl}}/api/orders/merchant/my/{{orderId}}/status", {
    auth: "merchantToken",
    json: { status: "CONFIRMED", note: "Full flow confirmed" },
  }),
  request("Merchant Preparing Order", "PATCH", "{{gatewayBaseUrl}}/api/orders/merchant/my/{{orderId}}/status", {
    auth: "merchantToken",
    json: { status: "PREPARING", note: "Full flow preparing" },
  }),
  request("Shipper Online Before Ready Order", "POST", "{{gatewayBaseUrl}}/api/deliveries/availability/toggle?shipperId={{shipperId}}", {
    auth: "shipperToken",
    json: { isGoOnline: true, lat: 10.7769, lng: 106.7009 },
  }),
  request("Merchant Ready Order", "PATCH", "{{gatewayBaseUrl}}/api/orders/merchant/my/{{orderId}}/status", {
    auth: "merchantToken",
    json: { status: "READY", note: "Full flow ready" },
  }),
  request("Get Active Offer For COD Order", "GET", "{{gatewayBaseUrl}}/api/deliveries/shippers/me/active-offer", {
    auth: "shipperToken",
    prerequest: `
pm.collectionVariables.set("activeOfferAssignmentId", "");
pm.collectionVariables.set("activeOfferOrderId", "");
${waitForAsyncProcessing(1500)}
`,
    tests: capture(`
const offerOrderId = get(data, "orderId");
if (get(data, "hasActiveOffer") && offerOrderId === pm.collectionVariables.get("orderId")) {
  const assignmentId = get(data, "assignmentId") || get(data, "offerId");
  setVar("activeOfferAssignmentId", assignmentId);
  setVar("assignmentId", assignmentId);
  setVar("activeOfferOrderId", offerOrderId);
}
`),
    description: "Captures the offer created from the COD order ready event.",
  }),
  request("Accept Active Offer For COD Order", "POST", "{{gatewayBaseUrl}}/api/deliveries/assignments/{{activeOfferAssignmentId}}/accept", {
    auth: "shipperToken",
    prerequest: requireVariables("activeOfferAssignmentId"),
    json: {},
  }),
  request("Pickup COD Assignment Status", "POST", "{{gatewayBaseUrl}}/api/deliveries/assignments/{{activeOfferAssignmentId}}/status", {
    auth: "shipperToken",
    prerequest: requireVariables("activeOfferAssignmentId"),
    json: {
      status: "PickedUp",
      note: "Full flow COD order picked up",
      proofFileKey: "{{deliveryFileKey}}",
    },
  }),
  request("Delivered COD Assignment Status", "POST", "{{gatewayBaseUrl}}/api/deliveries/assignments/{{activeOfferAssignmentId}}/status", {
    auth: "shipperToken",
    prerequest: requireVariables("activeOfferAssignmentId"),
    json: {
      status: "Delivered",
      note: "Full flow COD order delivered",
      proofFileKey: "deliveries/{{orderId}}/{{shipperId}}/delivered/full-flow-proof.jpg",
    },
  }),
  request("Get Payment By COD Order", "GET", "{{gatewayBaseUrl}}/api/orders/payments/{{orderId}}", {
    auth: "customerToken",
    tests: captureId("paymentId"),
  }),
  request("Add Cart Item For Cancel Order", "POST", "{{gatewayBaseUrl}}/api/orders/cart/items", {
    auth: "customerToken",
    json: { productId: "{{productId}}", quantity: 1, note: "Full flow cancel order", selectedOptions: [] },
  }),
  request("Create Cancel Candidate Order", "POST", "{{gatewayBaseUrl}}/api/orders", {
    auth: "customerToken",
    json: {
      merchantId: "{{merchantId}}",
      addressId: "{{customerAddressId}}",
      paymentMethod: "COD",
      note: "Full flow cancel candidate",
    },
    tests: captureId("cancelOrderId"),
  }),
  request("Cancel My Order", "PATCH", "{{gatewayBaseUrl}}/api/orders/{{cancelOrderId}}/cancel", {
    auth: "customerToken",
    json: { cancelReason: "Full flow cancellation coverage" },
  }),
  request("Add Cart Item For VNPay Order", "POST", "{{gatewayBaseUrl}}/api/orders/cart/items", {
    auth: "customerToken",
    json: { productId: "{{productId}}", quantity: 1, note: "Full flow VNPay order", selectedOptions: [] },
  }),
  request("Create VNPay Order", "POST", "{{gatewayBaseUrl}}/api/orders", {
    auth: "customerToken",
    json: {
      merchantId: "{{merchantId}}",
      addressId: "{{customerAddressId}}",
      paymentMethod: "VNPAY",
      note: "Full flow VNPay order",
    },
    tests: captureId("vnpayOrderId"),
  }),
  request("Get Payment By VNPay Order", "GET", "{{gatewayBaseUrl}}/api/orders/payments/{{vnpayOrderId}}", {
    auth: "customerToken",
    tests: captureId("paymentId"),
  }),
  request("Create VNPay Payment URL", "POST", "{{gatewayBaseUrl}}/api/orders/payments/{{vnpayOrderId}}/vnpay/url", {
    auth: "customerToken",
    json: { bankCode: "NCB" },
    tests: capture(`
setVar("vnpayPaymentUrl", get(data, "paymentUrl"));
setVar("vnpayOrderNumber", get(data, "orderNumber"));
`),
  }),
  request("VNPay Return Callback Placeholder", "GET", "{{gatewayBaseUrl}}/api/orders/payments/vnpay/return?vnp_TxnRef={{vnpayOrderNumber}}&vnp_ResponseCode=00&vnp_TransactionStatus=00&vnp_SecureHash=paste-valid-signature", {
    description: "Use a real signed VNPay callback query for strict success.",
  }),
  request("VNPay IPN Callback Placeholder", "GET", "{{gatewayBaseUrl}}/api/orders/payments/vnpay/ipn?vnp_TxnRef={{vnpayOrderNumber}}&vnp_ResponseCode=00&vnp_TransactionStatus=00&vnp_SecureHash=paste-valid-signature", {
    description: "Use a real signed VNPay callback query for strict success.",
  }),
], "Creates product-dependent cart/orders, including COD, cancellation, payment lookup, and VNPay URL generation.");

const delivery = folder("07 - Delivery Service", [
  request("Estimate Delivery Fee", "POST", "{{gatewayBaseUrl}}/api/deliveries/estimate-fee", {
    auth: "customerToken",
    json: {
      orderId: "{{orderId}}",
      pickupLat: 10.7769,
      pickupLng: 106.7009,
      deliveryLat: 10.77,
      deliveryLng: 106.695,
      subtotal: 75000,
      isRushHour: false,
    },
  }),
  request("Quote Delivery Fee", "POST", "{{gatewayBaseUrl}}/api/delivery-fee/quote", {
    auth: "customerToken",
    json: {
      pickupLat: 10.7769,
      pickupLng: 106.7009,
      deliveryLat: 10.77,
      deliveryLng: 106.695,
      subtotal: 75000,
      isRushHour: true,
    },
  }),
  request("Admin Get Delivery Fee Policies", "GET", "{{gatewayBaseUrl}}/api/delivery-fee/policies?pageIndex=1&pageSize=20&includeInactive=true", { auth: "adminToken" }),
  request("Admin Get Seed Delivery Fee Policy", "GET", "{{gatewayBaseUrl}}/api/delivery-fee/policies/{{feePolicyId}}", { auth: "adminToken" }),
  request("Admin Create Temp Delivery Fee Policy", "POST", "{{gatewayBaseUrl}}/api/delivery-fee/policies", {
    auth: "adminToken",
    json: {
      name: "Full Flow Temp Delivery Fee Policy {{runId}}",
      baseFee: 9000,
      minFee: 9000,
      maxFee: 65000,
      smallOrderThreshold: 50000,
      smallOrderSurcharge: 4000,
      rushHourSurcharge: 6000,
      isActive: false,
      distanceTiers: [
        { fromKm: 0, toKm: 2, feePerKm: 0 },
        { fromKm: 2, toKm: 5, feePerKm: 3500 },
        { fromKm: 5, toKm: null, feePerKm: 5500 },
      ],
    },
    tests: captureId("tempFeePolicyId"),
  }),
  request("Admin Update Temp Delivery Fee Policy", "PUT", "{{gatewayBaseUrl}}/api/delivery-fee/policies/{{tempFeePolicyId}}", {
    auth: "adminToken",
    json: {
      name: "Full Flow Temp Delivery Fee Policy Updated {{runId}}",
      baseFee: 10000,
      minFee: 10000,
      maxFee: 70000,
      smallOrderThreshold: 60000,
      smallOrderSurcharge: 4500,
      rushHourSurcharge: 6500,
      isActive: false,
      distanceTiers: [
        { fromKm: 0, toKm: 3, feePerKm: 0 },
        { fromKm: 3, toKm: 8, feePerKm: 4200 },
        { fromKm: 8, toKm: null, feePerKm: 6000 },
      ],
    },
  }),
  request("Admin Get All Availabilities", "GET", "{{gatewayBaseUrl}}/api/deliveries/availabilities?pageIndex=1&pageSize=20", { auth: "adminToken" }),
  request("Shipper Get Own Availability", "GET", "{{gatewayBaseUrl}}/api/deliveries/shippers/{{shipperId}}/availability", { auth: "shipperToken" }),
  request("Shipper Toggle Availability Online", "POST", "{{gatewayBaseUrl}}/api/deliveries/availability/toggle?shipperId={{shipperId}}", {
    auth: "shipperToken",
    json: { isGoOnline: true, lat: 10.7769, lng: 106.7009 },
  }),
  request("Patch Shipper Location", "PATCH", "{{gatewayBaseUrl}}/api/deliveries/shippers/{{shipperId}}/location", {
    auth: "shipperToken",
    json: { orderId: "{{orderId}}", latitude: 10.7736, longitude: 106.6976 },
  }),
  request("Post Delivery Location", "POST", "{{gatewayBaseUrl}}/api/deliveries/locations", {
    auth: "shipperToken",
    json: { orderId: "{{orderId}}", shipperId: "{{shipperId}}", latitude: 10.7737, longitude: 106.6977 },
  }),
  request("Get Order Location History", "GET", "{{gatewayBaseUrl}}/api/deliveries/orders/{{orderId}}/location-history?pageIndex=1&pageSize=20", { auth: "shipperToken" }),
  request("Get Shipper Location History", "GET", "{{gatewayBaseUrl}}/api/deliveries/shippers/{{shipperId}}/location-history?pageIndex=1&pageSize=20", { auth: "shipperToken" }),
  request("Admin Get All Assignments", "GET", "{{gatewayBaseUrl}}/api/deliveries/assignments?pageIndex=1&pageSize=20", { auth: "adminToken" }),
  request("Get Assignment By Id", "GET", "{{gatewayBaseUrl}}/api/deliveries/assignments/{{assignmentId}}", { auth: "shipperToken" }),
  request("Get Shipper Assignments", "GET", "{{gatewayBaseUrl}}/api/deliveries/shippers/{{shipperId}}/assignments?pageIndex=1&pageSize=20", { auth: "shipperToken" }),
  request("Get Active Offer", "GET", "{{gatewayBaseUrl}}/api/deliveries/shippers/me/active-offer", {
    auth: "shipperToken",
    tests: capture(`
if (get(data, "hasActiveOffer")) {
  const assignmentId = get(data, "assignmentId") || get(data, "offerId");
  setVar("activeOfferAssignmentId", assignmentId);
  setVar("assignmentId", assignmentId);
  setVar("activeOfferOrderId", get(data, "orderId"));
}
`),
    description: "A fresh active offer depends on order.completed/order.ready_for_pickup event processing.",
  }),
  request("Legacy Accept Or Reject Assignment", "POST", "{{gatewayBaseUrl}}/api/deliveries/assignments/accept", {
    auth: "shipperToken",
    prerequest: requireVariables("activeOfferAssignmentId"),
    json: {
      assignmentId: "{{activeOfferAssignmentId}}",
      offerId: "{{activeOfferAssignmentId}}",
      isAccepted: true,
      rejectReason: null,
    },
  }),
  request("Path Accept Assignment Offer", "POST", "{{gatewayBaseUrl}}/api/deliveries/assignments/{{activeOfferAssignmentId}}/accept", {
    auth: "shipperToken",
    prerequest: requireVariables("activeOfferAssignmentId"),
    json: {},
  }),
  request("Path Reject Assignment Offer", "POST", "{{gatewayBaseUrl}}/api/deliveries/assignments/{{activeOfferAssignmentId}}/reject", {
    auth: "shipperToken",
    prerequest: requireVariables("activeOfferAssignmentId"),
    json: { offerId: "{{activeOfferAssignmentId}}", reason: "Full flow rejection coverage" },
  }),
  request("Pickup Assignment Status", "POST", "{{gatewayBaseUrl}}/api/deliveries/assignments/{{activeOfferAssignmentId}}/status", {
    auth: "shipperToken",
    prerequest: requireVariables("activeOfferAssignmentId"),
    json: {
      status: "PickedUp",
      note: "Full flow picked up",
      proofFileKey: "{{deliveryFileKey}}",
    },
  }),
  request("Delivered Assignment Status", "POST", "{{gatewayBaseUrl}}/api/deliveries/assignments/{{activeOfferAssignmentId}}/status", {
    auth: "shipperToken",
    prerequest: requireVariables("activeOfferAssignmentId"),
    json: {
      status: "Delivered",
      note: "Full flow delivered",
      proofFileKey: "deliveries/{{orderId}}/{{shipperId}}/delivered/full-flow-proof.jpg",
    },
  }),
  request("Report Incident", "POST", "{{gatewayBaseUrl}}/api/deliveries/incidents", {
    auth: "customerToken",
    json: {
      orderId: "{{deliveredOrderId}}",
      type: "Other",
      description: "{{tempIncidentDescription}}",
      proofUrls: ["https://example.com/incidents/full-flow-1.jpg"],
    },
  }),
  request("Admin Get All Incidents", "GET", "{{gatewayBaseUrl}}/api/deliveries/incidents?pageIndex=1&pageSize=50", {
    auth: "adminToken",
    tests: captureListItemId("tempIncidentId", `
return get(item, "description") === pm.collectionVariables.get("tempIncidentDescription");
`),
  }),
  request("Get User Incidents", "GET", "{{gatewayBaseUrl}}/api/deliveries/users/{{customerUserId}}/incidents?pageIndex=1&pageSize=20", { auth: "customerToken" }),
  request("Get Seed Incident By Id", "GET", "{{gatewayBaseUrl}}/api/deliveries/incidents/{{incidentId}}", { auth: "customerToken" }),
  request("Resolve Temp Incident", "PATCH", "{{gatewayBaseUrl}}/api/deliveries/incidents/{{tempIncidentId}}/resolve", {
    auth: "adminToken",
    json: { status: "Resolved", resolution: "Resolved by full backend Postman flow." },
  }),
], "Delivery assignment offer flow needs a fresh event; REST endpoints are included in their operational order.");

const reviews = folder("08 - Catalog Reviews", [
  request("List Reviews", "GET", "{{gatewayBaseUrl}}/api/catalog/reviews?page=1&limit=20&sortBy=createdAt&sortOrder=desc"),
  request("Get Product Reviews", "GET", "{{gatewayBaseUrl}}/api/catalog/reviews/product/{{productId}}?page=1&limit=20&sortBy=createdAt&sortOrder=desc"),
  request("Get Product Review Summary", "GET", "{{gatewayBaseUrl}}/api/catalog/reviews/product/{{productId}}/summary"),
  request("Get User Reviews", "GET", "{{gatewayBaseUrl}}/api/catalog/reviews/user/{{customerUserId}}?page=1&limit=20&sortBy=createdAt&sortOrder=desc"),
  request("Get Merchant Reviews", "GET", "{{gatewayBaseUrl}}/api/catalog/reviews/merchant/{{merchantId}}?page=1&limit=20&sortBy=createdAt&sortOrder=desc"),
  request("Create Review", "POST", "{{gatewayBaseUrl}}/api/catalog/reviews", {
    auth: "customerToken",
    json: {
      orderId: "{{orderId}}",
      merchantId: "{{merchantId}}",
      productId: "{{productId}}",
      shipperId: "{{shipperId}}",
      rating: 5,
      comment: "Full flow review",
      images: ["https://example.com/reviews/full-flow.jpg"],
    },
    tests: captureId("reviewId"),
    description: "Requires the referenced order to be DELIVERED in OrderService.",
  }),
  request("Capture Review By Order", "GET", "{{gatewayBaseUrl}}/api/catalog/reviews?orderId={{orderId}}&productId={{productId}}&page=1&limit=20&sortBy=createdAt&sortOrder=desc", {
    tests: captureListItemId("reviewId", `
return get(item, "orderId") === pm.collectionVariables.get("orderId") &&
  get(item, "productId") === pm.collectionVariables.get("productId");
`),
    description: "Fallback capture so review-id routes never run with an empty id.",
  }),
  request("Get Review By Id", "GET", "{{gatewayBaseUrl}}/api/catalog/reviews/{{reviewId}}", {
    prerequest: requireVariables("reviewId"),
  }),
  request("Update Review PUT", "PUT", "{{gatewayBaseUrl}}/api/catalog/reviews/{{reviewId}}", {
    auth: "customerToken",
    prerequest: requireVariables("reviewId"),
    json: {
      rating: 4,
      comment: "Full flow review updated",
      images: ["https://example.com/reviews/full-flow-updated.jpg"],
    },
  }),
  request("Update Review PATCH", "PATCH", "{{gatewayBaseUrl}}/api/catalog/reviews/{{reviewId}}", {
    auth: "customerToken",
    prerequest: requireVariables("reviewId"),
    json: { comment: "Full flow review patched" },
  }),
  request("Merchant Reply To Review", "PATCH", "{{gatewayBaseUrl}}/api/catalog/reviews/{{reviewId}}/reply", {
    auth: "merchantToken",
    prerequest: requireVariables("reviewId"),
    json: { merchantReply: "Thank you for the full flow test review." },
  }),
  request("Delete Review Reply", "DELETE", "{{gatewayBaseUrl}}/api/catalog/reviews/{{reviewId}}/reply", {
    auth: "merchantToken",
    prerequest: requireVariables("reviewId"),
  }),
  request("Delete Review", "DELETE", "{{gatewayBaseUrl}}/api/catalog/reviews/{{reviewId}}", {
    auth: "customerToken",
    prerequest: requireVariables("reviewId"),
  }),
  request("Restore Review", "PATCH", "{{gatewayBaseUrl}}/api/catalog/reviews/{{reviewId}}/restore", {
    auth: "adminToken",
    prerequest: requireVariables("reviewId"),
  }),
], "Review creation depends on a delivered order in OrderService.");

const chat = folder("09 - Chat Service", [
  request("List Conversations", "GET", "{{gatewayBaseUrl}}/api/chats/conversations?page=1&limit=20", { auth: "customerToken" }),
  request("Create Order Merchant Conversation", "POST", "{{gatewayBaseUrl}}/api/chats/conversations", {
    auth: "customerToken",
    json: {
      conversationType: "ORDER_MERCHANT",
      orderId: "{{orderId}}",
      customerId: "{{customerUserId}}",
      merchantId: "{{merchantId}}",
    },
    tests: captureId("conversationId"),
  }),
  request("Get Conversation By Order", "GET", "{{gatewayBaseUrl}}/api/chats/orders/{{orderId}}/ORDER_MERCHANT", {
    auth: "customerToken",
    tests: captureId("conversationId"),
  }),
  request("Get Conversation By Id", "GET", "{{gatewayBaseUrl}}/api/chats/conversations/{{conversationId}}", { auth: "customerToken" }),
  request("Get Conversation Messages", "GET", "{{gatewayBaseUrl}}/api/chats/conversations/{{conversationId}}/messages?page=1&limit=20", { auth: "customerToken" }),
  request("Send Conversation Message", "POST", "{{gatewayBaseUrl}}/api/chats/conversations/{{conversationId}}/messages", {
    auth: "customerToken",
    json: { content: "Full flow chat message", messageType: "TEXT" },
    tests: captureId("messageId"),
  }),
  request("Mark Conversation Read", "PATCH", "{{gatewayBaseUrl}}/api/chats/conversations/{{conversationId}}/read", { auth: "customerToken" }),
]);

const wallet = folder("10 - Wallet Service", [
  request("Get My Merchant Wallet", "GET", "{{gatewayBaseUrl}}/api/wallets/me", {
    auth: "merchantToken",
    tests: captureId("walletId"),
  }),
  request("Get My Transactions", "GET", "{{gatewayBaseUrl}}/api/wallets/me/transactions?page=1&limit=20", { auth: "merchantToken" }),
  request("Get My Transactions By Order", "GET", "{{gatewayBaseUrl}}/api/wallets/me/transactions/order/{{orderId}}?page=1&limit=20", { auth: "merchantToken" }),
  request("Create Wallet Topup VNPay URL", "POST", "{{gatewayBaseUrl}}/api/wallets/me/topup/vnpay/url", {
    auth: "merchantToken",
    json: { amount: 50000, bankCode: "NCB" },
    tests: capture(`
setVar("walletTopupId", get(data, "topupId"));
setVar("walletTopupRequestCode", get(data, "requestCode"));
setVar("walletTopupPaymentUrl", get(data, "paymentUrl"));
`),
  }),
  request("Get My Topups", "GET", "{{gatewayBaseUrl}}/api/wallets/me/topups?page=1&limit=20", { auth: "merchantToken" }),
  request("Get My Topup By Id", "GET", "{{gatewayBaseUrl}}/api/wallets/me/topups/{{walletTopupId}}", { auth: "merchantToken" }),
  request("Get My Transactions By Reference", "GET", "{{gatewayBaseUrl}}/api/wallets/me/transactions/reference/wallet_topup/{{walletTopupId}}?page=1&limit=20", { auth: "merchantToken" }),
  request("Admin Get Wallet By Owner", "GET", "{{gatewayBaseUrl}}/api/wallets/admin/owners/MERCHANT/{{merchantId}}", { auth: "adminToken" }),
  request("Admin Get Negative Wallets", "GET", "{{gatewayBaseUrl}}/api/wallets/admin/negative?page=1&limit=20", { auth: "adminToken" }),
  request("Wallet Topup VNPay Return Placeholder", "GET", "{{gatewayBaseUrl}}/api/wallets/topup/vnpay/return?vnp_TxnRef={{walletTopupRequestCode}}&vnp_ResponseCode=00&vnp_TransactionStatus=00&vnp_SecureHash=paste-valid-signature", {
    description: "Use a real signed VNPay callback query for strict success.",
  }),
  request("Wallet Topup VNPay IPN Placeholder", "GET", "{{gatewayBaseUrl}}/api/wallets/topup/vnpay/ipn?vnp_TxnRef={{walletTopupRequestCode}}&vnp_ResponseCode=00&vnp_TransactionStatus=00&vnp_SecureHash=paste-valid-signature", {
    description: "Use a real signed VNPay callback query for strict success.",
  }),
]);

const reports = folder("11 - Report Service", [
  request("Admin Overview", "GET", "{{gatewayBaseUrl}}/api/reports/admin/overview", { auth: "adminToken" }),
  request("Admin Top Merchants", "GET", "{{gatewayBaseUrl}}/api/reports/admin/top-merchants", { auth: "adminToken" }),
  request("Admin Top Shippers", "GET", "{{gatewayBaseUrl}}/api/reports/admin/top-shippers", { auth: "adminToken" }),
  request("Admin Top Products", "GET", "{{gatewayBaseUrl}}/api/reports/admin/top-products", { auth: "adminToken" }),
  request("Merchant Overview", "GET", "{{gatewayBaseUrl}}/api/reports/merchant/me/overview", { auth: "merchantToken" }),
  request("Merchant Top Products", "GET", "{{gatewayBaseUrl}}/api/reports/merchant/me/top-products", { auth: "merchantToken" }),
  request("Shipper Overview", "GET", "{{gatewayBaseUrl}}/api/reports/shipper/me/overview", { auth: "shipperToken" }),
], "Unique report routes only. Duplicate route declarations in source are not separate callable URLs.");

const realtime = folder("12 - Realtime Hubs Reference", [
  request("Tracking Hub Negotiate", "POST", "{{gatewayBaseUrl}}/hubs/tracking/negotiate?access_token={{customerToken}}", {
    description: "SignalR negotiate endpoint for DeliveryService TrackingHub. Use a SignalR client for JoinOrderGroup and ReceiveLocation.",
  }),
  request("Assignment Hub Negotiate", "POST", "{{gatewayBaseUrl}}/hubs/assignments/negotiate?access_token={{shipperToken}}", {
    description: "SignalR negotiate endpoint for NotificationService AssignmentHub. Use a SignalR client for realtime assignment events.",
  }),
], "Postman REST collection can negotiate SignalR, but hub method invocation requires a SignalR/WebSocket client.");

const cleanup = folder("13 - Cleanup And Destructive Route Coverage", [
  request("Delete Temp User Address", "DELETE", "{{gatewayBaseUrl}}/api/users/{{customerUserId}}/addresses/{{tempAddressId}}", { auth: "customerToken" }),
  request("Delete Temp Merchant Address", "DELETE", "{{gatewayBaseUrl}}/api/merchants/{{merchantId}}/addresses/{{tempMerchantAddressId}}", { auth: "merchantToken" }),
  request("Delete Product Option", "DELETE", "{{gatewayBaseUrl}}/api/catalog/products/options/{{productOptionId}}", { auth: "merchantToken" }),
  request("Delete Product", "DELETE", "{{gatewayBaseUrl}}/api/catalog/products/{{productId}}", { auth: "merchantToken" }),
  request("Restore Product", "PATCH", "{{gatewayBaseUrl}}/api/catalog/products/{{productId}}/restore", { auth: "merchantToken" }),
  request("Final Delete Product", "DELETE", "{{gatewayBaseUrl}}/api/catalog/products/{{productId}}", { auth: "merchantToken" }),
  request("Delete Category", "DELETE", "{{gatewayBaseUrl}}/api/catalog/categories/{{categoryId}}", { auth: "adminToken" }),
  request("Restore Category", "PATCH", "{{gatewayBaseUrl}}/api/catalog/categories/{{categoryId}}/restore", { auth: "adminToken" }),
  request("Final Delete Category", "DELETE", "{{gatewayBaseUrl}}/api/catalog/categories/{{categoryId}}", { auth: "adminToken" }),
  request("Delete Voucher", "DELETE", "{{gatewayBaseUrl}}/api/orders/vouchers/{{voucherId}}", { auth: "merchantToken" }),
  request("Restore Voucher", "PATCH", "{{gatewayBaseUrl}}/api/orders/vouchers/{{voucherId}}/restore", { auth: "merchantToken" }),
  request("Final Delete Voucher", "DELETE", "{{gatewayBaseUrl}}/api/orders/vouchers/{{voucherId}}", { auth: "merchantToken" }),
  request("Delete Temp Delivery Fee Policy", "DELETE", "{{gatewayBaseUrl}}/api/delivery-fee/policies/{{tempFeePolicyId}}", { auth: "adminToken" }),
  request("Clear All Carts", "DELETE", "{{gatewayBaseUrl}}/api/orders/cart", { auth: "customerToken" }),
  request("Delete Temp Merchant Profile", "DELETE", "{{gatewayBaseUrl}}/api/merchants/{{tempMerchantId}}", {
    auth: "adminToken",
    description: "Deletes only the temp merchant profile if temp onboarding was completed.",
  }),
  request("Delete Temp Shipper Profile", "DELETE", "{{gatewayBaseUrl}}/api/shippers/{{tempShipperId}}", {
    auth: "adminToken",
    description: "Deletes only the temp shipper profile if temp onboarding was completed.",
  }),
  request("Delete Temp Customer User", "DELETE", "{{gatewayBaseUrl}}/api/users/{{tempCustomerUserId}}", {
    auth: "adminToken",
    description: "Deletes only the temp user if temp registration was completed.",
  }),
  request("Logout Customer", "POST", "{{gatewayBaseUrl}}/api/auth/logout", {
    headers: [["X-Device-Id", "{{deviceId}}-customer"]],
    json: { refreshToken: "{{customerRefreshToken}}" },
  }),
], "Cleanup tests destructive routes without targeting seeded merchant/user/shipper IDs.");

const collection = {
  info: {
    _postman_id: "c2f1ab3c-a09f-4da4-ae31-bdbb6e9482e4",
    name: collectionName,
    description: [
      "Sequential full-backend Postman collection for ProjectCode_BE.",
      "",
      "Run from top to bottom after starting ProjectCode_BE/Backend with Docker Compose.",
      "Default base URL is the ApiGateway at {{gatewayBaseUrl}}.",
      "",
      "The collection initializes generated names, emails, and codes in the first request.",
      "It captures access tokens and IDs into collection variables where responses provide them.",
      "",
      "Default tests assert that responses stay below HTTP 500. Set collection variable strict2xx=true when OTP, upload file, VNPay callback signatures, SignalR/realtime prerequisites, and temp onboarding tokens are ready.",
      "",
      "Manual/external steps that cannot be completed by REST alone: OTP verification, signed cloud upload binary PUT, real VNPay callback signatures, RabbitMQ-triggered delivery offers, and SignalR hub method invocation.",
    ].join("\\n"),
    schema: "https://schema.getpostman.com/json/collection/v2.1.0/collection.json",
  },
  event: [
    event("prerequest", collectionPrerequest),
    event("test", collectionTest),
  ],
  variable: variables,
  item: [
    setup,
    auth,
    address,
    users,
    filesNotifications,
    catalog,
    order,
    delivery,
    reviews,
    chat,
    wallet,
    reports,
    realtime,
    cleanup,
  ],
};

fs.writeFileSync(outputFile, JSON.stringify(collection, null, 2) + "\n", "utf8");
console.log(`Wrote ${outputFile}`);
