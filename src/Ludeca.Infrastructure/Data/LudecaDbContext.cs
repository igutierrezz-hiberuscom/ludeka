using Ludeca.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeca.Infrastructure.Data;

public class LudecaDbContext : DbContext
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<UserCollectionItem> CollectionItems => Set<UserCollectionItem>();
    public DbSet<GameLoan> Loans => Set<GameLoan>();
    public DbSet<UserGameReview> Reviews => Set<UserGameReview>();
    public DbSet<FoundingVerdict> FoundingVerdicts => Set<FoundingVerdict>();

    public LudecaDbContext(DbContextOptions<LudecaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Configuración de Game ---
        var game = modelBuilder.Entity<Game>();
        game.ToTable("Games");

        game.HasKey(g => g.Id);

        game.HasIndex(g => g.Slug).IsUnique();
        game.HasIndex(g => g.BggId).IsUnique();
        game.HasIndex(g => g.SpanishTitle);
        game.HasIndex(g => g.OriginalTitle);

        game.ComplexProperty(g => g.Age);
        game.ComplexProperty(g => g.Duration);

        // Mapeo JSON nativo en EF Core 10 para colecciones de Value Objects
        game.OwnsMany(g => g.Scalability, b => b.ToJson());
        game.OwnsMany(g => g.Sleeves, b => b.ToJson());

        // --- Configuración de UserCollectionItem ---
        var collection = modelBuilder.Entity<UserCollectionItem>();
        collection.ToTable("UserCollectionItems");
        collection.HasKey(c => c.Id);

        collection.HasIndex(c => new { c.UserId, c.GameId }).IsUnique();
        collection.HasIndex(c => new { c.UserId, c.Status });

        collection.HasOne(c => c.Game)
            .WithMany()
            .HasForeignKey(c => c.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Configuración de GameLoan ---
        var loan = modelBuilder.Entity<GameLoan>();
        loan.ToTable("GameLoans");
        loan.HasKey(l => l.Id);

        loan.HasIndex(l => new { l.UserId, l.IsReturned });
        loan.HasIndex(l => new { l.UserId, l.GameId });

        loan.HasOne(l => l.Game)
            .WithMany()
            .HasForeignKey(l => l.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Configuración de UserGameReview ---
        var review = modelBuilder.Entity<UserGameReview>();
        review.ToTable("UserGameReviews");
        review.HasKey(r => r.Id);

        review.HasIndex(r => new { r.UserId, r.GameId }).IsUnique();
        review.HasIndex(r => r.GameId);

        review.HasOne(r => r.Game)
            .WithMany()
            .HasForeignKey(r => r.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // Mapeo JSON para votos de comensales y experiencia familiar
        review.OwnsMany(r => r.PlayerCountRatings, b => b.ToJson());
        review.OwnsOne(r => r.FamilyExperience, b => b.ToJson());

        // --- Configuración de FoundingVerdict ---
        var verdict = modelBuilder.Entity<FoundingVerdict>();
        verdict.ToTable("FoundingVerdicts");
        verdict.HasKey(v => v.Id);

        verdict.HasIndex(v => v.GameId).IsUnique();

        verdict.OwnsMany(v => v.Photos, b => b.ToJson());
    }
}
