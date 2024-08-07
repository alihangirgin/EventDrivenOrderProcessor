using AutoMapper;
using EventDrivenOrderProcessor.SagaChoreography.Order.Api.Model;
using EventDrivenOrderProcessor.Shared;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventDrivenOrderProcessor.SagaChoreography.Order.Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        public OrderController(AppDbContext appDbContext, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateOrderDto model)
        {
            var order =  _mapper.Map<Model.Order>(model);
            await _appDbContext.AddAsync(order);
            await _appDbContext.SaveChangesAsync();

            var orderCreatedEvent = _mapper.Map<OrderCreatedEvent>(order);
            orderCreatedEvent.PaymentInput = new()
            {
                CVV = model.PaymentInput.CVV,
                CardName = model.PaymentInput.CardName,
                CardNumber = model.PaymentInput.CardNumber,
                Expiration = model.PaymentInput.Expiration,
                TotalPrice = model.OrderedItems.Sum(x=> x.Price * x.Count)
            };

            await _publishEndpoint.Publish(orderCreatedEvent);

            return Ok();
        }
    }
}
