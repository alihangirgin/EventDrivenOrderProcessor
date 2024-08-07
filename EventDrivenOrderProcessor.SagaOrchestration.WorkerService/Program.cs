using EventDrivenOrderProcessor.SagaOrchestration.WorkerService;
using EventDrivenOrderProcessor.SagaOrchestration.WorkerService.Models;
using EventDrivenOrderProcessor.Shared.Constants;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var orderStateConnectionString = builder.Configuration.GetConnectionString("OrderStateDb");

// Configure DbContext
builder.Services.AddDbContext<OrderStateDbContext>(options =>
    options.UseSqlServer(orderStateConnectionString));

builder.Services.AddMassTransit(x =>
{
    // Configure RabbitMQ
    x.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration.GetConnectionString("RabbitMq"));

        // ReceiveEndpoint tanýmý
        configurator.ReceiveEndpoint(RabbitMqConstants.OrderQueue, e =>
        {
            e.ConfigureSaga<OrderStateInstance>(context); 
        });


        configurator.ConfigureEndpoints(context);

    });


    // Configure Saga
    x.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            r.AddDbContext<DbContext, OrderStateDbContext>((provider, optionsBuilder) =>
            {
                optionsBuilder.UseSqlServer(orderStateConnectionString);
            });
        });
});



var host = builder.Build();

using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<OrderStateDbContext>();
dbContext.Database.Migrate();

host.Run();
