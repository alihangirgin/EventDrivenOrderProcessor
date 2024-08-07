using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenOrderProcessor.Shared;
using EventDrivenOrderProcessor.Shared.Constants;
using EventDrivenOrderProcessor.Shared.Events;
using EventDrivenOrderProcessor.Shared.Interfaces;
using MassTransit;

namespace EventDrivenOrderProcessor.SagaOrchestration.WorkerService.Models
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<IOrderCreatedStateEvent> OrderCreatedStateEvent { get; set; }
        public Event<IOrderCreatedStockRequestEvent> OrderCreatedRequestEvent { get; set; }
        public Event<IStockUpdatedStateEvent> StockUpdatedStateEvent { get; set; }
        public Event<IStockNotUpdatedStateEvent> StockNotUpdatedStateEvent { get; set; }
        public Event<IStockUpdatedPaymentRequestEvent> StockUpdatedPaymentRequestEvent { get; set; }
        public Event<IStockNotUpdatedOrderRequestEvent> StockNotUpdatedOrderRequestEvent { get; set; }
        public Event<IPaymentFailedStateEvent> PaymentFailedStateEvent { get; set; }
        public Event<IPaymentSuccessfulStateEvent> PaymentSuccessStateEvent { get; set; }
        public State OrderCreated { get; set; }
        public State StockUpdated { get; set; }
        public State StockNotUpdated { get; set; }
        public State PaymentSuccessful { get; set; }
        public State PaymentFailed { get; set; }
        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderCreatedStateEvent,
                y => y.CorrelateBy<Guid>(x => x.OrderId, z => z.Message.OrderId).SelectId(context => Guid.NewGuid()));

            // Define state transitions
            Initially(
                When(OrderCreatedStateEvent)
                    .Then(context =>
                    {
                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.BuyerId = context.Message.BuyerId;
                        context.Saga.CreatedAt = DateTime.Now;
                        context.Saga.CardNumber = context.Data.PaymentInput.CardNumber;
                        context.Saga.CVV = context.Data.PaymentInput.CVV;
                        context.Saga.Expiration = context.Data.PaymentInput.Expiration;
                        context.Saga.TotalPrice = context.Data.PaymentInput.TotalPrice;
                        context.Saga.CardName = context.Data.PaymentInput.CardName;
                    })
                    .Publish(context => new OrderCreatedStockRequestEvent()
                    {
                        CorrelationId   =  context.Saga.CorrelationId,
                        BuyerId = context.Data.BuyerId,
                        OrderId = context.Data.OrderId,
                        PaymentInput = context.Data.PaymentInput,
                        OrderedItems = context.Data.OrderedItems,
                    })
                    .TransitionTo(OrderCreated)
            );

            During(OrderCreated, When(StockNotUpdatedStateEvent).TransitionTo(StockNotUpdated)
                .Send(new Uri($"queue:{RabbitMqConstants.OrderStockNotUpdatedEventQueueName}"), context =>
                    new StockNotUpdatedOrderRequestEvent()
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        BuyerId = context.Data.BuyerId,
                        OrderId = context.Data.OrderId,
                        FailMessage = context.Message.FailMessage,
                    }),
                When(StockUpdatedStateEvent).TransitionTo(StockUpdated)
                    .Send(new Uri($"queue:{RabbitMqConstants.PaymentStockUpdatedEventQueueName}"), context =>
                        new StockUpdatedPaymentRequestEvent()
                        {
                            CorrelationId = context.Data.CorrelationId,
                            OrderedItems = context.Data.OrderedItems,
                            BuyerId = context.Data.BuyerId,
                            PaymentInput = context.Data.PaymentInput,
                            OrderId = context.Data.OrderId
                        })

            );

            During(StockUpdated,
                When(PaymentFailedStateEvent).TransitionTo(PaymentFailed)
                    .Then(context => { Console.WriteLine("Hebele"); })
                .Publish(context =>
                    new PaymentFailedToAllRequestEvent()
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        BuyerId = context.Data.BuyerId,
                        OrderId = context.Data.OrderId,
                        FailMessage = context.Message.FailMessage,
                    }),
                            When(PaymentSuccessStateEvent).TransitionTo(PaymentSuccessful)
                    .Then(context => { Console.WriteLine("Hübele"); })
                .Send(new Uri($"queue:{RabbitMqConstants.OrderPaymentSuccessfulEventQueueName}"), context =>
                    new PaymentSuccessfulOrderRequestEvent()
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        BuyerId = context.Data.BuyerId,
                        OrderId = context.Data.OrderId
                    })
            );
        }
    }
}
