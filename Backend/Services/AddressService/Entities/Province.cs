namespace AddressService.Entities
{
    public class Province
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? NameEn { get; set; }
        public string FullName { get; set; } = null!;
        public string? FullNameEn { get; set; }
        public string? CodeName { get; set; }
        public int? AdministrativeUnitId { get; set; }

        public AdministrativeUnit? AdministrativeUnit { get; set; }
        public ICollection<Ward> Wards { get; set; } = new List<Ward>();
    }
}
