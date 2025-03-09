using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using DotNetEnv;
using EliteService.EliteServiceManager;
using Microsoft.AspNetCore.Diagnostics;
using FluentValidation.AspNetCore;
using Amazon.DynamoDBv2;

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

if (builder.Environment.IsDevelopment())
{
    Env.Load();
}

var awsAccessKey = builder.Configuration["AWS:AccessKey"];
var awsSecretKey = builder.Configuration["AWS:SecretKey"];

builder.Configuration.AddEnvironmentVariables();

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
