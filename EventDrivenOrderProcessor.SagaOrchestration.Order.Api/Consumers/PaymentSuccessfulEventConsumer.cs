using EventDrivenOrderProcessor.Shared;
using EventDrivenOrderProcessor.Shared.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenOrderProcessor.SagaOrchestration.Order.Api.Consumers
{
    public class PaymentSuccessfulEventConsumer : IConsumer<IPaymentSuccessfulOrderRequestEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<PaymentSuccessfulEventConsumer> _logger;

        public PaymentSuccessfulEventConsumer(AppDbContext appDbContext, ILogger<PaymentSuccessfulEventConsumer> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IPaymentSuccessfulOrderRequestEvent> context)
        {
            var order = await _appDbContext.Orders.FirstOrDefaultAsync(x => x.Id == context.Message.OrderId);
            if (order == null)
            {
                _logger.LogCritical($"Order OrderId:{context.Message.OrderId} not found");
                return;
            }

            order.Status = Model.OrderStatus.Completed;
            _appDbContext.Orders.Update(order);
            await _appDbContext.SaveChangesAsync();

            _logger.LogInformation($"Order OrderId:{context.Message.OrderId} completed");
        }
    }
}
