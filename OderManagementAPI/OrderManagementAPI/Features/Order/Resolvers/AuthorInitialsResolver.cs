using AutoMapper;
using System.Linq;
using System;

namespace OrderManagementAPI.Features.Order.Resolvers
{
    public class AuthorInitialsResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source.Author))
            {
                return "?";
            }

            var nameParts = source.Author.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length > 1)
            {
                return $"{nameParts.First()[0]}{nameParts.Last()[0]}".ToUpper();
            }
            
            if (nameParts.Length == 1)
            {
                return $"{nameParts[0][0]}".ToUpper();
            }

            return "?";
        }
    }
}
