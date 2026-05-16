Here is a full breakdown of every bug and inconsistency found across the four ASP.NET services (AuthenticationService, UserService, DeliveryService, NotificationService) and the shared libraries:

---

## AuthenticationService

**1. OTP expiry uses `AddMinutes` but config unit is seconds**
In `AuthService.cs`, `OTP_EXPIRY_SECONDS` is loaded from `DefaultOtpExpiredTimeSpanInSeconds`, but then passed to `DateTime.UtcNow.AddMinutes(OTP_EXPIRY_SECONDS)` in all three places (new registration, re-registration, and resend OTP). If the config value is `300` (meaning 5 minutes in seconds), the OTP will actually expire after 300 minutes (5 hours). Should be `AddSeconds`.

**2. Lockout `LockoutEndDate` uses `AddSeconds` but config property is named `InMinutes`**
In `AuthService.Login`, the lockout event is populated with:
```
LockoutEndDate = DateTime.UtcNow.AddSeconds(_options.Value.LockoutSettings.DefaultLockoutTimeSpanInMinutes)
```
The property name says "Minutes" but `AddSeconds` is used. One of them is wrong — either the property name should say "Seconds" or the call should be `AddMinutes`.

**3. OTP generation uses `System.Random` instead of a cryptographically secure RNG**
`GenerateOtp()` uses `new Random().Next(100000, 999999)`, which is not cryptographically secure. `System.Security.Cryptography.RandomNumberGenerator` should be used instead.

**4. OTP resend path for existing unverified users bypasses the logging helper**
In `RegisterCustomerAsync`, when re-sending OTP to an already-registered but unverified user, the code calls `_eventPublisher.PublishAsync(otpEvent)` directly (with a commented-out `PublishOtpEventAsync` above it). For new users, it correctly calls `PublishOtpEventAsync`, which logs the publish. This is inconsistent — the commented-out line suggests it was intentional but the result is the event sends without logging.

**5. `RevokeTokenRequest` and `RevokeTokenResponse` DTOs exist but no endpoint uses them**
`IAuthService` has no `RevokeToken` method, and `AuthController` has no revoke endpoint. These DTOs are dead code (or a missing feature).

**6. No `[Authorize]` attribute on any Auth endpoints**
`AuthController` is fully public, which is correct for login/register/OTP, but there is no protection distinguishing unauthenticated from authenticated calls at the controller level. This is not a bug per se, but worth flagging for the revoke and logout endpoints which should arguably require a valid token.

---

## UserService

**7. Pagination metadata is lost when re-wrapping `PagedResult`**
In `UserService.cs`, `GetAllUserAsync`, `GetAllUserAddressesAsync`, and `GetAllUserAddressesByUserIdAsync` all call `ToPagedResultAsync` (which correctly sets `TotalCount`, `PaginationRequest`, etc.), then map the items and re-wrap in `new PagedResult<T>(response)`. The single-argument constructor only sets `Items` — `TotalCount` and `PaginationRequest` are reset to defaults. Clients will receive pagination metadata showing 0 total count and no next/previous page info.

**8. `RequestForMerchantRole` pre-populates `ReviewedBy` with the requester's own `userId`**
In `UserService.Merchant.cs`:
```csharp
merchantRequest.ReviewedBy = userId;  // This is the applicant, not a reviewer
```
This is overwritten correctly when a real reviewer acts on the request, but until then the field contains misleading data implying the user reviewed their own request. It should be left as `null` or `Guid.Empty`.

**9. `GetMerchantByUserIdAsync` is called redundantly in `ReviewMerchantRequestAsync` for the approved path**
After approving, the code fetches `existingMerchant = await _userRepository.GetMerchantByUserIdAsync(merchantRequest.UserId)` to guard against duplicates, but `merchantRequest.UserId` was never validated to exist beforehand — this is fine. However, if an approved request is replayed (e.g., event duplication), the approved-path check correctly returns a 409. This logic is sound but relies on no prior check against the request's `UserId` being currently a merchant before the request was created. The creation-time check in `RequestForMerchantRole` guards this.

**10. `GET /merchants` and `GET /merchants/{merchantId}` have no `[Authorize]`**
These endpoints are fully public, exposing all merchant data including `BusinessLicense` and `TaxId` fields (visible in `MerchantResponse`). If that information is sensitive, these endpoints need authorization.

**11. `GET /merchant/{merchantId}/location` has a different route prefix (`merchant` vs `merchants`) and no `[Authorize]`**
This looks like a typo — all other merchant endpoints use `merchants` (plural), but this one uses `merchant` (singular). It is also completely unauthenticated, unlike the similar `GET /merchants/{id}/addresses` which requires `[Authorize]`. This creates a duplicate, inconsistent, and unprotected endpoint.

---

## DeliveryService

**12. `FileKey` returned by `GET /files/upload-url` uses the original filename, but the upload URL is for a UUID-renamed file**
In `DeliveriesController.GetUploadUrl`, the response `FileKey` is:
```csharp
FileKey = $"deliveries/{orderId}/{shipperId}/{stage}/{Path.GetFileName(fileName)}"
```
But in `DeliveryService.GetUploadUrl`, the file is renamed to a new UUID:
```csharp
var newFileName = $"{Guid.NewGuid()}{extension}";
var filePath = $"deliveries/{orderId}/{shipperId}/{stage}/{newFileName}";
```
The `FileKey` in the response will not match the actual path of the uploaded file. The client will store the wrong key, and any later attempt to fetch the file using that key will fail.

