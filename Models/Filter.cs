namespace CampaignApi.Models
{
    public class Filter
    {
        public string? CompanyName { get; set; }
        public RewardType? RewardType { get; set; }
        public State? State { get; set; }
        public Status? status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
    }
}
