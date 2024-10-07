using BlazorMySQLMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorMySQLMinimalAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
    }
}
