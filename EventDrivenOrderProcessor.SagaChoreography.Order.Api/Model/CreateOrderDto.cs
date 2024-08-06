using EventDrivenOrderProcessor.SagaChoreography.Shared;

namespace EventDrivenOrderProcessor.SagaChoreography.Order.Api.Model
{
    public class CreateOrderDto
    {
        public string BuyerId { get; set; }
        public PaymentInputDto PaymentInput { get; set; }
        public List<OrderItemDto> OrderedItems { get; set; }
        public AddressDto Address { get; set; }
    }
}
