using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

/// <summary>
/// Proporciona reconciliación y actualización defensiva del esquema SQLite para bases de datos existentes.
/// Garantiza compatibilidad retroactiva cuando se introducen nuevas columnas o tablas sin requerir migraciones manuales.
/// </summary>
public static class SqliteSchemaMigrator
{
    public static async Task EnsureSchemaUpToDateAsync(LudekaDbContext db, CancellationToken ct = default)
    {
        var connection = db.Database.GetDbConnection();
        bool shouldClose = false;

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(ct);
            shouldClose = true;
        }

        try
        {
            // 1. Obtener tablas existentes en SQLite
            var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var tablesCmd = connection.CreateCommand())
            {
                tablesCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
                using var reader = await tablesCmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    existingTables.Add(reader.GetString(0));
                }
            }

            // Si la base de datos no tiene la tabla Games, EnsureCreated se encargará de crearla completa
            if (!existingTables.Contains("Games"))
            {
                return;
            }

            // 2. Reconciliar columnas de la tabla Games (Incremento 8: Expansiones y Ecosistema)
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var columnsCmd = connection.CreateCommand())
            {
                columnsCmd.CommandText = "PRAGMA table_info('Games');";
                using var reader = await columnsCmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    existingColumns.Add(reader.GetString(1)); // nombre de la columna
                }
            }

            var columnsToAdd = new (string Name, string TypeSql)[]
            {
                ("BaseGameId", "TEXT NULL"),
                ("Type", "INTEGER NOT NULL DEFAULT 0"),
                ("ExpansionNecessity", "INTEGER NULL"),
                ("ImpactTags", "TEXT NOT NULL DEFAULT '[]'"),
                ("WhatItBringsSummary", "TEXT NULL"),
                ("ExtraPlayerCount", "INTEGER NULL"),
                ("ExtraDurationMinutes", "INTEGER NULL")
            };

            foreach (var (colName, colDef) in columnsToAdd)
            {
                if (!existingColumns.Contains(colName))
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = $"ALTER TABLE \"Games\" ADD COLUMN \"{colName}\" {colDef};";
                    await alterCmd.ExecuteNonQueryAsync(ct);
                }
            }

            // Asegurar que las filas existentes no tengan valores NULL en colecciones JSON o enumerados no nulos
            using (var fixCmd = connection.CreateCommand())
            {
                fixCmd.CommandText = """
                    UPDATE "Games" SET "ImpactTags" = '[]' WHERE "ImpactTags" IS NULL;
                    UPDATE "Games" SET "Type" = 0 WHERE "Type" IS NULL;
                    """;
                await fixCmd.ExecuteNonQueryAsync(ct);
            }

            // Crear índices en Games si no existen
            using (var idxCmd = connection.CreateCommand())
            {
                idxCmd.CommandText = """
                    CREATE INDEX IF NOT EXISTS "IX_Games_BaseGameId" ON "Games" ("BaseGameId");
                    CREATE INDEX IF NOT EXISTS "IX_Games_Type" ON "Games" ("Type");
                    """;
                await idxCmd.ExecuteNonQueryAsync(ct);
            }

            // 3. Crear tabla ExpansionSynergies si no existe (Incremento 8)
            if (!existingTables.Contains("ExpansionSynergies"))
            {
                using var createCmd = connection.CreateCommand();
                createCmd.CommandText = """
                    CREATE TABLE IF NOT EXISTS "ExpansionSynergies" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_ExpansionSynergies" PRIMARY KEY,
                        "BaseGameId" TEXT NOT NULL,
                        "ExpansionAId" TEXT NOT NULL,
                        "ExpansionBId" TEXT NOT NULL,
                        "Level" INTEGER NOT NULL,
                        "Reason" TEXT NOT NULL,
                        CONSTRAINT "FK_ExpansionSynergies_Games_BaseGameId" FOREIGN KEY ("BaseGameId") REFERENCES "Games" ("Id") ON DELETE CASCADE,
                        CONSTRAINT "FK_ExpansionSynergies_Games_ExpansionAId" FOREIGN KEY ("ExpansionAId") REFERENCES "Games" ("Id") ON DELETE CASCADE,
                        CONSTRAINT "FK_ExpansionSynergies_Games_ExpansionBId" FOREIGN KEY ("ExpansionBId") REFERENCES "Games" ("Id") ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS "IX_ExpansionSynergies_BaseGameId" ON "ExpansionSynergies" ("BaseGameId");
                    CREATE INDEX IF NOT EXISTS "IX_ExpansionSynergies_ExpansionAId_ExpansionBId" ON "ExpansionSynergies" ("ExpansionAId", "ExpansionBId");
                    """;
                await createCmd.ExecuteNonQueryAsync(ct);
            }

            // 4. Crear tabla ExpansionRecipes si no existe (Incremento 8)
            if (!existingTables.Contains("ExpansionRecipes"))
            {
                using var createCmd = connection.CreateCommand();
                createCmd.CommandText = """
                    CREATE TABLE IF NOT EXISTS "ExpansionRecipes" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_ExpansionRecipes" PRIMARY KEY,
                        "BaseGameId" TEXT NOT NULL,
                        "Name" TEXT NOT NULL,
                        "Description" TEXT NOT NULL,
                        "IdealFor" TEXT NOT NULL,
                        "IncludedExpansionIds" TEXT NOT NULL DEFAULT '[]',
                        CONSTRAINT "FK_ExpansionRecipes_Games_BaseGameId" FOREIGN KEY ("BaseGameId") REFERENCES "Games" ("Id") ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS "IX_ExpansionRecipes_BaseGameId" ON "ExpansionRecipes" ("BaseGameId");
                    """;
                await createCmd.ExecuteNonQueryAsync(ct);
            }

            // 5. Crear tabla NotificationLogs si no existe (Incremento 9)
            if (!existingTables.Contains("NotificationLogs"))
            {
                using var createCmd = connection.CreateCommand();
                createCmd.CommandText = """
                    CREATE TABLE IF NOT EXISTS "NotificationLogs" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_NotificationLogs" PRIMARY KEY,
                        "EventType" INTEGER NOT NULL,
                        "Channel" INTEGER NOT NULL,
                        "Title" TEXT NOT NULL,
                        "Summary" TEXT NOT NULL,
                        "TargetUrl" TEXT NULL,
                        "ImageUrl" TEXT NULL,
                        "Status" INTEGER NOT NULL,
                        "ErrorDetails" TEXT NULL,
                        "CreatedAt" TEXT NOT NULL,
                        "SentAt" TEXT NULL
                    );
                    CREATE INDEX IF NOT EXISTS "IX_NotificationLogs_Status" ON "NotificationLogs" ("Status");
                    CREATE INDEX IF NOT EXISTS "IX_NotificationLogs_Channel" ON "NotificationLogs" ("Channel");
                    CREATE INDEX IF NOT EXISTS "IX_NotificationLogs_CreatedAt" ON "NotificationLogs" ("CreatedAt");
                    """;
                await createCmd.ExecuteNonQueryAsync(ct);
            }
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
