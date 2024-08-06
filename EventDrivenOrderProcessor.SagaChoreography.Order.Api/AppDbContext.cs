using EventDrivenOrderProcessor.SagaChoreography.Order.Api.Model;
using Microsoft.EntityFrameworkCore;


namespace EventDrivenOrderProcessor.SagaChoreography.Order.Api
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
