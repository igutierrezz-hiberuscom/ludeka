using System;
using System.Collections.Generic;
using System.Linq;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

public record FoundingPhotoDto(
    string PhotoUrl,
    string Caption
);

public record FoundingVerdictDto(
    Guid Id,
    Guid GameId,
    string AuthorUserId,
    string AuthorName,
    FoundingRecommendation Recommendation,
    string RecommendationLabel,
    string OverallVerdict,
    string TwoPlayerVerdict,
    string FamilyVerdict,
    IReadOnlyList<FoundingPhotoDto> Photos,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
)
{
    public static FoundingVerdictDto FromEntity(FoundingVerdict v)
    {
        string label = v.Recommendation switch
        {
            FoundingRecommendation.MustPlay => "🏆 Imprescindible de la Mesa",
            FoundingRecommendation.RecommendedWithAdaptations => "🏷️ Recomendado con adaptaciones",
            FoundingRecommendation.Skippable => "📦 Prescindible",
            _ => "Veredicto Oficial"
        };

        var photos = v.Photos.Select(p => new FoundingPhotoDto(p.PhotoUrl, p.Caption)).ToList();

        return new FoundingVerdictDto(
            v.Id,
            v.GameId,
            v.AuthorUserId,
            v.AuthorName,
            v.Recommendation,
            label,
            v.OverallVerdict,
            v.TwoPlayerVerdict,
            v.FamilyVerdict,
            photos.AsReadOnly(),
            v.CreatedAt,
            v.UpdatedAt
        );
    }
}

public record SaveFoundingVerdictRequest(
    Guid GameId,
    FoundingRecommendation Recommendation,
    string OverallVerdict,
    string TwoPlayerVerdict,
    string FamilyVerdict,
    List<FoundingPhotoDto>? Photos = null
);

public record AiGameSummaryDto(
    Guid GameId,
    string GameTitle,
    string ScalabilitySummary,
    string AgeSummary,
    string FootprintSummary,
    string GeneralVerdict
);
