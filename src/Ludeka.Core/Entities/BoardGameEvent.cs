using System;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Core.Entities;

/// <summary>
/// Representa una gran cita, feria o festival del calendario lúdico (ej. Festival de Córdoba, InterOcio, Essen SPIEL).
/// </summary>
public class BoardGameEvent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string Country { get; private set; } = "España";
    public string Location { get; private set; } = string.Empty;
    public string? WebsiteUrl { get; private set; }
    public string Organizer { get; private set; } = string.Empty;
    public bool IsOfficial { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    public bool IsInternational => CountryCatalog.IsInternational(Country);

    // Constructor para EF Core
    private BoardGameEvent() { }

    public BoardGameEvent(
        string title,
        string description,
        string imageUrl,
        DateOnly startDate,
        DateOnly endDate,
        string location,
        string? websiteUrl = null,
        string organizer = "",
        bool isOfficial = true,
        string country = "España")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del evento no puede estar vacío.", nameof(title));

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("La imagen o cartel del evento es obligatoria.", nameof(imageUrl));

        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("La ubicación o ciudad del evento es obligatoria.", nameof(location));

        if (endDate < startDate)
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio del evento.", nameof(endDate));

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        ImageUrl = imageUrl.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Location = location.Trim();
        WebsiteUrl = string.IsNullOrWhiteSpace(websiteUrl) ? null : websiteUrl.Trim();
        Organizer = organizer?.Trim() ?? string.Empty;
        IsOfficial = isOfficial;
        Country = string.IsNullOrWhiteSpace(country) ? "España" : CountryCatalog.Normalize(country);
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(
        string title,
        string description,
        string imageUrl,
        DateOnly startDate,
        DateOnly endDate,
        string location,
        string? websiteUrl,
        string organizer,
        bool isOfficial,
        string country = "España")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del evento no puede estar vacío.", nameof(title));

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("La imagen o cartel del evento es obligatoria.", nameof(imageUrl));

        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("La ubicación o ciudad del evento es obligatoria.", nameof(location));

        if (endDate < startDate)
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio del evento.", nameof(endDate));

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        ImageUrl = imageUrl.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Location = location.Trim();
        WebsiteUrl = string.IsNullOrWhiteSpace(websiteUrl) ? null : websiteUrl.Trim();
        Organizer = organizer?.Trim() ?? string.Empty;
        IsOfficial = isOfficial;
        Country = string.IsNullOrWhiteSpace(country) ? "España" : CountryCatalog.Normalize(country);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsCelebratedInCountry(string? targetCountry)
    {
        if (string.IsNullOrWhiteSpace(targetCountry))
            return true;

        if (IsInternational)
            return true;

        return string.Equals(CountryCatalog.Normalize(Country), CountryCatalog.Normalize(targetCountry), StringComparison.OrdinalIgnoreCase);
    }

    public bool IsOngoing(DateOnly today) => today >= StartDate && today <= EndDate;

    public bool IsPast(DateOnly today) => today > EndDate;

    public int DaysUntilStart(DateOnly today) => StartDate.DayNumber - today.DayNumber;

    public string GetFormattedDates()
    {
        if (StartDate == EndDate)
        {
            return $"{StartDate.Day} {GetSpanishMonth(StartDate.Month)} {StartDate.Year}";
        }

        if (StartDate.Month == EndDate.Month && StartDate.Year == EndDate.Year)
        {
            return $"{StartDate.Day}-{EndDate.Day} {GetSpanishMonth(StartDate.Month)} {StartDate.Year}";
        }

        return $"{StartDate.Day} {GetSpanishMonth(StartDate.Month)} - {EndDate.Day} {GetSpanishMonth(EndDate.Month)} {EndDate.Year}";
    }

    private static string GetSpanishMonth(int month) => month switch
    {
        1 => "Ene",
        2 => "Feb",
        3 => "Mar",
        4 => "Abr",
        5 => "May",
        6 => "Jun",
        7 => "Jul",
        8 => "Ago",
        9 => "Sep",
        10 => "Oct",
        11 => "Nov",
        12 => "Dic",
        _ => ""
    };
}
