
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace ExecutivesCompensation;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        // See https://aka.ms/aspnetcore/swashbuckle for Swagger / OpenAPI configuration docs.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
         {
             options.SwaggerDoc("v1", new OpenApiInfo { Title = "ExecutivesCompensation API", Description = "Executive compensation for companies listed on the ASX.", Version = "v1" });

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
}
