using System.ComponentModel.DataAnnotations;

namespace JobMarketApp.API.DTO.JobOffers
{
    public class AcceptJobOfferDto
    {
        [Required]
        public Guid JobId { get; set; }

        [Required]
        public Guid ContractorId { get; set; }
    }
}
