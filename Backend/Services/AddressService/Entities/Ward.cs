namespace AddressService.Entities
{
    public class Ward
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? NameEn { get; set; }
        public string? FullName { get; set; }
        public string? FullNameEn { get; set; }
        public string? CodeName { get; set; }
        public string? ProvinceCode { get; set; }
        public int? AdministrativeUnitId { get; set; }

        public Province? Province { get; set; }
        public AdministrativeUnit? AdministrativeUnit { get; set; }
    }
}
