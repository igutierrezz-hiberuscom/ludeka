using System.Threading.Tasks;
using Ludeka.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteSchemaMigratorTests
{
    [Fact]
    public async Task EnsureSchemaUpToDateAsync_ConTablaGamesAntigua_DebeAgregarColumnasYTablasFaltantes()
    {
        // Arrange: Crear una base de datos SQLite en memoria con el esquema antiguo (sin BaseGameId ni tablas de expansiones)
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        // Crear tabla Games antigua
        using (var createCmd = connection.CreateCommand())
        {
            createCmd.CommandText = """
                CREATE TABLE "Games" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_Games" PRIMARY KEY,
                    "BggId" INTEGER NOT NULL,
                    "Slug" TEXT NOT NULL,
                    "OriginalTitle" TEXT NOT NULL,
                    "SpanishTitle" TEXT NOT NULL,
                    "Designer" TEXT NOT NULL,
                    "Publisher" TEXT NOT NULL,
                    "YearPublished" INTEGER NOT NULL,
                    "CoverImageUrl" TEXT NULL,
                    "ThumbnailUrl" TEXT NULL,
                    "Description" TEXT NULL,
                    "BggRating" REAL NOT NULL,
                    "BggRank" INTEGER NULL,
                    "LudistRating" REAL NOT NULL,
                    "Confrontation" INTEGER NOT NULL,
                    "Style" INTEGER NOT NULL,
                    "IsOfficialSolo" INTEGER NOT NULL,
                    "Language" INTEGER NOT NULL,
                    "Footprint" INTEGER NOT NULL,
                    "Scalability" TEXT NOT NULL,
                    "Sleeves" TEXT NOT NULL,
                    "Age_BoxAge" INTEGER NOT NULL,
                    "Age_CommunityAge" INTEGER NOT NULL,
                    "Duration_EstimatedPerPlayerMinutes" INTEGER NOT NULL,
                    "Duration_MaxMinutes" INTEGER NOT NULL,
                    "Duration_MinMinutes" INTEGER NOT NULL
                );
                """;
            await createCmd.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(connection)
            .Options;

        using var db = new LudekaDbContext(options);

        // Act: Ejecutar la reconciliación defensiva de esquema
        await SqliteSchemaMigrator.EnsureSchemaUpToDateAsync(db);

        // Assert: Ahora consultar db.Games no debe arrojar 'no such column: g.BaseGameId'
        var games = await db.Games.ToListAsync();
        Assert.Empty(games);

        // Comprobar que las tablas de expansiones y notificaciones ahora existen
        var synergies = await db.ExpansionSynergies.ToListAsync();
        Assert.Empty(synergies);

        var recipes = await db.ExpansionRecipes.ToListAsync();
        Assert.Empty(recipes);

        var notificationLogs = await db.NotificationLogs.ToListAsync();
        Assert.Empty(notificationLogs);
    }
}
