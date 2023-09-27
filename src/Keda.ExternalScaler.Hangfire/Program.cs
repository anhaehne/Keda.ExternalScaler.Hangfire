using Hangfire;
using HangfireExternalScaler.Interceptors;
using HangfireExternalScaler.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));
builder.Services.AddScoped(s => s.GetRequiredService<JobStorage>().GetMonitoringApi());
builder.Services.AddGrpc(
            options =>
            {
                options.Interceptors.Add<ExceptionInterceptor>();
            });

var app = builder.Build();

app.UseRouting();
app.MapGrpcService<ExternalScalerService>();
app.MapGet("/", async context => await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909"));

app.Run();