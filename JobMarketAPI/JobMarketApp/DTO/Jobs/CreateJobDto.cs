using System.ComponentModel.DataAnnotations;

namespace JobMarketApp.API.DTO.Jobs
{
    public class CreateJobDto
    {
        [Required]
        public Guid CustomerId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        [Required]
        [Range(typeof(decimal), "0.01", "1000000000",
            ErrorMessage = "Budget must be between 0.01 and 1,000,000,000.")]
        public decimal Budget { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
