using EventDrivenOrderProcessor.SagaChoreography.Order.Api;
using EventDrivenOrderProcessor.SagaChoreography.Order.Api.Consumers;
using EventDrivenOrderProcessor.SagaChoreography.Shared;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<PaymentSuccessfulEventConsumer>();
    configure.AddConsumer<PaymentFailedEventConsumer>();
    configure.AddConsumer<StockNotUpdatedEventConsumer>();

    configure.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration.GetConnectionString("RabbitMq"));
        configurator.ReceiveEndpoint(RabbitMqConstants.OrderPaymentSuccessfulEventQueueName, endpointConfigurator =>
        {
            endpointConfigurator.ConfigureConsumer<PaymentSuccessfulEventConsumer>(context);
        });
        configurator.ReceiveEndpoint(RabbitMqConstants.OrderPaymentFailedEventQueueName, endpointConfigurator =>
        {
            endpointConfigurator.ConfigureConsumer<PaymentFailedEventConsumer>(context);
        });
        configurator.ReceiveEndpoint(RabbitMqConstants.OrderStockNotUpdatedEventQueueName, endpointConfigurator =>
        {
            endpointConfigurator.ConfigureConsumer<StockNotUpdatedEventConsumer>(context);
        });
    });
});

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDb")));
builder.Services.AddAutoMapper(typeof(MappingProfile));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
dbContext.Database.Migrate();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
