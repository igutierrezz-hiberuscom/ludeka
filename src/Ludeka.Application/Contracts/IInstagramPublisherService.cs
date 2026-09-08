using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Contracts;

public interface IInstagramPublisherService
{
    Task<InstagramPostDraftDto> CreateDraftFromGiveawayAsync(Guid giveawayId, string userId, string userName, string theme = "Dark", CancellationToken ct = default);
    Task<InstagramPostDraftDto> CreateDraftFromWeeklyReleaseAsync(Guid releaseId, string userId, string userName, string theme = "Dark", CancellationToken ct = default);
    Task<InstagramPostDraftDto> CreateDraftFromGameAsync(Guid gameId, string userId, string userName, string theme = "Dark", CancellationToken ct = default);
    Task<InstagramPostDraftDto?> GetDraftByIdAsync(Guid draftId, CancellationToken ct = default);
    Task<IReadOnlyList<InstagramPostDraftDto>> GetDraftsAsync(InstagramPostDraftStatus? status = null, CancellationToken ct = default);
    Task<InstagramPostDraftDto> UpdateDraftAsync(Guid draftId, UpdateInstagramDraftCommand command, CancellationToken ct = default);
    Task<InstagramPublishResultDto> PublishDraftAsync(Guid draftId, string userId, string userName, CancellationToken ct = default);
    Task<bool> DeleteDraftAsync(Guid draftId, CancellationToken ct = default);
}
