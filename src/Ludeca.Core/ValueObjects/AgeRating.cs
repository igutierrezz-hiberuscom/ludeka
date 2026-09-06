namespace Ludeca.Core.ValueObjects;

public record AgeRating(int BoxAge, int CommunityAge)
{
    public bool IsAccessibleEarlier => CommunityAge < BoxAge;
}
