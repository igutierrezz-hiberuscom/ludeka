using System;
using Ludeka.Core.Enums;

namespace Ludeka.Core.Entities;

/// <summary>
/// Representa un borrador editorial de publicación para la cuenta oficial de Instagram de Ludeka.
/// Gestiona la composición de la imagen (1:1), el texto/hashtags enriquecidos y el estado
/// de publicación a través de la Meta Graph API.
/// </summary>
public class InstagramPostDraft
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public InstagramPostSourceType SourceType { get; private set; } = InstagramPostSourceType.Manual;
    public string SourceId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Caption { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public string? SvgContent { get; private set; }
    public string Theme { get; private set; } = "Dark";
    public InstagramPostDraftStatus Status { get; private set; } = InstagramPostDraftStatus.Draft;
    public string? InstagramMediaId { get; private set; }
    public string? InstagramPermalink { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string CreatedByUserId { get; private set; } = string.Empty;
    public string CreatedByUserName { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PublishedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Constructor privado para EF Core
    private InstagramPostDraft() { }

    public InstagramPostDraft(
        string title,
        string caption,
        InstagramPostSourceType sourceType,
        string sourceId,
        string createdByUserId,
        string createdByUserName,
        string? imageUrl = null,
        string? svgContent = null,
        string theme = "Dark")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título de la publicación no puede estar vacío.", nameof(title));

        if (string.IsNullOrWhiteSpace(caption))
            throw new ArgumentException("El texto (copy) de la publicación no puede estar vacío.", nameof(caption));

        if (string.IsNullOrWhiteSpace(createdByUserId))
            throw new ArgumentException("El identificador del moderador creador no puede estar vacío.", nameof(createdByUserId));

        Title = title.Trim();
        Caption = caption.Trim();
        SourceType = sourceType;
        SourceId = sourceId?.Trim() ?? string.Empty;
        CreatedByUserId = createdByUserId.Trim();
        CreatedByUserName = string.IsNullOrWhiteSpace(createdByUserName) ? createdByUserId.Trim() : createdByUserName.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        SvgContent = string.IsNullOrWhiteSpace(svgContent) ? null : svgContent.Trim();
        Theme = NormalizeTheme(theme);
        Status = InstagramPostDraftStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDraft(string caption, string theme, string? imageUrl = null, string? svgContent = null)
    {
        if (Status == InstagramPostDraftStatus.Published)
            throw new InvalidOperationException("No se puede modificar un borrador que ya ha sido publicado en Instagram.");

        if (string.IsNullOrWhiteSpace(caption))
            throw new ArgumentException("El texto (copy) de la publicación no puede estar vacío.", nameof(caption));

        Caption = caption.Trim();
        Theme = NormalizeTheme(theme);

        if (!string.IsNullOrWhiteSpace(imageUrl))
            ImageUrl = imageUrl.Trim();

        if (!string.IsNullOrWhiteSpace(svgContent))
            SvgContent = svgContent.Trim();

        ErrorMessage = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkPublishing()
    {
        if (Status == InstagramPostDraftStatus.Published)
            throw new InvalidOperationException("El post ya está publicado.");

        Status = InstagramPostDraftStatus.Publishing;
        ErrorMessage = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkPublished(string mediaId, string permalink)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
            throw new ArgumentException("El Media ID de Instagram no puede estar vacío.", nameof(mediaId));

        if (string.IsNullOrWhiteSpace(permalink))
            throw new ArgumentException("El enlace permanente (permalink) no puede estar vacío.", nameof(permalink));

        InstagramMediaId = mediaId.Trim();
        InstagramPermalink = permalink.Trim();
        Status = InstagramPostDraftStatus.Published;
        ErrorMessage = null;
        PublishedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = InstagramPostDraftStatus.Failed;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Error desconocido durante la publicación en Instagram." : errorMessage.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeTheme(string? theme)
    {
        return string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";
    }
}
