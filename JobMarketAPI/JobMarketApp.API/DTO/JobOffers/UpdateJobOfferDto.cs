using System.ComponentModel.DataAnnotations;

namespace JobMarketApp.API.DTO.JobOffers
{
    public class UpdateJobOfferDto
    {
        [Required]
        public Guid JobId { get; set; }
        [Required]
        public Guid ContractorId { get; set; }
        [Required]
        [Range(typeof(decimal), "0.01", "1000000000",
            ErrorMessage = "Price must be between 0.01 and 1,000,000,000.")]
        public decimal Price { get; set; }
    }
}
