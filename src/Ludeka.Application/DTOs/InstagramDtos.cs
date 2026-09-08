using System;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

public record InstagramPostDraftDto(
    Guid Id,
    InstagramPostSourceType SourceType,
    string SourceId,
    string Title,
    string Caption,
    string? ImageUrl,
    string? SvgContent,
    string Theme,
    InstagramPostDraftStatus Status,
    string? InstagramMediaId,
    string? InstagramPermalink,
    string? ErrorMessage,
    string CreatedByUserId,
    string CreatedByUserName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt,
    DateTimeOffset? UpdatedAt)
{
    public static InstagramPostDraftDto FromEntity(InstagramPostDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        return new InstagramPostDraftDto(
            draft.Id,
            draft.SourceType,
            draft.SourceId,
            draft.Title,
            draft.Caption,
            draft.ImageUrl,
            draft.SvgContent,
            draft.Theme,
            draft.Status,
            draft.InstagramMediaId,
            draft.InstagramPermalink,
            draft.ErrorMessage,
            draft.CreatedByUserId,
            draft.CreatedByUserName,
            draft.CreatedAt,
            draft.PublishedAt,
            draft.UpdatedAt);
    }
}

public record CreateInstagramDraftCommand(
    InstagramPostSourceType SourceType,
    string SourceId,
    string Title,
    string? CustomCaption = null,
    string Theme = "Dark");

public record UpdateInstagramDraftCommand(
    string Caption,
    string Theme = "Dark",
    string? ImageUrl = null,
    string? SvgContent = null);

public record InstagramPublishResultDto(
    bool Success,
    string? MediaId,
    string? Permalink,
    string? ErrorMessage);
