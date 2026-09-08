using System;
using System.Collections.Generic;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class CountryFilteringEntityTests
{
    [Fact]
    public void Store_ShipsTo_ConsidersDomesticAndInternationalDestinations()
    {
        // Tienda española con envíos a España, Portugal y Francia
        var store = new Store(
            name: "Tienda Peninsular",
            slug: "tienda-peninsular",
            type: StoreType.Hybrid,
            country: "España",
            shippingCountries: new[] { "España", "Portugal", "Francia" }
        );

        Assert.Equal("España", store.Country);
        Assert.True(store.ShipsTo("España"));
        Assert.True(store.ShipsTo("Portugal"));
        Assert.True(store.ShipsTo("Francia"));
        Assert.False(store.ShipsTo("México"));
        Assert.False(store.ShipsTo("Argentina"));
    }

    [Fact]
    public void Store_ShipsTo_DefaultsToHostCountryWhenNoShippingCountriesProvided()
    {
        var store = new Store(
            name: "Tienda Local",
            slug: "tienda-local",
            country: "México"
        );

        Assert.Equal("México", store.Country);
        Assert.True(store.ShipsTo("México"));
        Assert.False(store.ShipsTo("España"));
    }

    [Fact]
    public void Giveaway_IsAvailableInCountry_InternationalAlwaysVisibleLocalOnlyInSpecifiedCountry()
    {
        var localGiveaway = new Giveaway(
            title: "Sorteo Nacional España",
            organizer: "Club Lúdico",
            collaborator: null,
            url: "https://instagram.com/p/local",
            platform: GiveawayPlatform.Instagram,
            deadlineAt: DateTimeOffset.UtcNow.AddDays(5),
            country: "España"
        );

        var intlGiveaway = new Giveaway(
            title: "Sorteo Mundial",
            organizer: "Publisher Global",
            collaborator: null,
            url: "https://x.com/giveaway/intl",
            platform: GiveawayPlatform.TwitterX,
            deadlineAt: DateTimeOffset.UtcNow.AddDays(5),
            country: "Internacional"
        );

        // Local España
        Assert.True(localGiveaway.IsAvailableInCountry("España"));
        Assert.False(localGiveaway.IsAvailableInCountry("México"));
        Assert.False(localGiveaway.IsAvailableInCountry("Argentina"));
        Assert.True(localGiveaway.IsAvailableInCountry(null)); // Sin filtro, visible

        // Internacional
        Assert.True(intlGiveaway.IsInternational);
        Assert.True(intlGiveaway.IsAvailableInCountry("España"));
        Assert.True(intlGiveaway.IsAvailableInCountry("México"));
        Assert.True(intlGiveaway.IsAvailableInCountry("Argentina"));
        Assert.True(intlGiveaway.IsAvailableInCountry(null));
    }

    [Fact]
    public void BoardGameEvent_IsCelebratedInCountry_InternationalAlwaysVisibleLocalOnlyInSpecifiedCountry()
    {
        var localEvent = new BoardGameEvent(
            title: "Jornadas Tierra de Nadie",
            description: "Jornadas de rol y juegos en Mollina",
            imageUrl: "https://tdn.es/cartel.jpg",
            startDate: new DateOnly(2026, 8, 5),
            endDate: new DateOnly(2026, 8, 9),
            location: "Mollina (Málaga)",
            websiteUrl: "https://tdn.es",
            organizer: "TDN",
            isOfficial: true,
            country: "España"
        );

        var intlEvent = new BoardGameEvent(
            title: "Global Board Game Convention Online",
            description: "Convención online abierta al mundo",
            imageUrl: "https://globalbg.org/banner.jpg",
            startDate: new DateOnly(2026, 11, 1),
            endDate: new DateOnly(2026, 11, 3),
            location: "Online",
            websiteUrl: "https://globalbg.org",
            organizer: "World BG",
            isOfficial: true,
            country: "Internacional"
        );

        // Local
        Assert.True(localEvent.IsCelebratedInCountry("España"));
        Assert.False(localEvent.IsCelebratedInCountry("Chile"));
        Assert.True(localEvent.IsCelebratedInCountry(null));

        // Internacional
        Assert.True(intlEvent.IsInternational);
        Assert.True(intlEvent.IsCelebratedInCountry("España"));
        Assert.True(intlEvent.IsCelebratedInCountry("Chile"));
        Assert.True(intlEvent.IsCelebratedInCountry("México"));
        Assert.True(intlEvent.IsCelebratedInCountry(null));
    }

    [Fact]
    public void GamePurchaseLink_ShipsTo_WorksWithMultiCountryShipping()
    {
        var link = new GamePurchaseLink(
            storeName: "Zacatrus!",
            affiliateUrl: "https://zacatrus.es/juego",
            price: 39.95m,
            currency: "EUR",
            inStock: true,
            country: "España",
            shippingCountries: new[] { "España", "Portugal" }
        );

        Assert.True(link.ShipsTo("España"));
        Assert.True(link.ShipsTo("Portugal"));
        Assert.False(link.ShipsTo("México"));
        Assert.True(link.ShipsTo(null)); // Sin filtro devuelve true
    }

    [Fact]
    public void SleeveItem_ShipsTo_WorksWithSpecifiedCountries()
    {
        var sleeve = new SleeveItem(
            FormatName: "Estándar",
            WidthMm: 63.5,
            HeightMm: 88.0,
            CardCount: 110,
            AffiliateUrl: "https://maydaygames.com",
            StoreName: "Mayday",
            Country: "Estados Unidos",
            ShippingCountries: new[] { "Estados Unidos", "España", "México" }
        );

        Assert.True(sleeve.ShipsTo("España"));
        Assert.True(sleeve.ShipsTo("México"));
        Assert.False(sleeve.ShipsTo("Argentina"));
    }

    [Fact]
    public void AppUser_And_UserPreference_HandleCountryAssignmentAndNormalization()
    {
        var user = new AppUser(
            id: Guid.NewGuid().ToString(),
            userName: "ludico_esp",
            email: "esp@ludeka.es"
        );

        user.SetCountry("espana");
        Assert.Equal("España", user.Country);

        user.SetCountry(null);
        Assert.Null(user.Country);

        var pref = new UserPreference("user-123", "emerald");
        pref.SetCountry("mexico");
        Assert.Equal("México", pref.Country);

        pref.SetCountry("");
        Assert.Null(pref.Country);
    }
}
