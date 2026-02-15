namespace JobMarketApp.API.DTO.Contractors
{
    public class ContractorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Rating { get; set; }
    }
}
