using Microsoft.EntityFrameworkCore;

namespace AspNetCore
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<FoodRecord> FoodRecords { get; set; }
        
        public DbSet<Todo> Todos { get; set; }
        
        public DbSet<Meal> Meals { get; set; }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
    }
}