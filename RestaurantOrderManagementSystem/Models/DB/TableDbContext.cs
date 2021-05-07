using Microsoft.EntityFrameworkCore;
using RestaurantOrderManagementSystem.Models.RelationshipIntermidiary;
using RestaurantOrderManagementSystem.Models.Table;

namespace RestaurantOrderManagementSystem.Models.DB
{
    public class TableDbContext: DbContext
    {
        public DbSet<Table.Table> Tables { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<TableComponent> TableComponent { get; set; }
        public DbSet<TableComponentTableComponent> TableComponentTableComponent { get; set; }

        public TableDbContext(DbContextOptions<TableDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TableComponentTableComponent>().HasKey(a => new { AreaId = a.TableComponent1Id, TableId = a.TableComponent2Id });
            modelBuilder.Entity<TableComponentTableComponent>()
                .HasOne(e => e.TableComponent1)
                .WithMany(e => e.TableComponentTableComponents1);
            modelBuilder.Entity<TableComponentTableComponent>()
                .HasOne(e => e.TableComponent2)
                .WithMany(e => e.TableComponentTableComponents2);
        }
        
        
    }
}