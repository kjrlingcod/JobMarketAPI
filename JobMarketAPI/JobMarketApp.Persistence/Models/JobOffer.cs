namespace JobMarketApp.Persistence.Models
{
    public class JobOffer
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid ContractorId { get; set; }
        public decimal Price { get; set; }
    }
}
