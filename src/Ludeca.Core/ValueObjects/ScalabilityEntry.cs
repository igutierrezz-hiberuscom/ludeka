using Ludeca.Core.Enums;

namespace Ludeca.Core.ValueObjects;

public record ScalabilityEntry(
    int PlayerCount,
    string DisplayCount,
    ScalabilityStatus Status,
    int BestVotes = 0,
    int RecommendedVotes = 0,
    int NotRecommendedVotes = 0)
{
    public int TotalVotes => BestVotes + RecommendedVotes + NotRecommendedVotes;

    public bool IsRecommendedOrBest => Status is ScalabilityStatus.MustPlay or ScalabilityStatus.Recommended;
}
