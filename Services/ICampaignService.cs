using CampaignApi.Models;

namespace CampaignApi.Services
{
    public interface ICampaignService
    {
        Task<int> CreateCampaign(CampaignRequest request);
        Task DeleteCampaign(int id);
        Task ChangeState(int id, State state);
        Task ChangeStatus(int id, Status status);
        Task EditCampaign(int id, EditCampaignRequest request);
        Task<int> CloneCampaign(int id);
        Task<List<Campaign>> GetAllCampaigns();
        Task<List<Campaign>> FilterCampaigns(Filter filter);
        Task<List<Campaign>> GetCampaignsByPage(int page);
    }
}
