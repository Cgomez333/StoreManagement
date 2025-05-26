    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;

    namespace StoreManagement.Models
    {
        public class StoreDbContext : DbContext
        {
            public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
            {
            }

            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<Supplier> Suppliers { get; set; }
            public DbSet<Customer> Customers { get; set; }
            public DbSet<Sale> Sales { get; set; }
            public DbSet<SaleDetail> SaleDetails { get; set; }
            public DbSet<InventoryMovement> InventoryMovements { get; set; }
            public DbSet<User> Users { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);
                builder.Entity<User>()
                    .HasIndex(u => u.Email)
                    .IsUnique();
            }
    }
    }