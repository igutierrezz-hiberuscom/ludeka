using System;
using System.Collections.Generic;
using System.Linq;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Core.Entities;

/// <summary>
/// Editorial de juegos de mesa en Ludeka.
/// </summary>
public class Publisher
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string? City { get; private set; }
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public List<SocialNetworkLink> SocialLinks { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Constructor privado para EF Core
    private Publisher() { }

    public Publisher(
        string name,
        string slug,
        string country,
        string? city = null,
        string? description = null,
        string? logoUrl = null,
        string? websiteUrl = null,
        IEnumerable<SocialNetworkLink>? socialLinks = null,
        Guid? id = null,
        DateTimeOffset? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la editorial no puede estar vacío.", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("El slug de la editorial no puede estar vacío.", nameof(slug));

        Id = id ?? Guid.NewGuid();
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Country = string.IsNullOrWhiteSpace(country) ? "España" : country.Trim();
        City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        WebsiteUrl = string.IsNullOrWhiteSpace(websiteUrl) ? null : websiteUrl.Trim();
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;

        if (socialLinks != null)
        {
            SocialLinks.AddRange(socialLinks);
        }
    }

    public void UpdateDetails(
        string name,
        string country,
        string? city,
        string? description,
        string? logoUrl,
        string? websiteUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la editorial no puede estar vacío.", nameof(name));

        Name = name.Trim();
        Country = string.IsNullOrWhiteSpace(country) ? "España" : country.Trim();
        City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
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
