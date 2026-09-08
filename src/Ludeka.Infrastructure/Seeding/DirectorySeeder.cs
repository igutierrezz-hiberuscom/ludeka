using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.YouTube;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Seeding;

/// <summary>
/// Sembrador aditivo de Editoriales, Creadores de contenido y Tiendas de referencia en el ecosistema hispanohablante.
/// Purga los diseñadores de juegos retirados del directorio y siembra únicamente creadores de contenido (INC-31).
/// </summary>
public static class DirectorySeeder
{
    public static async Task SeedDirectoryAsync(LudekaDbContext db, CancellationToken ct = default)
    {
        await SeedPublishersAsync(db, ct);
        await SeedCreatorsAsync(db, ct);
        await SeedStoresAsync(db, ct);
    }

    private static async Task SeedPublishersAsync(LudekaDbContext db, CancellationToken ct)
    {
        var existingSlugs = await db.Publishers.Select(p => p.Slug).ToListAsync(ct);
        var toAdd = new List<Publisher>();

        var devir = new Publisher(
            "Devir Iberia",
            "devir-iberia",
            "España",
            "Barcelona",
            "Editorial decana del juego de mesa moderno en español, responsable de Catan, Carcassonne y su reputada línea propia de autores nacionales.",
            "/images/publishers/devir.png",
            "https://devir.es",
            new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://devir.es"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@devirtv", "@devirtv", "Devir TV (Tutoriales y Directos)"),
                new SocialNetworkLink(SocialPlatform.Instagram, "https://instagram.com/deviriberia", "@deviriberia")
            }
        );

