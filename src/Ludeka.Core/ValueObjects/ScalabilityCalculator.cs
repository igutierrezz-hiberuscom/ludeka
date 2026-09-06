using Ludeka.Core.Enums;

namespace Ludeka.Core.ValueObjects;

public static class ScalabilityCalculator
{
    public static ScalabilityStatus DetermineStatus(int bestVotes, int recommendedVotes, int notRecommendedVotes)
    {
        int total = bestVotes + recommendedVotes + notRecommendedVotes;
        if (total == 0)
        {
            return ScalabilityStatus.NotRecommended;
        }

        // Si los votos "Best" superan o igualan a la suma de recommended y notRecommended -> MustPlay
        if (bestVotes > 0 && bestVotes >= (recommendedVotes + notRecommendedVotes))
        {
            return ScalabilityStatus.MustPlay;
        }

        // Si la mayoría aprueba el juego (Best + Recommended > NotRecommended) -> Recommended
        if ((bestVotes + recommendedVotes) > notRecommendedVotes)
        {
            return ScalabilityStatus.Recommended;
        }

        // De lo contrario -> NotRecommended
        return ScalabilityStatus.NotRecommended;
    }
}
