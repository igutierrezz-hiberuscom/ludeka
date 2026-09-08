using System;
using System.Collections.Generic;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class DirectoryDomainTests
{
    [Fact]
    public void SocialNetworkLink_ShouldInstantiateCorrectly_WhenValid()
    {
        var link = new SocialNetworkLink(
            SocialPlatform.YouTube,
            "https://youtube.com/@devirtv",
            "@devirtv",
            "Canal Oficial de Devir"
        );

        Assert.Equal(SocialPlatform.YouTube, link.Platform);
        Assert.Equal("https://youtube.com/@devirtv", link.Url);
        Assert.Equal("@devirtv", link.Handle);
        Assert.Equal("Canal Oficial de Devir", link.Title);
        Assert.Equal("▶️", link.PlatformIcon);
        Assert.Equal("YouTube", link.PlatformName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SocialNetworkLink_ShouldThrow_WhenUrlIsEmpty(string invalidUrl)
    {
        Assert.Throws<ArgumentException>(() => new SocialNetworkLink(SocialPlatform.Website, invalidUrl));
    }

    [Fact]
    public void Publisher_ShouldCreateAndFormat_Correctly()
    {
        var ytLink = new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@devirtv", "@devirtv");
        var pub = new Publisher(
            "Devir Iberia",
            "devir-iberia",
            "España",
            "Barcelona",
            "Editorial pionera de juegos de mesa",
            "https://example.com/devir.png",
            "https://devir.es",
            new[] { ytLink }
        );

        Assert.Equal("Devir Iberia", pub.Name);
        Assert.Equal("devir-iberia", pub.Slug);
        Assert.Equal("España", pub.Country);
        Assert.Equal("Barcelona", pub.City);
        Assert.Single(pub.SocialLinks);
        Assert.NotNull(pub.GetYouTubeLink());
        Assert.Equal("@devirtv", pub.GetYouTubeLink()!.Handle);

        // Update details
        pub.UpdateDetails("Devir", "España", "Barcelona", "Nueva descripción", "logo2.png", "https://devir.es/nuevo");
        Assert.Equal("Devir", pub.Name);
        Assert.NotNull(pub.UpdatedAt);

        // Add or replace social link
        var instaLink = new SocialNetworkLink(SocialPlatform.Instagram, "https://instagram.com/deviriberia");
        pub.AddOrUpdateSocialLink(instaLink);
        Assert.Equal(2, pub.SocialLinks.Count);
    }

    [Fact]
    public void Creator_ShouldCreateAndFormat_Correctly()
    {
        var ytLink = new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@AnalisisParalisis", "@AnalisisParalisis");
        var creator = new Creator(
            "Elizabeth Hargrave",
            "elizabeth-hargrave",
            "EEUU",
            "Diseñadora galardonada de Wingspan",
            "https://example.com/elizabeth.png",
            104523,
            "https://elizabethhargrave.com",
            new[] { ytLink }
        );

        Assert.Equal("Elizabeth Hargrave", creator.Name);
        Assert.Equal("elizabeth-hargrave", creator.Slug);
        Assert.Equal("EEUU", creator.Nationality);
        Assert.Equal(104523, creator.BggPersonId);
        Assert.Single(creator.SocialLinks);
        Assert.NotNull(creator.GetYouTubeLink());

        // Update details
        creator.UpdateDetails("E. Hargrave", "Estados Unidos", "Bio ampliada", "avatar.jpg", 104523, "https://web.org");
        Assert.Equal("E. Hargrave", creator.Name);
        Assert.Equal("Estados Unidos", creator.Nationality);
        Assert.NotNull(creator.UpdatedAt);
    }

    [Fact]
    public void Store_ShouldCreateAndManage_Correctly()
    {
        var ytLink = new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@zacatrustv", "@zacatrustv");
        var store = new Store(
            "Zacatrus!",
            "zacatrus",
            StoreType.Hybrid,
            "Madrid",
            "Calle Fernández de los Ríos 57",
            "Tienda física y online con ludoteca",
            "https://example.com/zacatrus.png",
            "https://zacatrus.es",
            "LDKZAC",
            true,
            new[] { ytLink }
        );

        Assert.Equal("Zacatrus!", store.Name);
        Assert.Equal("zacatrus", store.Slug);
        Assert.Equal(StoreType.Hybrid, store.Type);
        Assert.Equal("Madrid", store.City);
        Assert.Equal("LDKZAC", store.AffiliateCode);
        Assert.True(store.HasLoyaltyProgram);
        Assert.Single(store.SocialLinks);
        Assert.NotNull(store.GetYouTubeLink());

        store.UpdateDetails("Zacatrus Central", StoreType.Hybrid, "Madrid", "Calle Mayor 1", "Actualizada", null, "https://zacatrus.es", "LDKZAC2", false);
        Assert.Equal("Zacatrus Central", store.Name);
        Assert.False(store.HasLoyaltyProgram);
        Assert.Equal("LDKZAC2", store.AffiliateCode);
    }

    [Theory]
    [InlineData("", "slug")]
    [InlineData("   ", "slug")]
    [InlineData("Name", "")]
    [InlineData("Name", "   ")]
    public void Entities_ShouldThrow_WhenNameOrSlugEmpty(string name, string slug)
    {
        Assert.Throws<ArgumentException>(() => new Publisher(name, slug, "España"));
        Assert.Throws<ArgumentException>(() => new Creator(name, slug));
        Assert.Throws<ArgumentException>(() => new Store(name, slug));
    }
}
