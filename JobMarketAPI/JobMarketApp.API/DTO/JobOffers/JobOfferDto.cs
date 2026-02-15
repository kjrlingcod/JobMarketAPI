namespace JobMarketApp.API.DTO.JobOffers
{
    public class JobOfferDto
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid ContractorId { get; set; }
        public decimal Price { get; set; }
    }
}
