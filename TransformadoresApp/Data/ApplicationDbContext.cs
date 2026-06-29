using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Models;
using TransformadoresApp.Models.Catalogs;

namespace TransformadoresApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Material> Materials => Set<Material>();
        public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
        public DbSet<BomItem> BomItems => Set<BomItem>();
        public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<ItemAttribute> ItemAttributes => Set<ItemAttribute>();
        public DbSet<Item> Items => Set<Item>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación Material → UnitOfMeasure
            modelBuilder.Entity<Material>()
                .HasOne(m => m.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(m => m.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            // BomItem: combinación única Product + Material
            modelBuilder.Entity<BomItem>()
                .HasIndex(b => new { b.ProductId, b.MaterialId })
                .IsUnique();

            modelBuilder.Entity<BomItem>()
                .Property(b => b.QuantityPerUnit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductionOrder>()
                .Property(o => o.Quantity)
                .HasPrecision(18, 2);

            // Categorías jerárquicas
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Item → Category
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Item → UnitOfMeasure
            modelBuilder.Entity<Item>()
                .HasOne(i => i.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(i => i.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            // ItemType como entero
            modelBuilder.Entity<Item>()
                .Property(i => i.ItemType)
                .HasConversion<int>();

            // Código único
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.Code)
                .IsUnique();

            // Decimales
            modelBuilder.Entity<Item>()
                .Property(i => i.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Item>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Item>()
                .Property(i => i.MinimumStock)
                .HasPrecision(18, 2);

            // ItemAttribute → Item
            modelBuilder.Entity<ItemAttribute>()
                .HasOne(a => a.Item)
                .WithMany(i => i.Attributes)
                .HasForeignKey(a => a.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}