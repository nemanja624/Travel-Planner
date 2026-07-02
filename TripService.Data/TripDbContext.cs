using Microsoft.EntityFrameworkCore;
using TripService.Data.Models;

namespace TripService.Data;

public sealed class TripDbContext : DbContext
{
    public TripDbContext(DbContextOptions<TripDbContext> options)
        : base(options)
    {
    }

    public DbSet<Trip> Trips => Set<Trip>();

    public DbSet<Destination> Destinations => Set<Destination>();

    public DbSet<TripActivity> Activities => Set<TripActivity>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();

    public DbSet<ShareLink> ShareLinks => Set<ShareLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) // kako ce podaci biti napravljeni u bazi 
    {
        ConfigureTrips(modelBuilder);
        ConfigureDestinations(modelBuilder);
        ConfigureActivities(modelBuilder);
        ConfigureExpenses(modelBuilder);
        ConfigureChecklistItems(modelBuilder);
        ConfigureShareLinks(modelBuilder);
    }

    private static void ConfigureTrips(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.ToTable("Trips");

            entity.HasKey(trip => trip.Id);

            entity.Property(trip => trip.Title)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(trip => trip.Description)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(trip => trip.PlannedBudget)
                .HasColumnType("decimal(18,2)");

            entity.Property(trip => trip.Notes)
                .HasMaxLength(2000)
                .IsRequired();

            entity.HasMany(trip => trip.Destinations)
                .WithOne(destination => destination.Trip)
                .HasForeignKey(destination => destination.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(trip => trip.Activities)
                .WithOne(activity => activity.Trip)
                .HasForeignKey(activity => activity.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(trip => trip.Expenses)
                .WithOne(expense => expense.Trip)
                .HasForeignKey(expense => expense.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(trip => trip.ChecklistItems)
                .WithOne(item => item.Trip)
                .HasForeignKey(item => item.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(trip => trip.ShareLinks)
                .WithOne(link => link.Trip)
                .HasForeignKey(link => link.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureDestinations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Destination>(entity =>
        {
            entity.ToTable("Destinations");

            entity.HasKey(destination => destination.Id);

            entity.Property(destination => destination.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(destination => destination.Location)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(destination => destination.Description)
                .HasMaxLength(1000)
                .IsRequired();
        });
    }

    private static void ConfigureActivities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TripActivity>(entity =>
        {
            entity.ToTable("Activities");

            entity.HasKey(activity => activity.Id);

            entity.Property(activity => activity.Title)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(activity => activity.Location)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(activity => activity.Description)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(activity => activity.EstimatedCost)
                .HasColumnType("decimal(18,2)");

            entity.Property(activity => activity.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
        });
    }

    private static void ConfigureExpenses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("Expenses");

            entity.HasKey(expense => expense.Id);

            entity.Property(expense => expense.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(expense => expense.Category)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(expense => expense.Amount)
                .HasColumnType("decimal(18,2)");

            entity.Property(expense => expense.Description)
                .HasMaxLength(1000)
                .IsRequired();
        });
    }

    private static void ConfigureChecklistItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChecklistItem>(entity =>
        {
            entity.ToTable("ChecklistItems");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.Text)
                .HasMaxLength(200)
                .IsRequired();
        });
    }

    private static void ConfigureShareLinks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShareLink>(entity =>
        {
            entity.ToTable("ShareLinks");

            entity.HasKey(link => link.Id);

            entity.Property(link => link.AccessLevel)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(link => link.TokenHash)
                .HasMaxLength(128)
                .IsRequired();

            entity.HasIndex(link => link.TokenHash)
                .IsUnique();
        });
    }
}
