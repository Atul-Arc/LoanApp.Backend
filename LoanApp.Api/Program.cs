using LoanApp.Api.Middleware;
using LoanApp.Application.Configuration;
using LoanApp.Application.Interfaces;
using LoanApp.Infrastructure.Data;
using LoanApp.Infrastructure.Queries;
using LoanApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "LoanApp API", Version = "v1" });
        });

        builder.Services.AddDbContext<LoanAppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("LoanApp")));

        builder.Services.AddScoped<ILoanTypeQuery, LoanTypeQuery>();
        builder.Services.AddScoped<ILoanEligibilityService, LoanEligibilityService>();

        // Chat (Azure AI Foundry / Azure OpenAI) - legacy
        builder.Services.AddOptions<FoundryChatOptions>()
            .Bind(builder.Configuration.GetSection(FoundryChatOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint), "FoundryChat:Endpoint is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Deployment), "FoundryChat:Deployment is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "FoundryChat:ApiKey is required")
            .ValidateOnStart();

        // Chat (Azure AI Foundry Agent via Azure AI Projects) - v2
        builder.Services.AddOptions<FoundryChatV2Options>()
            .Bind(builder.Configuration.GetSection(FoundryChatV2Options.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint), "FoundryChatV2:Endpoint is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.AgentName), "FoundryChatV2:AgentName is required")
            .ValidateOnStart();

        builder.Services.AddSingleton<IChatSessionStore, InMemoryChatSessionStore>();
        builder.Services.AddSingleton<IChatService, FoundryChatClient>();
        builder.Services.AddSingleton<IChatV2Service, FoundryChatV2Client>();

        // Add CORS services
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    var allowedOrigin = builder.Configuration.GetValue<string>("Cors:AllowedOrigins");
                    if (!string.IsNullOrWhiteSpace(allowedOrigin))
                    {
                        policy.WithOrigins(allowedOrigin)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoanApp API v1");
            });
        }

        // app.UseHttpsRedirection();

        // Centralized exception handling should be early in the pipeline.
        app.UseMiddleware<ApiExceptionHandlingMiddleware>();

        app.UseCors();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
