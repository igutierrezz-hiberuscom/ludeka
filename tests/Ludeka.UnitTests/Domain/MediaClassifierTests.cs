using Ludeka.Core.Enums;
using Ludeka.Core.Helpers;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class MediaClassifierTests
{
    [Theory]
    [InlineData("Cómo funciona Catan en 3 minutos")]
    [InlineData("Vistazo rápido a Wingspan")]
    [InlineData("Terraforming Mars en 2 minutos")]
    [InlineData("Ark Nova quick overview")]
    [InlineData("Quick look at Dune Imperium")]
    [InlineData("Resumen de mecánicas de Cascadia")]
    [InlineData("Mecánicas en 3 minutos de Splendor")]
    public void Classify_WithQuickOverviewPatterns_ShouldReturnQuickOverview(string title)
    {
        var category = MediaClassifier.Classify(title);
        Assert.Equal(MediaCategory.QuickOverview, category);
    }

    [Theory]
    [InlineData("Partida completa a 2 jugadores de Scythe")]
    [InlineData("Gameplay en español de Root")]
    [InlineData("Jugando a Brass Birmingham")]
    [InlineData("Full playthrough of Spirit Island")]
    [InlineData("Let's play Concordia")]
    [InlineData("Duelo a 2: 7 Wonders Duel")]
    [InlineData("Partida en solitario contra el bot")]
    public void Classify_WithGameplayPatterns_ShouldReturnGameplay(string title)
    {
        var category = MediaClassifier.Classify(title);
        Assert.Equal(MediaCategory.Gameplay, category);
    }

    [Theory]
    [InlineData("Reseña completa de Heat: Pedal to the Metal")]
    [InlineData("Nuestra opinión sincera tras 10 partidas")]
    [InlineData("Análisis a fondo de Nemesis")]
    [InlineData("Primeras impresiones de Arcs")]
    [InlineData("¿Vale la pena comprar Voidfall?")]
    [InlineData("Veredicto final sobre Ark Nova")]
    [InlineData("Review en español de Everdell")]
    [InlineData("Unboxing y componentes de Gloomhaven")]
    [InlineData("Abriendo la caja de Blood Rage")]
    [InlineData("Desempaquetado de la edición coleccionista")]
    public void Classify_WithReviewOpinionPatterns_ShouldReturnReviewOpinion(string title)
    {
        var category = MediaClassifier.Classify(title);
        Assert.Equal(MediaCategory.ReviewOpinion, category);
    }

    [Theory]
    [InlineData("Cómo jugar a Carcassonne")]
    [InlineData("Tutorial completo de Los Colonos de Catan")]
    [InlineData("Aprende a jugar a Wingspan")]
    [InlineData("Reglas y explicación de Concordia")]
    [InlineData("Guía paso a paso para jugar a Azul")]
    [InlineData("How to play Ticket to Ride")]
    public void Classify_WithTutorialPatterns_ShouldReturnTutorial(string title)
    {
        var category = MediaClassifier.Classify(title);
        Assert.Equal(MediaCategory.Tutorial, category);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Catan El Juego")]
    [InlineData("Edición 25 aniversario")]
    public void Classify_WithFallbackOrBlank_ShouldReturnTutorial(string title)
    {
        var category = MediaClassifier.Classify(title);
        Assert.Equal(MediaCategory.Tutorial, category);
    }

    [Fact]
    public void Classify_WithQuickOverviewInTitle_ShouldPreemptTutorial()
    {
        var title = "Cómo funciona Catan y cómo jugar en 2 minutos";
        var category = MediaClassifier.Classify(title);
        Assert.Equal(MediaCategory.QuickOverview, category);
    }

    [Fact]
    public void Classify_WithKeywordsInDescription_ShouldIdentifyCategory()
    {
        var title = "Catan 2026";
        var description = "En este vídeo compartimos nuestra opinión y reseña tras probar la nueva expansión.";
        var category = MediaClassifier.Classify(title, description);
        Assert.Equal(MediaCategory.ReviewOpinion, category);
    }
}
