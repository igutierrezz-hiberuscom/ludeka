using System;
using System.Collections.Generic;
using System.Linq;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Core.Entities;

/// <summary>
/// Creador (autor, diseñador, ilustrador o divulgador de referencia) en Ludeka.
/// </summary>
public class Creator
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Nationality { get; private set; }
    public string? Bio { get; private set; }
    public string? AvatarUrl { get; private set; }
    public int? BggPersonId { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public List<SocialNetworkLink> SocialLinks { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Constructor privado para EF Core
    private Creator() { }

    public Creator(
        string name,
        string slug,
        string? nationality = null,
        string? bio = null,
        string? avatarUrl = null,
        int? bggPersonId = null,
        string? websiteUrl = null,
        IEnumerable<SocialNetworkLink>? socialLinks = null,
        Guid? id = null,
        DateTimeOffset? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del creador no puede estar vacío.", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("El slug del creador no puede estar vacío.", nameof(slug));

        Id = id ?? Guid.NewGuid();
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Nationality = string.IsNullOrWhiteSpace(nationality) ? null : nationality.Trim();
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        BggPersonId = bggPersonId;
        WebsiteUrl = string.IsNullOrWhiteSpace(websiteUrl) ? null : websiteUrl.Trim();
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;

        if (socialLinks != null)
        {
            SocialLinks.AddRange(socialLinks);
        }
    }

    public void UpdateDetails(
        string name,
        string? nationality,
        string? bio,
        string? avatarUrl,
        int? bggPersonId,
        string? websiteUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del creador no puede estar vacío.", nameof(name));

        Name = name.Trim();
        Nationality = string.IsNullOrWhiteSpace(nationality) ? null : nationality.Trim();
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        BggPersonId = bggPersonId;
        WebsiteUrl = string.IsNullOrWhiteSpace(websiteUrl) ? null : websiteUrl.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetSocialLinks(IEnumerable<SocialNetworkLink> links)
    {
        SocialLinks.Clear();
        if (links != null)
        {
            SocialLinks.AddRange(links);
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddOrUpdateSocialLink(SocialNetworkLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        SocialLinks.RemoveAll(l => l.Platform == link.Platform && l.Url.Equals(link.Url, StringComparison.OrdinalIgnoreCase));
        SocialLinks.Add(link);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public SocialNetworkLink? GetYouTubeLink() =>
        SocialLinks.FirstOrDefault(l => l.Platform == SocialPlatform.YouTube);
}
