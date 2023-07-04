using CampaignApi.Models;
using CampaignApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        [HttpPost()]
        public async Task<IActionResult> CreateCampaign(CampaignRequest cr)
        {
            return Ok(await _campaignService.CreateCampaign(cr));
        }

        [HttpDelete()]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            await _campaignService.DeleteCampaign(id);
            return Ok();
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateState(int id, State state)
        {
            await _campaignService.ChangeState(id, state);
            return Ok();
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus(int id, Status status)
        {
            await _campaignService.ChangeStatus(id, status);
            return Ok();
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditCampaign(int id, EditCampaignRequest request)
        {
            await _campaignService.EditCampaign(id, request);
            return Ok();
        }
        [HttpPost("clone")]
        public async Task<IActionResult> cloneCampaign(int id)
        {
            return Ok(await _campaignService.CloneCampaign(id));
        }

        [HttpGet("campaigns")]
        public async Task<IActionResult> GetAllCampaigns()
        {
            return Ok(await _campaignService.GetAllCampaigns());
        }

        [HttpGet("campaignsFilter")]
        public async Task<IActionResult> FilterCampaigns([FromQuery] Filter filter)
        {
            return Ok(await _campaignService.FilterCampaigns(filter));
        }

        [HttpGet("page")]
        public async Task<IActionResult> GetCampaignsOnPage(int page)
        {
            return Ok(await _campaignService.GetCampaignsByPage(page));
        }
    }
}
