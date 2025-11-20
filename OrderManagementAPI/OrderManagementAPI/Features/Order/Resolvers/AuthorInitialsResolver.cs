using AutoMapper;
namespace OrderManagementAPI.Features.Order.Resolvers;

public class AuthorInitialsResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        var names = source.Author?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (names == null || names.Length == 0)
        {
            return "?";
        }

        return names.Length >= 2 ? $"{names[0][0]}{names[^1][0]}".ToUpper() : $"{names[0][0]}".ToUpper();
    }
}