        var maldito = new Publisher(
            "Maldito Games",
            "maldito-games",
            "España",
            "Sevilla",
            "Editorial especializada en grandes producciones, eurogames de peso medio y experto, y localizaciones de alta fidelidad.",
            "/images/publishers/maldito.png",
            "https://malditogames.com",
            new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://malditogames.com"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@MalditoGames", "@MalditoGames", "Maldito Games Oficial")
            }
        );

        var tranjis = new Publisher(
            "Tranjis Games",
            "tranjis-games",
            "España",
            "Madrid",
            "Editorial creadora del fenómeno superventas 'Virus!' y referente en juegos accesibles para toda la familia.",
            "/images/publishers/tranjis.png",
            "https://tranjisgames.com",
            new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://tranjisgames.com"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@TranjisGames", "@TranjisGames", "Tranjis Games TV")
            }
        );

        var asmodee = new Publisher(
            "Asmodee Ibérica",
            "asmodee-iberica",
            "España",
            "Madrid",
            "Líder internacional en distribución y publicación de títulos clave del hobby como Dixit, 7 Wonders o Exploding Kittens.",
            "/images/publishers/asmodee.png",
            "https://asmodee.es",
            new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://asmodee.es"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@AsmodeeIberica", "@AsmodeeIberica", "Asmodee España")
            }
        );

        var stonemaier = new Publisher(
            "Stonemaier Games",
            "stonemaier-games",
            "EEUU",
            "St. Louis",
            "Editorial internacional conocida por sus cuidadas producciones como Wingspan, Scythe y Viticulture.",
            "/images/publishers/stonemaier.png",
            "https://stonemaiergames.com",
            new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://stonemaiergames.com"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@jameystegmaier", "@jameystegmaier", "Jamey Stegmaier")
            }
        );

        var tcg = new Publisher(
            "TCG Factory",
            "tcg-factory",
            "España",
            "Zaragoza",
            "Editorial y distribuidora española con catálogo variado infantil, familiar y temático.",
            null,
            "https://tcgfactory.com",
            new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://tcgfactory.com"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@tcgfactory", "@tcgfactory")
            }
        );

        var tomatoes = new Publisher(
            "2Tomatoes Games",
            "2tomatoes-games",
            "España",
            "Barcelona",
            "Editorial independiente centrada en juegos asimétricos, de autor y producciones de culto como Root.",
            null,
            "https://2tomatoesgames.com",
            new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://2tomatoesgames.com"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@2tomatoesgames", "@2tomatoesgames")
            }
        );

        var allSeed = new[] { devir, maldito, tranjis, asmodee, stonemaier, tcg, tomatoes };
        foreach (var pub in allSeed)
        {
            if (!existingSlugs.Contains(pub.Slug))
            {
                toAdd.Add(pub);
            }
        }

        if (toAdd.Count > 0)
        {
            await db.Publishers.AddRangeAsync(toAdd, ct);
            await db.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Slugs de los diseñadores de juegos sembrados por el seed histórico del directorio (INC-31).
    /// Lista cerrada: la purga solo elimina estas filas de semilla, jamás creators manuales.
    /// </summary>
    private static readonly string[] RetiredSeedCreatorSlugs =
    [
        "elizabeth-hargrave", "klaus-teuber", "uwe-rosenberg",
        "bruno-cathala", "jacob-fryxelius", "jamey-stegmaier"
    ];

    private static async Task SeedCreatorsAsync(LudekaDbContext db, CancellationToken ct)
    {
        // 1. PURGA quirúrgica: elimina solo los diseñadores de la lista cerrada de semillas retiradas.
        var retired = await db.Creators
            .Where(c => RetiredSeedCreatorSlugs.Contains(c.Slug))
            .ToListAsync(ct);
        if (retired.Count > 0)
        {
            db.Creators.RemoveRange(retired);
            await db.SaveChangesAsync(ct);
        }

        // 2. RE-SIEMBRA aditiva desde el padrón estático (fuente única), sin updates sobre existentes.
        var existingSlugs = await db.Creators.Select(c => c.Slug).ToListAsync(ct);
        var toAdd = new List<Creator>();

        foreach (var entry in ChannelFocusProvider.GetStaticCreators())
        {
            var slug = Game.GenerateSlug(entry.ChannelName);
            if (existingSlugs.Contains(slug))
            {
                continue;
            }

            var socialLinks = string.IsNullOrWhiteSpace(entry.Handle)
                ? null
                : new[]
                {
                    new SocialNetworkLink(
                        SocialPlatform.YouTube,
                        $"https://youtube.com/{entry.Handle}",
                        entry.Handle)
                };

            toAdd.Add(new Creator(
                name: entry.ChannelName,
                slug: slug,
                nationality: null,
                bio: entry.Description,
                avatarUrl: null,
                bggPersonId: null,
                websiteUrl: null,
                socialLinks: socialLinks
            ));
        }

        if (toAdd.Count > 0)
        {
            await db.Creators.AddRangeAsync(toAdd, ct);
            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task SeedStoresAsync(LudekaDbContext db, CancellationToken ct)
    {
        var existingSlugs = await db.Stores.Select(s => s.Slug).ToListAsync(ct);
        var toAdd = new List<Store>();

        var zacatrus = new Store(
            name: "Zacatrus!",
            slug: "zacatrus",
            type: StoreType.Hybrid,
            country: "España",
            city: "Madrid",
            address: "Calle Fernández de los Ríos 57 (tiendas en Madrid, Barcelona, Valencia, Sevilla, etc.)",
            description: "Cadena de tiendas físicas y tienda online referente con ludoteca de prueba, talleres y canal didáctico.",
            logoUrl: "/images/stores/zacatrus.png",
            websiteUrl: "https://zacatrus.es",
            affiliateCode: "LDKZAC",
            hasLoyaltyProgram: true,
            socialLinks: new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://zacatrus.es"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@zacatrustv", "@zacatrustv", "Zacatrus TV"),
                new SocialNetworkLink(SocialPlatform.Instagram, "https://instagram.com/zacatrus", "@zacatrus")
            },
            shippingCountries: new[] { "España", "Internacional" }
        );

        var cuartoDeJuegos = new Store(
            name: "Cuarto de Juegos",
            slug: "cuarto-de-juegos",
            type: StoreType.Hybrid,
            country: "España",
            city: "Madrid",
            address: "Calle Jorge Juan 42, 28001 Madrid",
            description: "Mítica tienda madrileña de juegos de mesa tradicionales y de autor con décadas de experiencia.",
            logoUrl: "/images/stores/cuartodejuegos.png",
            websiteUrl: "https://cuartodejuegos.es",
            affiliateCode: "LDKCDJ",
            hasLoyaltyProgram: true,
            socialLinks: new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://cuartodejuegos.es"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@cuartodejuegos", "@cuartodejuegos")
            },
            shippingCountries: new[] { "España" }
        );

        var dungeonMarvels = new Store(
            name: "Dungeon Marvels",
            slug: "dungeon-marvels",
            type: StoreType.Hybrid,
            country: "España",
            city: "Barcelona",
            address: "Carrer d'Olzinelles 29, 08014 Barcelona",
            description: "Tienda especializada de Barcelona con amplio surtido de eurogames, wargames y miniaturas.",
            logoUrl: null,
            websiteUrl: "https://dungeonmarvels.com",
            affiliateCode: "LDKDM",
            hasLoyaltyProgram: true,
            socialLinks: new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://dungeonmarvels.com"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@dungeonmarvels", "@dungeonmarvels")
            },
            shippingCountries: new[] { "España" }
        );

        var jugamosOtra = new Store(
            name: "Jugamos Otra",
            slug: "jugamos-otra",
            type: StoreType.OnlineOnly,
            country: "España",
            city: null,
            address: null,
            description: "Tienda online española especializada en novedades de importación y catálogo nacional.",
            logoUrl: null,
            websiteUrl: "https://jugamosotra.com",
            affiliateCode: "LDKJO",
            hasLoyaltyProgram: false,
            socialLinks: new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://jugamosotra.com"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@jugamosotra", "@jugamosotra")
            },
            shippingCountries: new[] { "España" }
        );

        var jugandoAndo = new Store(
            name: "Jugando Ando",
            slug: "jugando-ando",
            type: StoreType.Hybrid,
            country: "México",
            city: "Ciudad de México",
            address: "Av. Insurgentes Sur 300, Roma Norte, CDMX",
            description: "Tienda mexicana especializada en juegos de mesa modernos, torneos comunitarios y catálogo latinoamericano.",
            logoUrl: null,
            websiteUrl: "https://jugandoando.mx",
            affiliateCode: "LDKJAMX",
            hasLoyaltyProgram: true,
            socialLinks: new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://jugandoando.mx"),
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@jugandoandomx", "@jugandoandomx", "Jugando Ando México")
            },
            shippingCountries: new[] { "México" }
        );

        var elOgroAlegre = new Store(
            name: "El Ogro Alegre",
            slug: "el-ogro-alegre",
            type: StoreType.Hybrid,
            country: "Argentina",
            city: "Buenos Aires",
            address: "Av. Corrientes 1500, CABA, Buenos Aires",
            description: "Tienda de referencia en Argentina con amplia selección de editoriales locales y juegos de importación.",
            logoUrl: null,
            websiteUrl: "https://elogroalegre.com.ar",
            affiliateCode: "LDKARG",
            hasLoyaltyProgram: true,
            socialLinks: new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://elogroalegre.com.ar")
            },
            shippingCountries: new[] { "Argentina" }
        );

        var allSeed = new[] { zacatrus, cuartoDeJuegos, dungeonMarvels, jugamosOtra, jugandoAndo, elOgroAlegre };
        foreach (var s in allSeed)
        {
            if (!existingSlugs.Contains(s.Slug))
            {
                toAdd.Add(s);
            }
        }

        if (toAdd.Count > 0)
        {
            await db.Stores.AddRangeAsync(toAdd, ct);
            await db.SaveChangesAsync(ct);
        }
    }
}
