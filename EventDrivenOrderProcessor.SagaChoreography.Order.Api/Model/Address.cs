using Microsoft.EntityFrameworkCore;

namespace EventDrivenOrderProcessor.SagaChoreography.Order.Api.Model
{
    [Owned]
    public class Address
    {
        public string City { get; set; }
        public string Province { get; set; }
        public string AddressLine { get; set; }
    }
}
