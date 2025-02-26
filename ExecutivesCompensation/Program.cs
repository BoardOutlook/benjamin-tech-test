
using System.Net;
using System.Reflection;
using ExecutivesCompensation.Clients;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

namespace ExecutivesCompensation;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Required configuration.
        string? apiKey = builder.Configuration["ServiceApiKey"];
        if (apiKey == null)
        {
            throw new ArgumentException("Configuration is missing ServiceApiKey. Please run `dotnet user-secrets set \"ServiceApiKey\": <value>`");
        }
        string? companyInfoServiceBaseUrl = builder.Configuration["CompanyInfoServiceBaseUrl"];
        if (companyInfoServiceBaseUrl == null)
        {
            throw new ArgumentException("Configuration is missing CompanyInfoServiceBaseUrl.");
        }

        // TODO: tune cache size, etc.
        MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        // Register the ICompanyInfoClient dependency with dependency injection.
        builder.Services.AddHttpClient<ICompanyInfoClient, CachingCompanyInfoClient>(client =>
        {
            client.BaseAddress = new Uri(companyInfoServiceBaseUrl);
            var companyInfoClient = new CompanyInfoClient(client, apiKey);
            return new CachingCompanyInfoClient(companyInfoClient, cache);
        })
            // Set 5 min as the lifetime for the HttpMessageHandler objects in the pool.
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

        builder.Services.AddControllers();

        // See https://aka.ms/aspnetcore/swashbuckle for Swagger / OpenAPI configuration docs.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo { Title = "ExecutivesCompensation API", Description = "Executive compensation for companies listed on the ASX.", Version = "v1" });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExecutivesCompensation API V1");
               });
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    // Allow retrying transient HTTP errors with exponential backoff.
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(retryCount: 6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
