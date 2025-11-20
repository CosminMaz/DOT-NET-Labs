using FluentValidation;
using MediatR;
using System.Diagnostics;


namespace OrderManagementAPI.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!validators.Any()) return await next();
            var stopwatch = Stopwatch.StartNew();
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                validators.Select(v =>
                    v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Count != 0)
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
            logger.LogInformation(LogEvents.StockValidationPerformed, "Stock validation performed for {RequestName} took {DurationMs}ms", typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);

            if (failures.Count == 0) return await next();
            var currentMetrics = MetricsContext.CurrentMetrics;
            logger.LogWarning(
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
}