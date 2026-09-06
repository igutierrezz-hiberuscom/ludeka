using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Features.Bgg;

public class BggImportService : IBggImportService
{
    private readonly IBggClient _bggClient;
    private readonly IGameRepository _gameRepo;
    private readonly IUserCollectionRepository _collectionRepo;
    private readonly IPendingBggImportRepository _pendingRepo;
    private readonly ICurrentUserService _currentUserService;

    public BggImportService(
        IBggClient bggClient,
        IGameRepository gameRepo,
        IUserCollectionRepository collectionRepo,
        IPendingBggImportRepository pendingRepo,
        ICurrentUserService currentUserService)
    {
        _bggClient = bggClient;
        _gameRepo = gameRepo;
        _collectionRepo = collectionRepo;
        _pendingRepo = pendingRepo;
        _currentUserService = currentUserService;
    }

    public async Task<BggImportResultDto> ImportUserCollectionAsync(BggImportRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ArgumentException("El nombre de usuario de BGG no puede estar vacío.", nameof(request.Username));

        string userId = _currentUserService.UserId;
        var bggItems = await _bggClient.FetchUserCollectionAsync(request.Username.Trim(), ct);

        int importedCount = 0;
        int enqueuedCount = 0;
        var errors = new List<string>();

        foreach (var bggItem in bggItems)
        {
            CollectionStatus? targetStatus = null;
            if (bggItem.IsOwned && request.ImportOwned)
            {
                targetStatus = CollectionStatus.InCollection;
            }
            else if (bggItem.IsWishlist && request.ImportWishlist)
            {
                targetStatus = CollectionStatus.Wishlist;
            }
            else if (bggItem.IsWantToBuy && request.ImportWishlist)
            {
                targetStatus = CollectionStatus.WantToBuy;
            }

            if (targetStatus == null) continue;

            try
            {
                var localGame = await _gameRepo.GetByBggIdAsync(bggItem.BggId, ct);
                if (localGame != null)
                {
                    // El juego YA existe en el catálogo local
                    var existingItem = await _collectionRepo.GetByUserAndGameAsync(userId, localGame.Id, ct);
                    if (existingItem == null)
                    {
                        var newItem = new UserCollectionItem(userId, localGame.Id, targetStatus.Value);
                        await _collectionRepo.AddAsync(newItem, ct);
                        importedCount++;
                    }
                    else if (existingItem.Status != targetStatus.Value)
                    {
                        existingItem.ChangeStatus(targetStatus.Value);
                        await _collectionRepo.UpdateAsync(existingItem, ct);
                    }
                }
                else
                {
                    // El juego NO existe en Ludeka: se encola
                    var existingPendingItem = await _collectionRepo.GetByUserAndBggIdAsync(userId, bggItem.BggId, ct);
                    if (existingPendingItem == null)
                    {
                        var newPendingItem = new UserCollectionItem(
                            userId: userId,
                            bggId: bggItem.BggId,
                            pendingTitle: bggItem.Title,
                            status: targetStatus.Value,
                            thumbnailUrl: bggItem.ThumbnailUrl,
                            yearPublished: bggItem.YearPublished
                        );
                        await _collectionRepo.AddAsync(newPendingItem, ct);
                        enqueuedCount++;

                        // Registrar o actualizar en PendingBggImports
                        var pendingQueueEntry = await _pendingRepo.GetByBggIdAsync(bggItem.BggId, ct);
                        if (pendingQueueEntry == null)
                        {
                            var newQueueEntry = new PendingBggImport(
                                bggId: bggItem.BggId,
                                title: bggItem.Title,
                                yearPublished: bggItem.YearPublished,
                                thumbnailUrl: bggItem.ThumbnailUrl,
                                coverImageUrl: bggItem.CoverImageUrl
                            );
                            await _pendingRepo.AddAsync(newQueueEntry, ct);
                        }
                        else
                        {
                            pendingQueueEntry.IncrementRequestCount();
                            await _pendingRepo.UpdateAsync(pendingQueueEntry, ct);
                        }
                    }
                    else if (existingPendingItem.Status != targetStatus.Value)
                    {
                        existingPendingItem.ChangeStatus(targetStatus.Value);
                        await _collectionRepo.UpdateAsync(existingPendingItem, ct);
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error al importar {bggItem.Title} (BggId: {bggItem.BggId}): {ex.Message}");
            }
        }

        return new BggImportResultDto(
            TotalProcessed: bggItems.Count,
            ImportedToCollection: importedCount,
            EnqueuedForCataloging: enqueuedCount,
            Errors: errors
        );
    }
}
