using Microsoft.EntityFrameworkCore;
using RestaurantOrderManagementSystem.Models.Menu;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;

namespace RestaurantOrderManagementSystem.Models.DB
{
    public class MenuItemDbContext : DbContext
    {
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItemOption> MenuItemOptions { get; set; }
        public DbSet<MenuItemCategory> MenuItemCategories { get; set; }
        public DbSet<MenuItemMenuItemOption> MenuItemMenuItemOptions { get; set; }
        
        public MenuItemDbContext(DbContextOptions<MenuItemDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
            
            modelBuilder.Entity<MenuItemCategory>().HasKey(mic => new {mic.MenuItemId, mic.CategoryId});
            modelBuilder.Entity<MenuItemCategory>()
                .HasOne(mic => mic.MenuItem)
                .WithMany(mi => mi.MenuItemCategories)
                .HasForeignKey(mic => mic.MenuItemId);
            modelBuilder.Entity<MenuItemCategory>()
                .HasOne(mic => mic.Category)
                .WithMany(c => c.MenuItemCategories)
                .HasForeignKey(mic => mic.CategoryId);

            modelBuilder.Entity<MenuItemMenuItemOption>().HasKey(mio => new {mio.MenuItemId, mio.OptionId});
            modelBuilder.Entity<MenuItemMenuItemOption>()
                .HasOne(mio => mio.MenuItem)
                .WithMany(mi => mi.MenuItemOptions)
                .HasForeignKey(mio => mio.MenuItemId);
            modelBuilder.Entity<MenuItemMenuItemOption>()
                .HasOne(mio => mio.MenuItemOption)
                .WithMany(mo => mo.MenuItemOptions)
                .HasForeignKey(mio => mio.OptionId);
        }
    }
}