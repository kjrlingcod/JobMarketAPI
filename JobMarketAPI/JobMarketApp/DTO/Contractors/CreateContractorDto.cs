using System.ComponentModel.DataAnnotations;

namespace JobMarketApp.API.DTO.Contractors
{
    public class CreateContractorDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Range(0, 5)]
        public decimal Rating { get; set; }
    }
}
