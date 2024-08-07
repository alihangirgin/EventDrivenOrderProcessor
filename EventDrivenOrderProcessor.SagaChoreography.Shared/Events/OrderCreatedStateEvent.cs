using EventDrivenOrderProcessor.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenOrderProcessor.Shared.Events
{
    public class OrderCreatedStateEvent : IOrderCreatedStateEvent
    {
        public PaymentInputMessage PaymentInput { get; set; }
        public List<OrderItemMessage> OrderedItems { get; set; }
        public Guid OrderId { get; set; }
        public string BuyerId { get; set; }
    }
}
