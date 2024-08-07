using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenOrderProcessor.Shared.Interfaces
{
    public interface IOrderCreatedStateEvent 
    {
        public Guid OrderId { get; set; }
        public string BuyerId { get; set; }
        public PaymentInputMessage PaymentInput { get; set; }
        public List<OrderItemMessage> OrderedItems { get; set; }
    }
}
