using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignApi.Models
{   
    public enum RewardType
    {
        FreeSpin = 1,
        Cash = 2,
        FreeBet = 3
    }
    public enum State
    {
        Published = 1,
        Unpublished = 2
    }
    public enum Status
    {
        Active = 1, 
        Cancelled = 2,
        ReActivated = 3,
        Initialized = 4,
        Finished = 5
    }
    
    public class Campaign
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("create_date")]
        public DateTime CreateDate { get; set; }

        [Column("campaign_name")]
        public string CampaignName { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("reward_type")]
        public RewardType RewardType { get; set; }

        [Column("state")]
        public State State { get; set; }

        [Column("status")]
        public Status Status { get; set; }

        [Column ("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
