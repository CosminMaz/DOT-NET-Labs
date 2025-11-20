using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagementAPI;
using OrderManagementAPI.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrderManagementAPI.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var stopwatch = Stopwatch.StartNew();
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v =>
                        v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .Where(r => r.Errors.Any())
                    .SelectMany(r => r.Errors)
                    .ToList();
                
                stopwatch.Stop();
                
                if (MetricsContext.CurrentMetrics != null)
                {
                    MetricsContext.CurrentMetrics = MetricsContext.CurrentMetrics with
                    {
                        ValidationDuration = MetricsContext.CurrentMetrics.ValidationDuration.Add(stopwatch.Elapsed)
                    };
                }

                // Assuming general validation includes stock validation
                _logger.LogInformation(LogEvents.StockValidationPerformed, "Stock validation performed for {RequestName} took {DurationMs}ms", typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);

                if (failures.Any())
                {
                    var currentMetrics = MetricsContext.CurrentMetrics;
                    _logger.LogWarning(
                        LogEvents.OrderValidationFailed,
                        "Validation failed for OperationId: {OperationId}, Title: {OrderTitle}, ISBN: {ISBN}, Category: {Category} after {DurationMs}ms. Errors: {Errors}",
                        currentMetrics?.OperationId,
                        currentMetrics?.OrderTitle,
                        currentMetrics?.ISBN,
                        currentMetrics?.Category,
                        stopwatch.ElapsedMilliseconds,
                        failures
                    );
                    throw new ValidationException(failures);
                }
            }
            return await next();
        }
    }
}