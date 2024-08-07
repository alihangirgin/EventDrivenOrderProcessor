using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenOrderProcessor.Shared
{
    public class PaymentFailedEvent
    {
        public Guid OrderId { get; set; }
        public string BuyerId { get; set; }
        public string FailMessage { get; set; }
        public List<OrderItemMessage> OrderedItems { get; set; }
    }
}
