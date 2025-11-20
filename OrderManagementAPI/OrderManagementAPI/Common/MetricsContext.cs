namespace OrderManagementAPI.Common
{
    public static class MetricsContext
    {
        private static readonly AsyncLocal<OrderCreationMetrics?> _currentMetrics = new AsyncLocal<OrderCreationMetrics?>();

        public static OrderCreationMetrics? CurrentMetrics
        {
            get => _currentMetrics.Value;
            set => _currentMetrics.Value = value;
        }
    }
}