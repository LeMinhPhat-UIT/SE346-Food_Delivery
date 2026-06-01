namespace AddressService.DTOs
{
    public class WardResponse
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? NameEn { get; set; }
        public string? FullName { get; set; }
        public string? FullNameEn { get; set; }
        public string? CodeName { get; set; }
        public string? ProvinceCode { get; set; }
        public string? ProvinceName { get; set; }
        public string? ProvinceFullName { get; set; }
        public AdministrativeUnitResponse? AdministrativeUnit { get; set; }
    }
}
