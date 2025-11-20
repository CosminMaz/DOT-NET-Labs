using AutoMapper;
namespace OrderManagementAPI.Features.Order.Resolvers;

public class PublishedAgeResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        var age = DateTime.UtcNow - source.PublishedDate;

        if (age.Days < 30)
        {
            return "New Release";
        }
        if (age.Days < 365)
        {
            var months = (int)(age.Days / 30);
            return $"{months} months old";
        }
        if (age.Days < 1825)
        {
            var years = (int)(age.Days / 365);
            return $"{years} years old";
        }
        
        return "Classic";
    }
}
