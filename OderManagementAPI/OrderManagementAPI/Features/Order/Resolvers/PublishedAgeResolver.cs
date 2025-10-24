using AutoMapper;
using System;

namespace OrderManagementAPI.Features.Order.Resolvers
{
    public class PublishedAgeResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            var timeSincePublished = DateTime.Now - source.PublishedDate;

            if (timeSincePublished.TotalDays < 30)
            {
                return "New Release";
            }
            else if (timeSincePublished.TotalDays < 365)
            {
                int months = (int)(timeSincePublished.TotalDays / (365.25 / 12));
                return $"{months} months old";
            }
            else if (timeSincePublished.TotalDays < 1825) // 5 years
            {
                int years = (int)(timeSincePublished.TotalDays / 365.25);
                return $"{years} years old";
            }
            else
            {
                return "Classic";
            }
        }
    }
}
