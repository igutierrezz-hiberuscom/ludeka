using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Ludeka.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Seeding;

/// <summary>
/// Sembrador aditivo de Editoriales, Creadores y Tiendas de referencia en el ecosistema hispanohablante.
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

    private static async Task SeedCreatorsAsync(LudekaDbContext db, CancellationToken ct)
    {
        var existingSlugs = await db.Creators.Select(c => c.Slug).ToListAsync(ct);
        var toAdd = new List<Creator>();

        var elizabeth = new Creator(
            "Elizabeth Hargrave",
            "elizabeth-hargrave",
            "Estados Unidos",
            "Diseñadora de juegos de mesa y ornitóloga aficionada, creadora del aclamado y premiado Wingspan, Mariposas y Tussie Mussie.",
            "/images/creators/elizabeth-hargrave.png",
            104523,
            "https://www.elizabethhargrave.com",
            new[]
            {
                new SocialNetworkLink(SocialPlatform.Website, "https://www.elizabethhargrave.com"),
                new SocialNetworkLink(SocialPlatform.Twitter, "https://twitter.com/elizhargrave", "@elizhargrave")
            }
        );

        var klaus = new Creator(
            "Klaus Teuber",
            "klaus-teuber",
            "Alemania",
            "Leyenda del diseño de juegos de mesa moderno y cuatro veces galardonado con el Spiel des Jahres. Padre indiscutible de Catan (1995).",
            "/images/creators/klaus-teuber.png",
            84,
            "https://catan.com"
        );

        var uwe = new Creator(
            "Uwe Rosenberg",
            "uwe-rosenberg",
            "Alemania",
            "Uno de los autores europeos más prolíficos y admirados de todos los tiempos. Creador de Agricola, Caverna, Patchwork, Le Havre y Feast for Odin.",
            "/images/creators/uwe-rosenberg.png",
            10
        );

        var bruno = new Creator(
            "Bruno Cathala",
            "bruno-cathala",
            "Francia",
            "Maestro del juego de mesa dinámico y la tensión a dos jugadores. Creador de 7 Wonders Duel, Kingdomino, Abyss y Five Tribes.",
            "/images/creators/bruno-cathala.png",
            1727,
            "http://www.brunocathala.com"
        );

        var jacob = new Creator(
            "Jacob Fryxelius",
            "jacob-fryxelius",
            "Suecia",
            "Científico y autor sueco creador de Terraforming Mars y la saga espacial FryxGames.",
            null,
            47970
        );

        var jamey = new Creator(
            "Jamey Stegmaier",
            "jamey-stegmaier",
            "Estados Unidos",
            "Diseñador y cofundador de Stonemaier Games. Creador de Scythe, Viticulture y Euphoria, además de referente en divulgación de crowdfunding.",
            "/images/creators/jamey-stegmaier.png",
            61168,
            "https://stonemaiergames.com",
            new[]
            {
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@jameystegmaier", "@jameystegmaier", "Stonemaier Games Channel")
            }
        );

        var sergio = new Creator(
            "Sergio (Análisis Parálisis)",
            "analisis-paralisis",
            "España",
            "Referente absoluto de la divulgación audiovisual de juegos de mesa en español desde hace más de una década.",
            "/images/creators/analisis-paralisis.png",
            null,
            "https://analisisparalisis.es",
            new[]
            {
                new SocialNetworkLink(SocialPlatform.YouTube, "https://youtube.com/@AnalisisParalisis", "@AnalisisParalisis", "Canal Análisis Parálisis")
            }
        );

        var allSeed = new[] { elizabeth, klaus, uwe, bruno, jacob, jamey, sergio };
        foreach (var cr in allSeed)
        {
            if (!existingSlugs.Contains(cr.Slug))
            {
                toAdd.Add(cr);
            }
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
