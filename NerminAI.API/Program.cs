using NerminAI.Application;
using NerminAI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Clean Architecture DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS for Streamlit frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8501", "https://nerminai.streamlit.app")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
