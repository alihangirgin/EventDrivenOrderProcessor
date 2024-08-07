using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenOrderProcessor.Shared.Interfaces
{
    public interface IStockNotUpdatedOrderRequestEvent
    {
        public Guid CorrelationId { get; set; }
        public Guid OrderId { get; set; }
        public string BuyerId { get; set; }
        public string FailMessage { get; set; }
    }
}
