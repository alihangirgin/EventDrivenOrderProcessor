using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace EventDrivenOrderProcessor.SagaOrchestration.WorkerService.Models
{
    public class OrderStateMap : SagaClassMap<OrderStateInstance>
    {
    }
}
