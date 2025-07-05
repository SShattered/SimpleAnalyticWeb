using Microsoft.EntityFrameworkCore;
using SimpleAnalyticApp.Models;

namespace SimpleAnalyticApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<TaskModel> Tasks { get; set; }
    }
}
