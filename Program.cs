using Amazon.Extensions.NETCore.Setup;
using Amazon.SecretsManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyApi.Models;
using System.Text.Json;
using Amazon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Amazon.SecretsManager.Model;

public partial class Program
{
    public static async Task Main(string[] args)
    { 
        var builder = WebApplication.CreateBuilder(args);

        // Add AWS service client
        builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonSecretsManager>();

        // Add services to the container.
        builder.Services.AddControllers();

        // Load AWS options
        var awsOptions = builder.Configuration.GetAWSOptions();
        awsOptions.Region = Amazon.RegionEndpoint.APSoutheast1; // Ensure the region is set

        // Build service provider to resolve the Secrets Manager client
        var serviceProvider = builder.Services.BuildServiceProvider();
        var secretsManagerClient = serviceProvider.GetRequiredService<IAmazonSecretsManager>();

        // Fetch the secret value
        var secretValueResponse = await secretsManagerClient.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = "UAT/API"
        });

        var secretString = secretValueResponse.SecretString;
        // Assuming the secret is stored as a JSON object
        var secretData = JsonSerializer.Deserialize<Dictionary<string, string>>(secretString);
        var connectionString = secretData["connectionString"];

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}


// Add DbContext
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//{
//    var secretManager = builder.Services.BuildServiceProvider().GetRequiredService<IAmazonSecretsManager>();
//    var secretValue = secretManager.GetSecretValueAsync(new Amazon.SecretsManager.Model.GetSecretValueRequest
//    {
//        SecretId = "UAT/API"
//    }).Result;
//    var secretJson = JsonDocument.Parse(secretValue.SecretString).RootElement;
//    var connectionString = secretJson.GetProperty("ConnectionString").GetString();
//    options.UseMySQL(connectionString );
//});




//var app = builder.Build();



//builder.Services.AddDefaultAWSOptions(awsOptions);
//builder.Services.AddAWSService<IAmazonSecretsManager>();


//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}

//app.UseHttpsRedirection();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();


