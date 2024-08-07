
using Microsoft.EntityFrameworkCore;


namespace EventDrivenOrderProcessor.SagaOrchestration.Stock.Api
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        public DbSet<Stock.Api.Model.Stock> Stocks { get; set; }
    }
}
