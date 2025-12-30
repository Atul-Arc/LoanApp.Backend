using LoanApp.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        // Chat (Azure AI Foundry / Azure OpenAI)
        builder.Services.AddOptions<FoundryChatOptions>()
            .Bind(builder.Configuration.GetSection(FoundryChatOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint), "FoundryChat:Endpoint is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Deployment), "FoundryChat:Deployment is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "FoundryChat:ApiKey is required")
            .ValidateOnStart();

        builder.Services.AddSingleton<IChatSessionStore, InMemoryChatSessionStore>();
        builder.Services.AddSingleton<FoundryChatClient>();

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

        app.UseCors();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
