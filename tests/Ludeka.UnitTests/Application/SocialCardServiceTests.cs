using Ludeka.Application.DTOs;
using Ludeka.Application.Features.Community;
using Xunit;

namespace Ludeka.UnitTests.Application;

public class SocialCardServiceTests
{
    [Fact]
    public void GenerateCard_ReturnsSvgAndInstagramCaption()
    {
        // Arrange
        var service = new SocialCardService();
        var data = new SocialCardDataDto(
            GameTitle: "Brass: Birmingham",
            OriginalTitle: "Brass: Birmingham",
            YearPublished: 2018,
            Designer: "Martin Wallace, Gavan Brown",
            Publisher: "Maldito Games",
            CoverImageUrl: "https://cf.geekdo-images.com/brass.jpg",
            Rating: 8.6,
            StyleText: "⚙️ Eurogame",
            ConfrontationText: "⚔️ Competitivo",
            IdealPlayersText: "🟢 Ideal a 3 o 4 jugadores",
            EstimatedDurationPerPlayer: 45,
            FoundingVerdictBadge: "🛡️ Imprescindible");

        // Act
        var card = service.GenerateCard(data);

        // Assert
        Assert.NotNull(card);
        Assert.False(string.IsNullOrWhiteSpace(card.SvgContent));
        Assert.Contains("viewBox=\"0 0 1080 1080\"", card.SvgContent);
        Assert.Contains("Brass: Birmingham", card.SvgContent);
        Assert.Contains("Ludeka", card.SvgContent);
        Assert.Contains("Maldito Games", card.SvgContent);
        Assert.Contains("★ 8.6", card.SvgContent);

        Assert.False(string.IsNullOrWhiteSpace(card.InstagramCaption));
        Assert.Contains("Brass: Birmingham", card.InstagramCaption);
        Assert.Contains("#juegosdemesa", card.InstagramCaption);
        Assert.Contains("#malditogames", card.InstagramCaption);
        Assert.Contains("ludeka.app", card.InstagramCaption);

        Assert.Contains("brass:-birmingham", card.FileName);
    }
}
