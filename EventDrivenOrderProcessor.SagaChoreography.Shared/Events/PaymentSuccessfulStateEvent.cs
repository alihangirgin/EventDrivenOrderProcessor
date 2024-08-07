using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenOrderProcessor.Shared.Interfaces;

namespace EventDrivenOrderProcessor.Shared.Events
{
    public class PaymentSuccessfulStateEvent : IPaymentSuccessfulStateEvent
    {
        public Guid CorrelationId { get; set; }
        public Guid OrderId { get; set; }
        public string BuyerId { get; set; }
    }
}
