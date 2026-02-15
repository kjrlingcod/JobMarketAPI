namespace JobMarketApp.Persistence.Models
{
    public class Job
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Budget { get; set; }
        public string Description { get; set; }
        public Guid AcceptedBy { get; set; }
    }
}
