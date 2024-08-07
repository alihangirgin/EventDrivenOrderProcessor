using AutoMapper;
using EventDrivenOrderProcessor.Shared;
using EventDrivenOrderProcessor.SagaOrchestration.Order.Api.Model;
using EventDrivenOrderProcessor.Shared.Constants;
using EventDrivenOrderProcessor.Shared.Events;
using EventDrivenOrderProcessor.Shared.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace EventDrivenOrderProcessor.SagaOrchestration.Order.Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        public OrderController(AppDbContext appDbContext, IMapper mapper, ISendEndpointProvider sendEndpointProvider)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _sendEndpointProvider = sendEndpointProvider;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateOrderDto model)
        {
            var order =  _mapper.Map<Model.Order>(model);
            await _appDbContext.AddAsync(order);
            await _appDbContext.SaveChangesAsync();

            var orderCreatedEvent = _mapper.Map<OrderCreatedStateEvent>(order);
            orderCreatedEvent.PaymentInput = new()
            {
                CVV = model.PaymentInput.CVV,
                CardName = model.PaymentInput.CardName,
                CardNumber = model.PaymentInput.CardNumber,
                Expiration = model.PaymentInput.Expiration,
                TotalPrice = model.OrderedItems.Sum(x=> x.Price * x.Count)
            };

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqConstants.OrderQueue}"));
            await sendEndpoint.Send<IOrderCreatedStateEvent>(orderCreatedEvent);

            return Ok();
        }
    }
}
