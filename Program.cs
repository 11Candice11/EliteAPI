using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using DotNetEnv;
using EliteService.EliteServiceManager;
using Microsoft.AspNetCore.Diagnostics;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<EliteServiceManager>();
builder.Services.AddHttpContextAccessor();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers()
    .AddFluentValidation()
    .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<DynamoDbService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:8080",
                    "https://eliteui-gdbgddepedcagxgn.southafricanorth-01.azurewebsites.net")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IEliteServiceManager, EliteServiceManager>();

builder.Services.AddControllers();

//
// // Add AWS Options from Configuration (appsettings.json or environment variables)
// builder.Services.AddAWSService<IAmazonDynamoDB>();
//
// // Register AmazonDynamoDBClient
// builder.Services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
//
// // Register DynamoDBService
// builder.Services.AddSingleton<IDynamoDBService, DynamoDBService>();

if (builder.Environment.IsDevelopment())
{
    Env.Load();
}

var awsAccessKey = builder.Configuration["AWS:AccessKey"];
var awsSecretKey = builder.Configuration["AWS:SecretKey"];

Console.WriteLine($"AWS Access Key: {awsAccessKey}");
Console.WriteLine($"AWS Secret Key: {awsSecretKey}");

builder.Configuration.AddEnvironmentVariables();

// Explicitly load environment variables
// builder.Configuration
//     .SetBasePath(Directory.GetCurrentDirectory())
//     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
//     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
//     .AddEnvironmentVariables();  // Ensure this is present
//
// // Debugging: Print out values
// Console.WriteLine($"AWS Access Key from IConfiguration: {builder.Configuration["AWS:AccessKey"]}");
// Console.WriteLine($"AWS Secret Key from IConfiguration: {builder.Configuration["AWS:SecretKey"]}");
// Console.WriteLine($"AWS Access Key from Environment: {Environment.GetEnvironmentVariable("AWS__AccessKey")}");
// Console.WriteLine($"AWS Secret Key from Environment: {Environment.GetEnvironmentVariable("AWS__SecretKey")}");

var app = builder.Build();

app.MapControllers();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
});

// Enable the CORS policy
app.UseCors("AllowSpecificOrigins");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();
