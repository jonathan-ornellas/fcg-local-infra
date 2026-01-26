using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext().WriteTo.Console());

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHealthChecks();

builder.Services.AddOpenTelemetry()
.ConfigureResource(r => r.AddService(
    serviceName: "fcg-gateway",
    serviceVersion: "1.0.0"))
.WithTracing(t =>
{
    t.AddAspNetCoreInstrumentation();
    t.AddHttpClientInstrumentation();
    t.AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://jaeger:4317"));
});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseRouting();

// Prometheus - Métricas HTTP (requests, duração, status codes)
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("service", ctx => "fcg-gateway");
    options.AddCustomLabel("method", ctx => ctx.Request.Method);
});

// Endpoint para expor métricas no formato Prometheus
app.MapMetrics();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service="fcg-gateway", routes="YARP", metrics="/metrics" }));
app.MapReverseProxy();

app.Run();
