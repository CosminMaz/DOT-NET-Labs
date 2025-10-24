using AutoMapper;

namespace OrderManagementAPI.Features.Order.Resolvers
{
    public class AvailabilityStatusResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            if (!source.IsAvailable) // StockQuantity <= 0
            {
                return "Out of Stock";
            }

            if (source.StockQuantity == 1)
            {
                return "Last Copy";
            }

            if (source.StockQuantity <= 5)
            {
                return "Limited Stock";
            }

            return "In Stock";
        }
    }
}
