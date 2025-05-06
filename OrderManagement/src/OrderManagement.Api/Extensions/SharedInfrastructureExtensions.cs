using BuildingBlocks.EfCore;
using BuildingBlocks.EFCore;
using BuildingBlocks.Jwt;
using BuildingBlocks.Logging;
using BuildingBlocks.Mapster;
using BuildingBlocks.OpenApi;
using BuildingBlocks.ProblemDetails;
using BuildingBlocks.Web;
using Figgle;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using OrderManagement;
using OrderManagement.Data;
using OrderManagement.Data.Seed;
using OrderManagement.Identities.Extensions.Infrastructure;
using Serilog;

namespace Api.Extensions;

public static class SharedInfrastructureExtensions
{
    public static WebApplicationBuilder AddSharedInfrastructure(this WebApplicationBuilder builder)
    {
        var appOptions = builder.Services.GetOptions<AppOptions>(nameof(AppOptions));
        Console.WriteLine(FiggleFonts.Standard.Render(appOptions.Name));

        builder.Services.AddRazorPages();

        builder.AddCustomSerilog(builder.Environment);
        builder.Services.AddJwt();
        builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers();
        builder.Services.AddAspnetOpenApi();
        builder.Services.AddCustomVersioning();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddCustomMediatR();

        builder.AddMinimalEndpoints(assemblies: typeof(OrderManagementRoot).Assembly);
        builder.Services.AddValidatorsFromAssembly(typeof(OrderManagementRoot).Assembly);
        builder.Services.AddCustomMapster(typeof(OrderManagementRoot).Assembly);

        builder.AddCustomDbContext<AppDbContext>();
        builder.Services.AddScoped<IDataSeeder, DataSeeder>();
        builder.AddCustomIdentityServer();

        builder.Services.Configure<ForwardedHeadersOptions>(
            options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

        builder.Services.Configure<ApiBehaviorOptions>(
            options => options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddProblemDetails();

        return builder;
    }


    public static WebApplication UserSharedInfrastructure(this WebApplication app)
    {
        var appOptions = app.Configuration.GetOptions<AppOptions>(nameof(AppOptions));

        app.UseCustomProblemDetails();

        app.UseSerilogRequestLogging(
            options =>
            {
                options.EnrichDiagnosticContext = LogEnrichHelper.EnrichFromRequest;
            });

        app.MapGet("/", x => x.Response.WriteAsync(appOptions.Name));

        if (app.Environment.IsDevelopment())
        {
            app.UseAspnetOpenApi();
        }

        app.UseForwardedHeaders();
        app.UseMigration<AppDbContext>();

        app.UseStaticFiles();

        app.UseIdentityServer();

   // Correct middleware ordering
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages()
        .RequireAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });


        return app;
    }
}
