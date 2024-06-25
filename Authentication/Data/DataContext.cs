using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Authentication.Models.Products;
using Authentication.Models.Users;

namespace Authentication.Data
{
    public class DataContext : IdentityDbContext<IdentityUser>
	{
		public DataContext(DbContextOptions<DataContext> options) : base(options)
		{

		}

		public DbSet<Product> Products => Set<Product>();
	}
}
