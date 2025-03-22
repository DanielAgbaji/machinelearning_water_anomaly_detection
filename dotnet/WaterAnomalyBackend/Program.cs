using WaterAnomalyBackend.Services;
using WaterAnomalyDetection.Integration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<WaterAnomalyApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AnomalyApi:BaseUrl"]!);
});

builder.Services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
builder.Services.AddScoped<IPredictionRepository, PredictionRepository>();
builder.Services.AddScoped<AnomalyDetectionService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
