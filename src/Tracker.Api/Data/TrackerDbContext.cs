using Microsoft.EntityFrameworkCore;
using Tracker.Api.Models;

namespace Tracker.Api.Data;

public class TrackerDbContext(DbContextOptions<TrackerDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Song> Songs => Set<Song>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Movie>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Song>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>();
        });

        modelBuilder.Entity<TodoItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Priority).HasConversion<string>();
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "The Winds of Winter", Author = "George R.R. Martin", Genre = "Fantasy", ReleaseDate = new DateOnly(2026, 12, 1), Status = TrackingStatus.Upcoming, Notes = "The long-awaited sixth novel in A Song of Ice and Fire" },
            new Book { Id = 2, Title = "Project Hail Mary 2", Author = "Andy Weir", Genre = "Sci-Fi", ReleaseDate = new DateOnly(2026, 6, 15), Status = TrackingStatus.Upcoming },
            new Book { Id = 3, Title = "Dune Messiah", Author = "Frank Herbert", Genre = "Sci-Fi", Status = TrackingStatus.Completed, Notes = "Re-read before the movie" }
        );

        modelBuilder.Entity<Movie>().HasData(
            new Movie { Id = 1, Title = "Avatar 3", Director = "James Cameron", Genre = "Sci-Fi", ReleaseDate = new DateOnly(2026, 12, 19), Status = TrackingStatus.Upcoming },
            new Movie { Id = 2, Title = "The Batman Part II", Director = "Matt Reeves", Genre = "Action", ReleaseDate = new DateOnly(2027, 10, 1), Status = TrackingStatus.Upcoming },
            new Movie { Id = 3, Title = "Oppenheimer", Director = "Christopher Nolan", Genre = "Drama", Status = TrackingStatus.Completed, Notes = "Amazing cinematography" }
        );

        modelBuilder.Entity<Song>().HasData(
            new Song { Id = 1, Title = "Midnight Rain", Artist = "Taylor Swift", Album = "Midnights", Genre = "Pop", Status = TrackingStatus.Completed },
            new Song { Id = 2, Title = "New Album Drop", Artist = "Kendrick Lamar", Genre = "Hip-Hop", ReleaseDate = new DateOnly(2026, 3, 1), Status = TrackingStatus.Upcoming },
            new Song { Id = 3, Title = "Renaissance Act II", Artist = "Beyoncé", Genre = "R&B", ReleaseDate = new DateOnly(2026, 5, 20), Status = TrackingStatus.Upcoming }
        );

        modelBuilder.Entity<TodoItem>().HasData(
            new TodoItem { Id = 1, Title = "Set up home theater for Avatar 3", Description = "Buy new speakers and calibrate display", DueDate = new DateOnly(2026, 12, 1), Priority = Priority.High, Category = "Entertainment" },
            new TodoItem { Id = 2, Title = "Pre-order The Winds of Winter", DueDate = new DateOnly(2026, 11, 1), Priority = Priority.Medium, Category = "Books" },
            new TodoItem { Id = 3, Title = "Create Spotify playlist for new releases", Priority = Priority.Low, IsCompleted = true, Category = "Music" }
        );
    }
}
