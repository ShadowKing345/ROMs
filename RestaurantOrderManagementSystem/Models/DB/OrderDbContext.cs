using Microsoft.EntityFrameworkCore;
using RestaurantOrderManagementSystem.Models.Order;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;

namespace RestaurantOrderManagementSystem.Models.DB
{
    public class OrderDbContext: DbContext
    {
        public DbSet<Cart> Carts { get; set; }
        public DbSet<MenuItemOrder> MenuItemOrders { get; set; }
        public DbSet<MenuItemOptionSet> MenuItemOptionSets { get; set; }
        
        public DbSet<CartMenuItemOrder> CartMenuItemOrders { get; set; }
        public DbSet<MenuItemOrderMenuItemOptionSet> MenuItemOrderMenuItemOptionSets { get; set; }
        
        public OrderDbContext(DbContextOptions<OrderDbContext> options): base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CartMenuItemOrder>().HasKey(c => new {c.CartId, c.MenuItemOrderId});
            modelBuilder.Entity<CartMenuItemOrder>()
                .HasOne(x => x.Cart)
                .WithMany(x => x.CartMenuItemOrders);
            modelBuilder.Entity<CartMenuItemOrder>()
                .HasOne(x => x.MenuItemOrder)
                .WithMany(x => x.CartMenuItemOrders);
            
            modelBuilder.Entity<MenuItemOrderMenuItemOptionSet>().HasKey(m => new {m.MenuItemOrderId, m.MenuItemOptionSetId});
            modelBuilder.Entity<MenuItemOrderMenuItemOptionSet>()
                .HasOne(m => m.MenuItemOrder)
                .WithMany(m => m.MenuItemOrderMenuItemOptionSets);
            modelBuilder.Entity<MenuItemOrderMenuItemOptionSet>()
                .HasOne(m => m.MenuItemOptionSet)
                .WithOne(m => m.MenuItemOrderMenuItemOptionSet);
        }
    }
}