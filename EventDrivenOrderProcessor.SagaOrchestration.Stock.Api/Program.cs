using EventDrivenOrderProcessor.Shared;
using EventDrivenOrderProcessor.SagaOrchestration.Stock.Api;
using EventDrivenOrderProcessor.SagaOrchestration.Stock.Api.Consumers;
using EventDrivenOrderProcessor.Shared.Constants;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<OrderCreatedEventConsumer>();
    configure.AddConsumer<PaymentFailedEventConsumer>();

    configure.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration.GetConnectionString("RabbitMq"));
        configurator.ReceiveEndpoint(RabbitMqConstants.StockOrderCreatedEventQueueName, endpointConfigurator =>
        {
            endpointConfigurator.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });
        configurator.ReceiveEndpoint(RabbitMqConstants.StockPaymentFailedEventQueueName, endpointConfigurator =>
        {
            endpointConfigurator.ConfigureConsumer<PaymentFailedEventConsumer>(context);
        });
    });
});

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("StockDb")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
dbContext.Database.Migrate();

if (!dbContext.Stocks.Any())
{
    dbContext.Stocks.Add(new() { Count = 100, ProductId = 1 });
    dbContext.Stocks.Add(new() { Count = 50, ProductId = 2 });
    dbContext.Stocks.Add(new() { Count = 2, ProductId = 3 });
    dbContext.SaveChanges();
}


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