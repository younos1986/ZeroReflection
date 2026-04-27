using System;
using Microsoft.Extensions.DependencyInjection;

namespace ZeroReflection.Api;

/// <summary>
/// Extension methods for running ZeroReflection API in different modes
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Detects if running in AWS Lambda environment
    /// </summary>
    public static bool IsRunningInLambda()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME"));
    }
}
