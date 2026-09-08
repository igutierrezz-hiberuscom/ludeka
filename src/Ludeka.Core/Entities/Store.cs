using System;
using System.Collections.Generic;
using System.Linq;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Core.Entities;

/// <summary>
/// Tienda física, online o híbrida especializada en juegos de mesa.
/// </summary>
public class Store
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public StoreType Type { get; private set; }
    public string Country { get; private set; } = "España";
    public string? City { get; private set; }
    public string? Address { get; private set; }
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? AffiliateCode { get; private set; }
    public bool HasLoyaltyProgram { get; private set; }
    public List<SocialNetworkLink> SocialLinks { get; private set; } = [];
    public List<string> ShippingCountries { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Constructor privado para EF Core
    private Store() { }

    public Store(
        string name,
        string slug,
        StoreType type = StoreType.Hybrid,
        string? city = null,
        string? address = null,
        string? description = null,
        string? logoUrl = null,
        string? websiteUrl = null,
        string? affiliateCode = null,
        bool hasLoyaltyProgram = false,
        IEnumerable<SocialNetworkLink>? socialLinks = null,
        Guid? id = null,
        DateTimeOffset? createdAt = null,
        string country = "España",
        IEnumerable<string>? shippingCountries = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la tienda no puede estar vacío.", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("El slug de la tienda no puede estar vacío.", nameof(slug));

        Id = id ?? Guid.NewGuid();
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Type = type;
        City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        WebsiteUrl = string.IsNullOrWhiteSpace(websiteUrl) ? null : websiteUrl.Trim();
        AffiliateCode = string.IsNullOrWhiteSpace(affiliateCode) ? null : affiliateCode.Trim();
        HasLoyaltyProgram = hasLoyaltyProgram;
        Country = string.IsNullOrWhiteSpace(country) ? "España" : CountryCatalog.Normalize(country);
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;

        if (socialLinks != null)
        {
            SocialLinks.AddRange(socialLinks);
        }

        if (shippingCountries != null)
        {
            ShippingCountries.AddRange(shippingCountries.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => CountryCatalog.Normalize(c)));
        }
        else
        {
            // Por defecto, realiza envíos al menos a su país sede
            ShippingCountries.Add(Country);
        }
    }

    public void UpdateDetails(
        string name,
        StoreType type,
        string? city,
        string? address,
        string? description,
        string? logoUrl,
        string? websiteUrl,
        string? affiliateCode,
        bool hasLoyaltyProgram,
        string country = "España",
        IEnumerable<string>? shippingCountries = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la tienda no puede estar vacío.", nameof(name));

        Name = name.Trim();
        Type = type;
        City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        WebsiteUrl = string.IsNullOrWhiteSpace(websiteUrl) ? null : websiteUrl.Trim();
        AffiliateCode = string.IsNullOrWhiteSpace(affiliateCode) ? null : affiliateCode.Trim();
        HasLoyaltyProgram = hasLoyaltyProgram;
        Country = string.IsNullOrWhiteSpace(country) ? "España" : CountryCatalog.Normalize(country);

        if (shippingCountries != null)
        {
            ShippingCountries.Clear();
            ShippingCountries.AddRange(shippingCountries.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => CountryCatalog.Normalize(c)));
            if (!ShippingCountries.Contains(Country))
            {
                ShippingCountries.Add(Country);
            }
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool ShipsTo(string? targetCountry)
    {
        if (string.IsNullOrWhiteSpace(targetCountry))
            return true;

        var normalizedTarget = CountryCatalog.Normalize(targetCountry);

        if (string.Equals(Country, normalizedTarget, StringComparison.OrdinalIgnoreCase))
            return true;

        if (ShippingCountries.Any(c => CountryCatalog.IsInternational(c) ||
                                       string.Equals(CountryCatalog.Normalize(c), normalizedTarget, StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
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
