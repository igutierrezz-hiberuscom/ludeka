using System;

namespace Ludeca.Core.ValueObjects;

public record UserFamilyExperienceVote
{
    public bool PlayedWithChildren { get; init; }
    public int? SuggestedMinAge { get; init; }
    public bool IsAdaptedRules { get; init; }

    public UserFamilyExperienceVote() { }

    public UserFamilyExperienceVote(bool playedWithChildren, int? suggestedMinAge = null, bool isAdaptedRules = false)
    {
        if (suggestedMinAge.HasValue && (suggestedMinAge.Value < 3 || suggestedMinAge.Value > 18))
        {
            throw new ArgumentOutOfRangeException(nameof(suggestedMinAge), "La edad mínima sugerida debe estar entre 3 y 18 años.");
        }

        PlayedWithChildren = playedWithChildren;
        SuggestedMinAge = suggestedMinAge;
        IsAdaptedRules = isAdaptedRules;
    }
}
