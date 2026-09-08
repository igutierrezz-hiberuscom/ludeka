using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Contracts;

public interface IInstagramPostDraftRepository
{
    Task<IReadOnlyList<InstagramPostDraft>> GetDraftsAsync(InstagramPostDraftStatus? status = null, CancellationToken ct = default);
    Task<InstagramPostDraft?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<InstagramPostDraft?> GetBySourceAsync(InstagramPostSourceType sourceType, string sourceId, CancellationToken ct = default);
    Task AddAsync(InstagramPostDraft draft, CancellationToken ct = default);
    Task UpdateAsync(InstagramPostDraft draft, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
