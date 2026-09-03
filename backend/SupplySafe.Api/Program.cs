using System.Text.Json.Serialization;
using SupplySafe.Api.Application.Interfaces;
using SupplySafe.Api.Application.Services;
using SupplySafe.Api.Infrastructure.AI;
using SupplySafe.Api.Infrastructure.Notifications;
using SupplySafe.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "SupplySafe AI API",
        Version = "v1",
        Description = "Don't wait for the supply chain to break. Act before it does."
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("HackathonCors", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddSingleton<ISupplyRiskEngine, SupplyRiskEngine>();
builder.Services.AddScoped<IAiRiskAnalyzer, GrokRiskAnalyzer>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<RiskAnalysisService>();
builder.Services.AddScoped<IncidentService>();

builder.Services.AddHttpClient("xai", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["XAI:BaseUrl"] ?? "https://api.x.ai");
    client.Timeout = TimeSpan.FromSeconds(3);
});

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "An unexpected error occurred.",
            traceId = context.TraceIdentifier
        });
    });
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SupplySafe AI v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("HackathonCors");
app.MapControllers();

app.Run();
