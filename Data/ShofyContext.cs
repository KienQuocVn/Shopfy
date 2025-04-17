using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Shofy.Models;

namespace Shofy.Data
{
    public class ShofyContext : DbContext
    {
        public ShofyContext (DbContextOptions<ShofyContext> options) : base(options)
        {

        }

        public DbSet<Shofy.Models.User> User { get; set; }

        public DbSet<Shofy.Models.Product> Product { get; set; }

        public DbSet<Shofy.Models.Cart> Cart { get; set; }

        public DbSet<Shofy.Models.CartItem> CartItem { get; set; }

        public DbSet<Shofy.Models.Order> Order { get; set; }

        public DbSet<Shofy.Models.OrderDetail> OrderDetail { get; set; }

        public DbSet<Shofy.Models.Payment> Payment { get; set; }

        public DbSet<Shofy.Models.Review> Review { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Cart>().ToTable("Carts");
            modelBuilder.Entity<CartItem>().ToTable("CartItems");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetails");
            modelBuilder.Entity<Payment>().ToTable("Payments");
            modelBuilder.Entity<Review>().ToTable("Reviews");

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserID)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserID);

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.UserID, r.ProductID });

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.CartID);

            modelBuilder.Entity<OrderDetail>()
                .HasIndex(od => od.OrderID);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.OrderID);

                
        }
    }
}