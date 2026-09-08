using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Seeding;

/// <summary>
/// Sembrador inicial de ferias, festivales y convenciones de referencia en el sector de los juegos de mesa.
/// </summary>
public static class BoardGameEventSeeder
{
    public static async Task SeedEventsAsync(LudekaDbContext db, CancellationToken ct = default)
    {
        if (await db.BoardGameEvents.AnyAsync(ct))
            return;

        var events = new List<BoardGameEvent>
        {
            new(
                title: "Festival Internacional de Juegos de Córdoba",
                description: "La mayor fiesta del juego de mesa en España. Cuatro días de juego libre, torneos, premios Jugo del Año y presentaciones con autores internacionales en los patios históricos del Palacio de la Merced.",
                imageUrl: "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&auto=format&fit=crop&q=80",
                startDate: new DateOnly(2026, 10, 9),
                endDate: new DateOnly(2026, 10, 12),
                location: "Palacio de la Merced, Córdoba",
                country: "España",
                websiteUrl: "https://festivaldejuegoscordoba.es",
                organizer: "Asociación Cultural Jugamos Tod@s",
                isOfficial: true
            ),
            new(
                title: "SPIEL Essen (Internationale Spieltage)",
                description: "La feria mundial por excelencia del juego de mesa de mesa. Más de 1.000 expositores de 50 países y el lanzamiento de más de 1.500 novedades lúdicas anuales.",
                imageUrl: "https://images.unsplash.com/photo-1610890716171-6b1bb98ffd09?w=800&auto=format&fit=crop&q=80",
                startDate: new DateOnly(2026, 10, 22),
                endDate: new DateOnly(2026, 10, 25),
                location: "Messe Essen, Alemania",
                country: "Internacional",
                websiteUrl: "https://spiel-essen.de",
                organizer: "Merz Verlag",
                isOfficial: true
            ),
            new(
                title: "DAU Barcelona — Festival del Joc",
                description: "Gran encuentro de juegos de mesa contemporáneos, divulgación, prototipos y entrega de los premios DAU a los mejores creadores del año en el recinto histórico Fabra i Coats.",
                imageUrl: "https://images.unsplash.com/photo-1543269865-cbf427effbad?w=800&auto=format&fit=crop&q=80",
                startDate: new DateOnly(2026, 11, 21),
                endDate: new DateOnly(2026, 11, 22),
                location: "Fabra i Coats, Barcelona",
                country: "España",
                websiteUrl: "https://www.barcelona.cat/festivaldau",
                organizer: "Institut de Cultura de Barcelona (ICUB)",
                isOfficial: true
            ),
            new(
                title: "Feria InterOcio Madrid",
                description: "La feria de referencia del ocio lúdico e interactivo en la capital de España. Más de 10.000 m² de mesas para jugar, demostraciones de editoriales y zona de prototipos.",
                imageUrl: "https://images.unsplash.com/photo-1511193311914-0346f16efe90?w=800&auto=format&fit=crop&q=80",
                startDate: new DateOnly(2027, 3, 12),
                endDate: new DateOnly(2027, 3, 14),
                location: "IFEMA Pabellón 1, Madrid",
                country: "España",
                websiteUrl: "https://feria-interocio.com",
                organizer: "InterOcio Eventos",
                isOfficial: true
            ),
            new(
                title: "Roll-a-Game Expo México",
                description: "La convención líder de juegos de mesa modernos en México. Demostraciones de editoriales latinoamericanas, torneo nacional y zona de creadores independientes.",
                imageUrl: "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&auto=format&fit=crop&q=80",
                startDate: new DateOnly(2027, 5, 21),
                endDate: new DateOnly(2027, 5, 23),
                location: "Expo Guadalajara, Jalisco",
                country: "México",
                websiteUrl: "https://rollagame.com",
                organizer: "Comunidad Lúdica Mexicana",
                isOfficial: true
            ),
            new(
                title: "Gen Con Indianapolis",
                description: "La convención de juegos de mesa y rol decana de Norteamérica, con más de 70.000 asistentes, eventos multitudinarios y el epicentro de la comunidad internacional.",
                imageUrl: "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80",
                startDate: new DateOnly(2027, 8, 5),
                endDate: new DateOnly(2027, 8, 8),
                location: "Indiana Convention Center, Indianápolis",
                country: "Internacional",
                websiteUrl: "https://www.gencon.com",
                organizer: "Gen Con LLC",
                isOfficial: true
            )
        };

        await db.BoardGameEvents.AddRangeAsync(events, ct);
        await db.SaveChangesAsync(ct);
    }
}
