using Api.Extensions;
using BuildingBlocks.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddSharedInfrastructure();

var app = builder.Build();

app.UserSharedInfrastructure();
app.MapMinimalEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
