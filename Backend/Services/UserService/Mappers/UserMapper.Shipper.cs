using Riok.Mapperly.Abstractions;
using UserService.DTOs.ShipperDTOs;
using UserService.Entities;

namespace UserService.Mappers
{
    public partial class UserMapper
    {
        [MapperIgnoreTarget(nameof(ShipperRequest.Id))]
        [MapperIgnoreTarget(nameof(ShipperRequest.UserId))]
        [MapperIgnoreTarget(nameof(ShipperRequest.VerificationStatus))]
        [MapperIgnoreTarget(nameof(ShipperRequest.RejectedReason))]
        [MapperIgnoreTarget(nameof(ShipperRequest.VerifiedAt))]
        [MapperIgnoreTarget(nameof(ShipperRequest.CreatedAt))]
        [MapperIgnoreTarget(nameof(ShipperRequest.ReviewedBy))]
        [MapperIgnoreTarget(nameof(ShipperRequest.User))]
        [MapperIgnoreTarget(nameof(ShipperRequest.ReviewedUser))]
        public partial ShipperRequest ToShipperRequest(CreateShipperRequest request);

        [MapperIgnoreTarget(nameof(Shipper.Id))]
        [MapperIgnoreTarget(nameof(Shipper.VehiclePlate))]
        [MapperIgnoreTarget(nameof(Shipper.Status))]
        [MapperIgnoreTarget(nameof(Shipper.Request))]
        [MapperIgnoreTarget(nameof(Shipper.User))]
        [MapperIgnoreTarget(nameof(Shipper.CreatedAt))]
        [MapperIgnoreTarget(nameof(Shipper.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Shipper.DeletedAt))]
        [MapperIgnoreSource(nameof(ShipperRequest.VerificationStatus))]
        [MapperIgnoreSource(nameof(ShipperRequest.RejectedReason))]
        [MapperIgnoreSource(nameof(ShipperRequest.VerifiedAt))]
        [MapperIgnoreSource(nameof(ShipperRequest.CreatedAt))]
        [MapperIgnoreSource(nameof(ShipperRequest.ReviewedBy))]
        [MapperIgnoreSource(nameof(ShipperRequest.User))]
        [MapperIgnoreSource(nameof(ShipperRequest.ReviewedUser))]
        [MapperIgnoreSource(nameof(ShipperRequest.LicenseNumber))]
        [MapperIgnoreSource(nameof(ShipperRequest.LicenseFrontUrl))]
        [MapperIgnoreSource(nameof(ShipperRequest.LicenseBackUrl))]
        [MapperIgnoreSource(nameof(ShipperRequest.IdCardFrontUrl))]
        [MapperIgnoreSource(nameof(ShipperRequest.IdCardBackUrl))]
        [MapperIgnoreSource(nameof(ShipperRequest.SelfieUrl))]
        [MapperIgnoreSource(nameof(ShipperRequest.IdNumber))]
        [MapperIgnoreSource(nameof(ShipperRequest.FullName))]
        [MapperIgnoreSource(nameof(ShipperRequest.DateOfBirth))]
        [MapProperty(nameof(ShipperRequest.Id), nameof(Shipper.RequestId))]
        public partial Shipper ToShipper(ShipperRequest shipperRequest);

        [MapperIgnoreSource(nameof(Shipper.DeletedAt))]
        [MapperIgnoreSource(nameof(Shipper.User))]
        [MapperIgnoreSource(nameof(Shipper.RequestId))]
        [MapperIgnoreSource(nameof(Shipper.Request))]
        public partial ShipperResponse ToShipperResponse(Shipper shipper);

        [MapperIgnoreSource(nameof(Shipper.DeletedAt))]
        [MapperIgnoreSource(nameof(Shipper.User))]
        [MapperIgnoreSource(nameof(Shipper.RequestId))]
        [MapperIgnoreSource(nameof(Shipper.Request))]
        public partial IEnumerable<ShipperResponse> ToShipperResponseList(IEnumerable<Shipper> shipperList);

        [MapperIgnoreSource(nameof(ShipperRequest.User))]
        [MapperIgnoreSource(nameof(ShipperRequest.ReviewedUser))]
        [MapProperty(nameof(ShipperRequest.IdCardFrontUrl), nameof(ShipperRequestResponse.IdFrontUrl))]
        [MapProperty(nameof(ShipperRequest.IdCardBackUrl), nameof(ShipperRequestResponse.IdBackUrl))]
        [MapProperty(nameof(ShipperRequest.VerificationStatus), nameof(ShipperRequestResponse.Status))]
        public partial IEnumerable<ShipperRequestResponse> ToShipperRequestResponseList(IEnumerable<ShipperRequest> shipperRequests);

        [MapperIgnoreSource(nameof(ShipperRequest.User))]
        [MapperIgnoreSource(nameof(ShipperRequest.ReviewedUser))]
        [MapProperty(nameof(ShipperRequest.IdCardFrontUrl), nameof(ShipperRequestResponse.IdFrontUrl))]
        [MapProperty(nameof(ShipperRequest.IdCardBackUrl), nameof(ShipperRequestResponse.IdBackUrl))]
        [MapProperty(nameof(ShipperRequest.VerificationStatus), nameof(ShipperRequestResponse.Status))]
        public partial ShipperRequestResponse ToShipperRequestResponse(ShipperRequest shipperRequest);
    }
}
