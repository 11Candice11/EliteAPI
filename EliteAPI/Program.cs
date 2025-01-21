using System.Text.Json.Serialization;
using EliteService.EliteServiceManager;
using Microsoft.AspNetCore.Diagnostics;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// builder.AddConfiguration();
// builder.ConfigureSecrets();
// builder.ConfigureServices();
// builder.AddAuth();
builder.Services.AddTransient<EliteServiceManager>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers()
    .AddFluentValidation()
    .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
// builder.ConfigureFluentbit();
builder.Services.AddHealthChecks();

// USE WHEN NOT LOCAL
// builder.Services.AddCors(options =>
// {
//     options.AddDefaultPolicy(policy =>
//     {
//         policy.AllowAnyHeader();
//         policy.AllowAnyMethod();
//         policy.AllowCredentials();
//     });
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost8080",
        builder =>
        {
            builder.WithOrigins("http://localhost:8080")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// app.UseSwagger();
// app.UseRouting();
// app.UseCors();
// app.UseAuthentication();
// app.UseAuthorization();
app.MapControllers();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
});

// Enable the CORS policy
app.UseCors("AllowLocalhost8080");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();