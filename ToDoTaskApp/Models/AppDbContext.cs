using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TodoApp.Models;

namespace TodoApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TodoTask> TodoTask { get; set; }
    }
}
