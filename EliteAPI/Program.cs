using EliteAPI.EliteAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Ensure this is included.
builder.Services.AddSingleton<EliteManager>();
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

var app = builder.Build();
app.UseCors("AllowLocalhost8080");

// Enable Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Ensure this line is here.
}

app.UseRouting();
app.UseCors();
app.MapControllers();

app.Run();