**13. No authentication/authorization on any `DeliveriesController` endpoint**
`Program.cs` only calls `app.UseAuthorization()` but never registers authentication middleware (`UseAuthentication` or JWT bearer). There is no `[Authorize]` attribute on any endpoint in `DeliveriesController`. Any caller — authenticated or not — can toggle shipper availability, view all assignments, accept/reject assignments, and get upload URLs.

**14. `TrackingHub` has no `[Authorize]` attribute**
Any WebSocket client can connect to `/hubs/tracking`, call `SendLocation`, `JoinOrderGroup`, and `LeaveOrderGroup` without authentication. Location events will be published to RabbitMQ unauthenticated.

**15. `AcceptAssignment` maps all non-"already accepted" failures to `404 Not Found`**
In `DeliveriesController.AcceptAssignment`:
```csharp
if (message.Contains("already accepted"))
    return Conflict(...);
return NotFound(...);  // catches "Assignment has already been handled", "Reject reason required", etc.
```
Failures like "Assignment has already been handled" or "Reject reason is required" are business logic rejections and should return `400 Bad Request`, not `404 Not Found`.

**16. Race condition in `AcceptOrRejectAssignmentAsync` is acknowledged in a comment but not actually fixed**
The comment says "Rate condition: Check if another shipper has already accepted this order," but the check-then-update is not wrapped in a database transaction or optimistic concurrency lock. Two shippers accepting simultaneously can both pass the guard and both write `AssignmentStatus.Accepted`.

**17. `GetAllShipperAssignmentsByShipperIdAsync` does not filter out cancelled assignments**
`GetAllShipperAssignmentsAsync` (the global list) filters `Status != Cancelled`. But `GetAllShipperAssignmentsByShipperIdAsync` has no such filter, so a shipper querying their own assignments gets cancelled ones too, inconsistently with the global endpoint.

**18. `OrderCompletedEventHandler` publishes `ShipperFoundEvent` only when shippers are available, but silently does nothing otherwise**
If no available shippers are found, no event is published and no fallback mechanism or alert is triggered. This means the order simply stalls with no notification to the customer or system. At minimum, a warning log should be emitted or a different event published.

---

## NotificationService

**19. `DeliveryMilestoneEventHandler` and `OrderCompletedEventHandler` do not persist notifications to the database**
Both handlers send push notifications but never call any create/save method on `_notificationRepository`. The `INotificationRepository` interface only exposes device-fetching methods — there is no `CreateNotificationAsync`. As a result, users have no notification history, and `GET /notifications` (if it exists) would always return nothing.

**20. `ShipperFoundEventHandler` has unused imports**
It imports `Twilio.TwiML.Messaging`, `static System.Runtime.InteropServices.JavaScript.JSType`, `Microsoft.EntityFrameworkCore`, and `System.Linq` — none of which are used. This suggests copy-paste leftover and should be cleaned up.

---

## Summary Table

| # | Service | File | Severity | Description |
|---|---------|------|----------|-------------|
| 1 | Auth | AuthService.cs | **High** | OTP expiry uses `AddMinutes` on a value in seconds |
| 2 | Auth | AuthService.cs | **High** | Lockout duration uses `AddSeconds` on a "Minutes" config value |
| 3 | Auth | AuthService.cs | **Medium** | OTP generated with non-cryptographic `System.Random` |
| 4 | Auth | AuthService.cs | Low | OTP resend for existing user bypasses logging helper inconsistently |
| 5 | Auth | DTOs/ | Low | `RevokeTokenRequest/Response` DTOs are dead code |
| 7 | User | UserService.cs | **High** | Pagination metadata (`TotalCount`, `PaginationRequest`) lost when re-wrapping `PagedResult` |
| 8 | User | UserService.Merchant.cs | Medium | `ReviewedBy` incorrectly pre-set to applicant's own ID |
| 10 | User | UsersController.Merchant.cs | **High** | `GET /merchants` and `GET /merchants/{id}` expose sensitive fields publicly |
| 11 | User | UsersController.Merchant.cs | **High** | Route typo `merchant` vs `merchants` and endpoint is unauthenticated |
| 12 | Delivery | DeliveriesController.cs | **High** | `FileKey` in response doesn't match the actual uploaded file path |
| 13 | Delivery | Program.cs + Controller | **Critical** | No authentication middleware; entire controller is unauthenticated |
| 14 | Delivery | TrackingHub.cs | **High** | SignalR hub has no `[Authorize]`, open to anonymous connections |
| 15 | Delivery | DeliveriesController.cs | Medium | Wrong HTTP status code (`404`) for business logic failures |
| 16 | Delivery | DeliveryService.cs | **High** | Race condition in accept assignment not truly protected |
| 17 | Delivery | DeliveryRepository.cs | Medium | Shipper-specific assignment query includes cancelled assignments |
| 18 | Delivery | OrderCompletedEventHandler.cs | Medium | No fallback when no shippers are available |
| 19 | Notification | Event Handlers | **High** | Notifications sent but never persisted to DB |
| 20 | Notification | ShipperFoundEventHandler.cs | Low | Unused imports (Twilio, JSType, EF) left in file |