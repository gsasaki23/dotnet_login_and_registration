using Microsoft.EntityFrameworkCore;

namespace login_reg.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions options) : base(options) { }
        // tables in db
        public DbSet<User> Users { get; set; }
    }
}