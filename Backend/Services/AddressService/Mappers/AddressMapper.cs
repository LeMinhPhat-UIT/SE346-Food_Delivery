using AddressService.DTOs;
using AddressService.Entities;

namespace AddressService.Mappers
{
    public class AddressMapper
    {
        public IEnumerable<ProvinceResponse> ToProvinceResponses(IEnumerable<Province> provinces)
        {
            return provinces.Select(ToProvinceResponse);
        }

        public ProvinceResponse ToProvinceResponse(Province province)
        {
            return new ProvinceResponse
            {
                Code = province.Code,
                Name = province.Name,
                NameEn = province.NameEn,
                FullName = province.FullName,
                FullNameEn = province.FullNameEn,
                CodeName = province.CodeName,
                AdministrativeUnit = ToAdministrativeUnitResponse(province.AdministrativeUnit)
            };
        }

        public IEnumerable<WardResponse> ToWardResponses(IEnumerable<Ward> wards)
        {
            return wards.Select(ToWardResponse);
        }

        public WardResponse ToWardResponse(Ward ward)
        {
            return new WardResponse
            {
                Code = ward.Code,
                Name = ward.Name,
                NameEn = ward.NameEn,
                FullName = ward.FullName,
                FullNameEn = ward.FullNameEn,
                CodeName = ward.CodeName,
                ProvinceCode = ward.ProvinceCode,
                ProvinceName = ward.Province?.Name,
                ProvinceFullName = ward.Province?.FullName,
                AdministrativeUnit = ToAdministrativeUnitResponse(ward.AdministrativeUnit)
            };
        }

        public AddressResolutionResponse ToAddressResolutionResponse(Province province, Ward ward, string? addressLine)
        {
            var wardFullName = ward.FullName ?? ward.Name;
            var normalizedAddressLine = string.IsNullOrWhiteSpace(addressLine) ? null : addressLine.Trim();
            var fullAddress = normalizedAddressLine is null
                ? $"{wardFullName}, {province.FullName}"
                : $"{normalizedAddressLine}, {wardFullName}, {province.FullName}";

            return new AddressResolutionResponse
            {
                ProvinceCode = province.Code,
                ProvinceName = province.Name,
                ProvinceFullName = province.FullName,
                WardCode = ward.Code,
                WardName = ward.Name,
                WardFullName = wardFullName,
                AddressLine = normalizedAddressLine,
                FullAddress = fullAddress,
                City = province.FullName,
                District = string.Empty,
                Ward = wardFullName
            };
        }

        private static AdministrativeUnitResponse? ToAdministrativeUnitResponse(AdministrativeUnit? administrativeUnit)
        {
            if (administrativeUnit is null)
                return null;

            return new AdministrativeUnitResponse
            {
                Id = administrativeUnit.Id,
                FullName = administrativeUnit.FullName,
                FullNameEn = administrativeUnit.FullNameEn,
                ShortName = administrativeUnit.ShortName,
                ShortNameEn = administrativeUnit.ShortNameEn,
                CodeName = administrativeUnit.CodeName,
                CodeNameEn = administrativeUnit.CodeNameEn
            };
        }
    }
}
