using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagementAPI.Common.Middleware
{
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationMiddleware> _logger;
        private const string CorrelationHeaderName = "X-Correlation-ID";

        public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId = GetCorrelationId(context);
            context.Items["CorrelationId"] = correlationId; // Store in HttpContext for later access

            // Add CorrelationId to the logging scope
            using (_logger.BeginScope(new Dictionary<string, object> { { "CorrelationId", correlationId } }))
            {
                await _next(context);
            }
        }

        private string GetCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationHeaderName, out var correlationIds))
            {
                return correlationIds.FirstOrDefault() ?? Guid.NewGuid().ToString();
            }
            return Guid.NewGuid().ToString();
        }
    }
}