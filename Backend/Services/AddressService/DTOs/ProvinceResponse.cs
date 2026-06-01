namespace AddressService.DTOs
{
    public class ProvinceResponse
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? NameEn { get; set; }
        public string FullName { get; set; } = null!;
        public string? FullNameEn { get; set; }
        public string? CodeName { get; set; }
        public AdministrativeUnitResponse? AdministrativeUnit { get; set; }
    }
}
