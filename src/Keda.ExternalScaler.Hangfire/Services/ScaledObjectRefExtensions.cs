using System;
using Externalscaler;

namespace HangfireExternalScaler.Services;

public static class ScaledObjectRefExtensions
{
    public static (string Queue, int TargetSize) GetHangfireScalerConfiguration(this ScaledObjectRef scaledObjectRef)
    {
        if (scaledObjectRef is null)
            throw new ArgumentException("scaledObjectRef must be specified");


        if (!scaledObjectRef.ScalerMetadata.ContainsKey("queue") ||
            string.IsNullOrEmpty(scaledObjectRef.ScalerMetadata["queue"]))
            throw new ArgumentException("queue must be specified");

        if (!scaledObjectRef.ScalerMetadata.ContainsKey("targetSize"))
            throw new ArgumentException("targetSize must be specified");

        if (!int.TryParse(scaledObjectRef.ScalerMetadata["targetSize"], out var targetSize))
            throw new ArgumentException("targetSize must be a valid integer");

        return (scaledObjectRef.ScalerMetadata["queue"], targetSize);
    }
}