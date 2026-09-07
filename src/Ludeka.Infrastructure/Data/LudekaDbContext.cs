using Ludeka.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class LudekaDbContext : DbContext
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<UserCollectionItem> CollectionItems => Set<UserCollectionItem>();
    public DbSet<GameLoan> Loans => Set<GameLoan>();
    public DbSet<UserGameReview> Reviews => Set<UserGameReview>();
    public DbSet<FoundingVerdict> FoundingVerdicts => Set<FoundingVerdict>();
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<PendingBggImport> PendingBggImports => Set<PendingBggImport>();
    public DbSet<Giveaway> Giveaways => Set<Giveaway>();
    public DbSet<WeeklyRelease> WeeklyReleases => Set<WeeklyRelease>();
    public DbSet<RuleQuestion> RuleQuestions => Set<RuleQuestion>();
    public DbSet<RuleAnswer> RuleAnswers => Set<RuleAnswer>();
    public DbSet<RuleVote> RuleVotes => Set<RuleVote>();
    public DbSet<ExpansionSynergy> ExpansionSynergies => Set<ExpansionSynergy>();
    public DbSet<ExpansionRecipe> ExpansionRecipes => Set<ExpansionRecipe>();

    public LudekaDbContext(DbContextOptions<LudekaDbContext> options) : base(options)
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
        game.HasIndex(g => g.BaseGameId);
        game.HasIndex(g => g.Type);

        game.ComplexProperty(g => g.Age);
        game.ComplexProperty(g => g.Duration);

        // Mapeo JSON nativo en EF Core 10 para colecciones de Value Objects y primitivas
        game.OwnsMany(g => g.Scalability, b => b.ToJson());
        game.OwnsMany(g => g.Sleeves, b => b.ToJson());
        game.PrimitiveCollection(g => g.ImpactTags);

        // Relación reflexiva para juego base y expansiones
        game.HasOne(g => g.BaseGame)
            .WithMany(g => g.Expansions)
            .HasForeignKey(g => g.BaseGameId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Configuración de UserCollectionItem ---
        var collection = modelBuilder.Entity<UserCollectionItem>();
        collection.ToTable("UserCollectionItems");
        collection.HasKey(c => c.Id);

        collection.HasIndex(c => new { c.UserId, c.GameId });
        collection.HasIndex(c => new { c.UserId, c.BggId });
        collection.HasIndex(c => new { c.UserId, c.Status });

        collection.HasOne(c => c.Game)
            .WithMany()
            .HasForeignKey(c => c.GameId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Configuración de PendingBggImport ---
        var pending = modelBuilder.Entity<PendingBggImport>();
        pending.ToTable("PendingBggImports");
        pending.HasKey(p => p.Id);

        pending.HasIndex(p => p.BggId).IsUnique();
        pending.HasIndex(p => new { p.Status, p.RequestedCount });
        pending.HasIndex(p => p.CreatedAt);

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

        // --- Configuración de MediaItem ---
        var media = modelBuilder.Entity<MediaItem>();
        media.ToTable("MediaItems");
        media.HasKey(m => m.Id);

        media.HasIndex(m => m.GameId);
        media.HasIndex(m => m.Status);
        media.HasIndex(m => m.Type);
        media.HasIndex(m => m.Platform);
        media.HasIndex(m => new { m.GameId, m.Status });

        media.HasOne(m => m.Game)
            .WithMany()
            .HasForeignKey(m => m.GameId)
            .OnDelete(DeleteBehavior.SetNull);

        // --- Configuración de Giveaway ---
        var giveaway = modelBuilder.Entity<Giveaway>();
        giveaway.ToTable("Giveaways");
        giveaway.HasKey(g => g.Id);

        giveaway.HasIndex(g => g.DeadlineAt);
        giveaway.HasIndex(g => g.Platform);
        giveaway.HasIndex(g => g.IsCommunityExclusive);

        giveaway.HasOne(g => g.Game)
            .WithMany()
            .HasForeignKey(g => g.GameId)
            .OnDelete(DeleteBehavior.SetNull);

        // --- Configuración de WeeklyRelease ---
        var release = modelBuilder.Entity<WeeklyRelease>();
        release.ToTable("WeeklyReleases");
        release.HasKey(r => r.Id);

        release.HasIndex(r => r.ReleaseDate);

        release.HasOne(r => r.Game)
            .WithMany()
            .HasForeignKey(r => r.GameId)
            .OnDelete(DeleteBehavior.SetNull);

        // --- Configuración de RuleQuestion ---
        var question = modelBuilder.Entity<RuleQuestion>();
        question.ToTable("RuleQuestions");
        question.HasKey(q => q.Id);

        question.HasIndex(q => q.GameId);
        question.HasIndex(q => q.CreatedAt);

        question.HasOne(q => q.Game)
            .WithMany()
            .HasForeignKey(q => q.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        question.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Configuración de RuleAnswer ---
        var answer = modelBuilder.Entity<RuleAnswer>();
        answer.ToTable("RuleAnswers");
        answer.HasKey(a => a.Id);

        answer.HasIndex(a => a.QuestionId);
        answer.HasIndex(a => a.IsAccepted);

        // --- Configuración de RuleVote ---
        var vote = modelBuilder.Entity<RuleVote>();
        vote.ToTable("RuleVotes");
        vote.HasKey(v => v.Id);

        vote.HasIndex(v => new { v.UserId, v.QuestionId });
        vote.HasIndex(v => new { v.UserId, v.AnswerId });

        // --- Configuración de ExpansionSynergy ---
        var synergy = modelBuilder.Entity<ExpansionSynergy>();
        synergy.ToTable("ExpansionSynergies");
        synergy.HasKey(s => s.Id);

        synergy.HasIndex(s => s.BaseGameId);
        synergy.HasIndex(s => new { s.ExpansionAId, s.ExpansionBId });

        synergy.HasOne(s => s.BaseGame)
            .WithMany()
            .HasForeignKey(s => s.BaseGameId)
            .OnDelete(DeleteBehavior.Cascade);

        synergy.HasOne(s => s.ExpansionA)
            .WithMany()
            .HasForeignKey(s => s.ExpansionAId)
            .OnDelete(DeleteBehavior.Cascade);

        synergy.HasOne(s => s.ExpansionB)
            .WithMany()
            .HasForeignKey(s => s.ExpansionBId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Configuración de ExpansionRecipe ---
        var recipe = modelBuilder.Entity<ExpansionRecipe>();
        recipe.ToTable("ExpansionRecipes");
        recipe.HasKey(r => r.Id);

        recipe.HasIndex(r => r.BaseGameId);
        recipe.PrimitiveCollection(r => r.IncludedExpansionIds);

        recipe.HasOne(r => r.BaseGame)
            .WithMany()
            .HasForeignKey(r => r.BaseGameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
