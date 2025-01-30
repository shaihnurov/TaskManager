using Microsoft.EntityFrameworkCore;
using TaskManager.Model;

namespace TaskManager.Services
{
    public class ApplicationContextDb : DbContext
    {
        public DbSet<TaskModel> Tasks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=tasks.db");
        }
    }
}