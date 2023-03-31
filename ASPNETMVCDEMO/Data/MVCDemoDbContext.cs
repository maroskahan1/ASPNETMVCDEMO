using ASPNETMVCDEMO.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ASPNETMVCDEMO.Data
{
    public class MVCDemoDbContext : DbContext
    {
        public MVCDemoDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobExecutionHistory> JobExecutionHistory { get; set; }
    }
}
