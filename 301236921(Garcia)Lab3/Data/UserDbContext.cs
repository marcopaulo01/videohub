using _301236921_Garcia_Lab3.Models;
using Microsoft.EntityFrameworkCore;

namespace _301236921_Garcia_Lab3.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
