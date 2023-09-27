using System;
using System.Threading.Tasks;
using Externalscaler;
using Google.Protobuf.Collections;
using Grpc.Core;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

namespace HangfireExternalScaler.Services;

public class ExternalScalerService : ExternalScaler.ExternalScalerBase
{
    private readonly IMonitoringApi _monitoringApi;
    private readonly ILogger<ExternalScalerService> _logger;

    public ExternalScalerService(IMonitoringApi monitoringApi, ILogger<ExternalScalerService> logger) : base()
    {
        _monitoringApi = monitoringApi;
        _logger = logger;
    }

    public override Task<IsActiveResponse> IsActive(ScaledObjectRef scaledObjectRef, ServerCallContext context)
    {
        var scalerConfiguration = scaledObjectRef.GetHangfireScalerConfiguration();

        var enqueuedCount = _monitoringApi.EnqueuedCount(scalerConfiguration.Queue);
        var fetchedCount = _monitoringApi.FetchedCount(scalerConfiguration.Queue);

        var isActive = true;

        // Only allow scaling to 0 when fetchedCount (jobs currently being processed)
        // is also 0
        if (enqueuedCount == 0 && fetchedCount == 0)
        {
            isActive = false;
        }

        _logger.LogDebug("Enqueued/Fetched: {EnqueuedCount}/{FetchedCount}",
            enqueuedCount, fetchedCount);
        _logger.LogDebug("IsActive: {IsActive}", isActive);

        var isActiveResponse = new IsActiveResponse() { Result = isActive };
        return Task.FromResult(isActiveResponse);
    }

    public override Task<GetMetricSpecResponse> GetMetricSpec(ScaledObjectRef scaledObjectRef, ServerCallContext context)
    {
        _logger.LogInformation("GetMetricSpec: {Name}", scaledObjectRef.Name);
        _logger.LogDebug("ScaledObjectRef {@ScaledObjectRef}", scaledObjectRef);

        var scalerConfiguration = scaledObjectRef.GetHangfireScalerConfiguration();
        var response = new GetMetricSpecResponse();
        var fields = new RepeatedField<MetricSpec>
        {
            new MetricSpec()
            {
                MetricName = "ScaleRecommendation",
                TargetSize = scalerConfiguration.TargetSize
            }
        };

        response.MetricSpecs.Add(fields);

        return Task.FromResult(response);

    }

    public override Task<GetMetricsResponse> GetMetrics(GetMetricsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("ScaledObjectRef {@ScaledObjectRef}", request.ScaledObjectRef);

        var scalerConfiguration =
            request.ScaledObjectRef.GetHangfireScalerConfiguration();

        _logger.LogDebug("Retrieving metrics for {Queue}", scalerConfiguration.Queue);

        var enqueuedCount = _monitoringApi.EnqueuedCount(scalerConfiguration.Queue);

        _logger.LogDebug("Enqueued: {EnqueuedCount}", enqueuedCount);

        var response = new GetMetricsResponse();

        var queueLength = enqueuedCount;

        response.MetricValues.Add(new MetricValue()
        {
            MetricName = "queueLength",
            MetricValue_ = queueLength
        });

        _logger.LogDebug("QueueLength: {QueueLength}", queueLength);

        return Task.FromResult(response);
    }
}