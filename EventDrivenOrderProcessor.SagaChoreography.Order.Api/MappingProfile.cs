using AutoMapper;
using EventDrivenOrderProcessor.SagaChoreography.Order.Api.Model;
using EventDrivenOrderProcessor.Shared;

namespace EventDrivenOrderProcessor.SagaChoreography.Order.Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateOrderDto, Model.Order>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));
            CreateMap<AddressDto, Address>();
            CreateMap<OrderItemDto, OrderItem>();

            CreateMap<Model.Order, OrderCreatedEvent>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id));
            CreateMap<OrderItem, OrderItemMessage>();

        }
    }
}
