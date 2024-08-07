using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenOrderProcessor.SagaOrchestration.WorkerService.Models;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenOrderProcessor.SagaOrchestration.WorkerService
{
    public class OrderStateDbContext : SagaDbContext
    {
        public OrderStateDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get
            {
                return new ISagaClassMap[]
                {
                    new OrderStateMap()
                };
            }
        }
    }
}
