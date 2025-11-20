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

        if (names.Length >= 2)
        {
            return $"{names[0][0]}{names[names.Length - 1][0]}".ToUpper();
        }

        return $"{names[0][0]}".ToUpper();
    }
}
