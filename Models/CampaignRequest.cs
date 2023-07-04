namespace CampaignApi.Models
{
    public class CampaignRequest
    {
        public string CampaignName { get; set; }
        public DateTime Start_date { get; set; }
        public DateTime End_date { get; set; }
        public RewardType RewardType { get; set; }
    }
}
