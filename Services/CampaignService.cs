using CampaignApi.Models;
using CampaignApi.Models.Connection;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Net.NetworkInformation;
using System.Xml.Linq;

namespace CampaignApi.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly Connector _connector;

        private readonly IConfiguration _configuration;
        public CampaignService(IOptions<Connector> connectionStrings, IConfiguration configuration) {
            _connector = connectionStrings.Value;
            _configuration = configuration;
        }

        public async Task<int> CreateCampaign(CampaignRequest request)
        {
            Campaign newCampaign = new Campaign();

            newCampaign.CreateDate= DateTime.UtcNow;
            newCampaign.CampaignName = request.CampaignName;
            newCampaign.StartDate = request.Start_date;
            newCampaign.EndDate = request.End_date;
            newCampaign.RewardType = request.RewardType;
            newCampaign.State = State.Unpublished;
            newCampaign.Status = Status.Initialized;
            newCampaign.IsDeleted = false;

            using (var connection = new NpgsqlConnection(_connector.ConnectionStrings))
            {
                var query = "INSERT INTO campaigns (create_date, campaign_name, start_date, end_date, reward_type, state, status, is_deleted)" +
                    " values (@CreateDate, @CampaignName, @StartDate, @EndDate, @RewardType, @State, @Status, @IsDeleted) returning id";
                var id = connection.QuerySingleOrDefaultAsync<int>(query, newCampaign);
                return await id;
            }
        }

        private static async Task<Status> GetCampaignStatus(int id, NpgsqlConnection connection)
        {
            var query = "SELECT status FROM campaigns where id = @id";
            var state = await connection.QuerySingleOrDefaultAsync<Status>(query, new { id });
            return state;
        }

        private static async Task<State> GetCampaignState(int id, NpgsqlConnection connection)
        {
            var query = "SELECT state FROM campaigns where id = @id";
            var state = await connection.QuerySingleOrDefaultAsync<State>(query, new { id });
            return state;
        }

        public async Task DeleteCampaign(int id)
        {
            using (var connection = new NpgsqlConnection(_connector.ConnectionStrings))
            {
                if (await GetCampaignStatus(id, connection) == Status.Active || await GetCampaignStatus(id, connection) == Status.ReActivated) { throw new ArgumentException("Cannot Delete Campaign With Active Status"); }
                var query = "UPDATE campaigns SET is_deleted = true where id = @id";
                await connection.ExecuteAsync(query, new {id});
            }
        }

        public async Task ChangeState(int id, State state)
        {
            using (var connection = new NpgsqlConnection(_connector.ConnectionStrings))
            {
                State curCampaignState = await GetCampaignState(id, connection);
                if (curCampaignState == state) return;
                if (curCampaignState == State.Published) throw new ArgumentException("Cannot Change From State Published To Unpublished");
                if(curCampaignState == State.Unpublished)
                {
                    var query = "UPDATE campaigns SET state = @Published, status = @Active where id = @id";
                    await connection.ExecuteAsync(query, new { State.Published, Status.Active, id });
                }
            }
        }

        public async Task ChangeStatus(int id, Status status)
        {
            using (var connection = new NpgsqlConnection(_connector.ConnectionStrings))
            {
                State curState = await GetCampaignState(id, connection);
                Status curStatus = await GetCampaignStatus(id, connection);
                if (curStatus == status) return;
                if (curStatus == Status.Active && status == Status.ReActivated)
                    throw new ArgumentException("Can not re-activate activated campaign");
                if (curStatus == Status.ReActivated && status == Status.Active)
                    throw new ArgumentException("Can not activate re-activated campaign");
                if (curState == State.Unpublished && curStatus == Status.Initialized)
                    throw new ArgumentException("Can not change Initialized status before publishing");
                if (curStatus == Status.Finished)
                    throw new ArgumentException("Can not change Finished campaign");
                var query = "Update campaigns Set status = @status where id = @id";
                await connection.ExecuteAsync(query, new { status, id});
            }
        }

        public async Task EditCampaign(int id, EditCampaignRequest request)
        {
            string campaignName = request.CampaignName;
            DateTime startDate = request.Start_date;
            DateTime endDate = request.End_date;
            RewardType rewardType = request.RewardType;
            Status status = request.status;
            using (var connection = new NpgsqlConnection(_connector.ConnectionStrings))
            {
                State curCampaignState = await GetCampaignState(id, connection);
                if (curCampaignState == State.Published && (campaignName != null || startDate != DateTime.MinValue || status != 0))
                    throw new ArgumentException("Can not change certain fields of published campaign");

                string query;
                if(curCampaignState == State.Published)
                {
                    query = "UPDATE campaigns SET end_date = @endDate, reward_type = @rewardType where id = @id";
                    await connection.ExecuteAsync(query, new { endDate, rewardType, id });
                }
                else
                {
                    query = "UPDATE campaigns SET campaign_name = @campaignName, start_date = @startDate, end_date = @endDate, reward_type = @rewardType, status = @status where id = @id";
                    await connection.ExecuteAsync(query, new { campaignName, startDate, endDate, rewardType, status, id });
                }
            }
        }

        public async Task<int> CloneCampaign(int id)
        {
            using (var connection = new NpgsqlConnection(_connector.ConnectionStrings)) // s seeee 
            {
                var nameQuery = "select case when position('(' in campaign_name) > 0 then substr(campaign_name, 1, position('(' in campaign_name) - 2)" +
                    "else campaign_name end from campaigns where id = @id";
                var campaignName = await connection.QuerySingleOrDefaultAsync<string>(nameQuery, new { id });
                var numNamesQuey = "select count(*) from campaigns where campaign_name like @SearchName";
                var numNames = await connection.QuerySingleOrDefaultAsync<int>(numNamesQuey, new { SearchName = $"{campaignName} (%)", campaignName});

                var campaignToCloneQuery = "select id as Id, create_date as CreateDate, campaign_name as CampaignName, start_date as StartDate, end_date as EndDate, reward_type as RewardType, state as State, status as Status, is_deleted as IsDeleted from campaigns where id = @id";
                var camp = await connection.QuerySingleOrDefaultAsync<Campaign>(campaignToCloneQuery, new {id});

                numNames++;
                camp.CampaignName = campaignName + $" ({numNames})";
                camp.CreateDate = DateTime.UtcNow;
                var query = "INSERT INTO campaigns (create_date, campaign_name, start_date, end_date, reward_type, state, status, is_deleted)" +
                    " values (@CreateDate, @CampaignName, @StartDate, @EndDate, @RewardType, @State, @Status, @IsDeleted) returning id";
                var newId = connection.QuerySingleOrDefaultAsync<int>(query, camp);
                return await newId;
            }
        }

        public async Task<List<Campaign>> GetAllCampaigns()
        {
            using (var connection = new NpgsqlConnection(_connector.ConnectionStrings))
            {
                var campaignToCloneQuery = @"select id as Id, create_date as CreateDate, campaign_name as CampaignName, start_date as StartDate,
                                end_date as EndDate, reward_type as RewardType, state as State, 
                            status as Status, is_deleted as IsDeleted from campaigns
                            ORDER BY create_date DESC";
                var camp = await connection.QueryAsync<Campaign>(campaignToCloneQuery);

                return camp.ToList();
            }
        }

        public async Task<List<Campaign>> FilterCampaigns(Filter filter)
        {
            string? name = filter.CompanyName;
            DateTime? from = filter.StartDate;
            DateTime? to = filter.EndDate;
            RewardType? rt = filter.RewardType;
            State? state = filter.State;
            Status? status = filter.status;

            using (var connection = new NpgsqlConnection(_connector.ConnectionStrings))
            {
                string query = @"SELECT id as Id, create_date as CreateDate, campaign_name as CampaignName, 
                            start_date as StartDate, end_date as EndDate, reward_type as RewardType,
                            state as State, status as Status, is_deleted as IsDeleted  FROM campaigns " +
                 "WHERE (@name IS NULL OR campaign_name LIKE @nameFilter) " +
                 "AND (@from::timestamp IS NULL OR start_date >= @from) " +
                 "AND (@to::timestamp IS NULL OR end_date <= @to) " +
                 "AND (@rt is null OR reward_type = @rt) " +
                 "AND (@state is null OR state = @state) " +
                 "AND (@status is null or status = @status) " +
                 "AND is_deleted = false";
                var campaigns = await connection.QueryAsync<Campaign>(query, new {name, nameFilter = $"%{name}%", from,
                    to, rt, state, status});
                return campaigns.ToList();
            }
        }

        public async Task<List<Campaign>> GetCampaignsByPage(int page)
        {
            using (var connection = new NpgsqlConnection(_connector.ConnectionStrings))
            {
                string query = @"SELECT id as Id, create_date as CreateDate, campaign_name as CampaignName, 
                            start_date as StartDate, end_date as EndDate, reward_type as RewardType,
                            state as State, status as Status, is_deleted as IsDeleted FROM campaigns
                            ORDER BY create_date DESC OFFSET @offset limit @NumCampaignsOnPage";
                var campaigns = await connection.QueryAsync<Campaign>(query, new {offset = (page - 1) * _configuration.GetValue<int>("NumCampaignsOnPage"), NumCampaignsOnPage = _configuration.GetValue<int>("NumCampaignsOnPage") });
                return campaigns.ToList();
            }
        }
    }
}
