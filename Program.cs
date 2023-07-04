using CampaignApi.MiddleWares;
using CampaignApi.Models.Connection;
using CampaignApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICampaignService, CampaignService>();
builder.Services.Configure<Connector>(builder.Configuration.GetSection("Connection"));
builder.Services.AddOptions();
var app = builder.Build();
app.UseMiddleware<ExceptionHandlerMiddleware>();
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.UseRouting();
app.MapControllers();

app.Run();
