using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface ISocialCardService
{
    GeneratedSocialCardDto GenerateCard(SocialCardDataDto data);
    string GenerateSvg(SocialCardDataDto data);
    string GenerateInstagramCaption(SocialCardDataDto data);
}
