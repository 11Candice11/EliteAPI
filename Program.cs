using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using DotNetEnv;
using EliteService.EliteServiceManager;
using Microsoft.AspNetCore.Diagnostics;
using FluentValidation.AspNetCore;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using EliteService.EliteServiceManager.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddHttpClient<PdfRetrievalService>();
builder.Services.AddTransient<EliteServiceManager>();
builder.Services.AddScoped<ClientService>(); // ✅ Register ClientService
builder.Services.AddHttpContextAccessor();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers()
    .AddFluentValidation()
    .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<DynamoDbService>();

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowSpecificOrigins",
//         builder =>
//         {
//             builder.WithOrigins("http://localhost:8080",
//                     "https://eliteui-gdbgddepedcagxgn.southafricanorth-01.azurewebsites.net")
//                 .AllowAnyHeader()
//                 .AllowAnyMethod();
//         });
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IEliteServiceManager, EliteServiceManager>();

builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    Env.Load();
}

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
app.UseCors("AllowAll");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();
