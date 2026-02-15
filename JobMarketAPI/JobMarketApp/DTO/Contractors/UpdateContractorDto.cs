namespace JobMarketApp.API.DTO.Contractors
{
    public class UpdateContractorDto
    {
        public string Name { get; set; } = default!;
        public decimal Rating { get; set; }
    }
}
