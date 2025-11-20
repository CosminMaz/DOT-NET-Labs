using AutoMapper;
using System.Globalization;
namespace OrderManagementAPI.Features.Order.Resolvers;

public class PriceFormatterResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        return source.Price.ToString("C2");
    }
}
