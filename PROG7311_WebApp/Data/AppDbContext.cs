using PROG7311_WebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace PROG7311_WebApp.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
    
    

    }
}
