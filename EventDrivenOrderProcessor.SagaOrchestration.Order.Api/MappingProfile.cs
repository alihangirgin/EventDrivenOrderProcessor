using AutoMapper;
using EventDrivenOrderProcessor.Shared;
using EventDrivenOrderProcessor.SagaOrchestration.Order.Api.Model;
using EventDrivenOrderProcessor.Shared.Events;

namespace EventDrivenOrderProcessor.SagaOrchestration.Order.Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateOrderDto, Model.Order>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));
            CreateMap<AddressDto, Address>();
            CreateMap<OrderItemDto, OrderItem>();

            CreateMap<Model.Order, OrderCreatedStateEvent>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id));
            CreateMap<OrderItem, OrderItemMessage>();

        }
    }
}
