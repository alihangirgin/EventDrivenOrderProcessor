using EventDrivenOrderProcessor.SagaOrchestration.Order.Api.Model;
using Microsoft.EntityFrameworkCore;


namespace EventDrivenOrderProcessor.SagaOrchestration.Order.Api
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Model.Order> Orders { get; set; }
    }
}
