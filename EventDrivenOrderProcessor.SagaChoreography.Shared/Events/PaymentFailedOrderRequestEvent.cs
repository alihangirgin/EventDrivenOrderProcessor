using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenOrderProcessor.Shared.Interfaces;

namespace EventDrivenOrderProcessor.Shared.Events
{
    public class PaymentFailedOrderRequestEvent : IPaymentFailedToAllRequestEvent
    {
        public Guid CorrelationId { get; set; }
        public Guid OrderId { get; set; }
        public string BuyerId { get; set; }
        public string FailMessage { get; set; }
        public List<OrderItemMessage> OrderedItems { get; set; }
    }
}
