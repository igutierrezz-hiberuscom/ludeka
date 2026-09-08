using Ludeka.Core.Enums;

namespace Ludeka.Application.Contracts;

public interface IInstagramComposerService
{
    string ComposeSvg(InstagramPostSourceType sourceType, object sourceEntity, string theme = "Dark");
    string GenerateCaption(InstagramPostSourceType sourceType, object sourceEntity);
}
