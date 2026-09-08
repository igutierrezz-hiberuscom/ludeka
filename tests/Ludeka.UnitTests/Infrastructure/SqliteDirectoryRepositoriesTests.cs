using System;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Ludeka.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteDirectoryRepositoriesTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private LudekaDbContext _context = null!;
    private SqlitePublisherRepository _publisherRepo = null!;
    private SqliteCreatorRepository _creatorRepo = null!;
    private SqliteStoreRepository _storeRepo = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _publisherRepo = new SqlitePublisherRepository(_context);
        _creatorRepo = new SqliteCreatorRepository(_context);
        _storeRepo = new SqliteStoreRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task SqlitePublisherRepository_ShouldPersistAndQueryCorrectly()
    {
        var pub = new Publisher(
            "Devir Iberia",
            "devir-iberia",
            "España",
            "Barcelona",
            "Editorial decana",
            "/images/publishers/devir.png",
            "https://devir.es",
            new[] { new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@devirtv", "@devirtv") }
        );

        await _publisherRepo.AddAsync(pub);

        var bySlug = await _publisherRepo.GetBySlugAsync("devir-iberia");
        Assert.NotNull(bySlug);
        Assert.Equal("Devir Iberia", bySlug.Name);
        Assert.Single(bySlug.SocialLinks);
        Assert.Equal("@devirtv", bySlug.SocialLinks[0].Handle);

        var byName = await _publisherRepo.GetByNameAsync("Devir Iberia");
        Assert.NotNull(byName);

        // Update
        bySlug.UpdateDetails("Devir", "España", "Barcelona", "Nueva descripción", null, null);
        await _publisherRepo.UpdateAsync(bySlug);

        var updated = await _publisherRepo.GetByIdAsync(pub.Id);
        Assert.NotNull(updated);
        Assert.Equal("Devir", updated.Name);

        // Delete
        await _publisherRepo.DeleteAsync(pub.Id);
        Assert.Null(await _publisherRepo.GetByIdAsync(pub.Id));
    }

    [Fact]
    public async Task SqliteCreatorRepository_ShouldPersistAndQueryCorrectly()
    {
        var creator = new Creator(
            "Elizabeth Hargrave",
            "elizabeth-hargrave",
            "Estados Unidos",
            "Diseñadora de Wingspan",
            "/images/creators/elizabeth.png",
            104523,
            "https://elizabethhargrave.com",
            new[] { new SocialNetworkLink(SocialPlatform.Twitter, "https://twitter.com/elizhargrave", "@elizhargrave") }
        );

        await _creatorRepo.AddAsync(creator);

        var bySlug = await _creatorRepo.GetBySlugAsync("elizabeth-hargrave");
        Assert.NotNull(bySlug);
        Assert.Equal("Elizabeth Hargrave", bySlug.Name);
        Assert.Equal(104523, bySlug.BggPersonId);
        Assert.Single(bySlug.SocialLinks);

        // Delete
        await _creatorRepo.DeleteAsync(creator.Id);
        Assert.Null(await _creatorRepo.GetByIdAsync(creator.Id));
    }

    [Fact]
    public async Task SqliteStoreRepository_ShouldPersistAndQueryCorrectly()
    {
        var store = new Store(
            "Zacatrus!",
            "zacatrus",
            StoreType.Hybrid,
            "Madrid",
            "Calle Fernández 57",
            "Tienda física y online",
            "/images/stores/zacatrus.png",
            "https://zacatrus.es",
            "LDKZAC",
            true,
            new[] { new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@zacatrustv", "@zacatrustv") }
        );

        await _storeRepo.AddAsync(store);

        var bySlug = await _storeRepo.GetBySlugAsync("zacatrus");
        Assert.NotNull(bySlug);
        Assert.Equal("Zacatrus!", bySlug.Name);
        Assert.Equal(StoreType.Hybrid, bySlug.Type);
        Assert.True(bySlug.HasLoyaltyProgram);

        var all = await _storeRepo.GetAllAsync();
        Assert.Single(all);

        // Delete
        await _storeRepo.DeleteAsync(store.Id);
        Assert.Empty(await _storeRepo.GetAllAsync());
    }
}
