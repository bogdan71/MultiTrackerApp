using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tracker.Api.Models;

namespace Tracker.Api.Data;

public class TrackerDbContext(DbContextOptions<TrackerDbContext> options) 
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Song> Songs => Set<Song>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Required for Identity tables

        modelBuilder.Entity<Book>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Movie>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Song>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<TodoItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Priority).HasConversion<string>();
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Slug).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Item>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
