using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderManagementSystem.Accounts;

namespace RestaurantOrderManagementSystem.Models.DB
{
    public class AccountDbContext : IdentityDbContext<ApplicationUser>
    {
        public AccountDbContext(DbContextOptions<AccountDbContext> options)
            : base(options)
        {
        }
    }
}