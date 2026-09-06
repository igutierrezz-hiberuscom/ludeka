using System;
using Ludeka.Core.Entities;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class RuleQATests
{
    [Fact]
    public void Question_VotesAndAnswers_ManageCorrectly()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var question = new RuleQuestion(
            gameId: gameId,
            userId: "user-1",
            userName: "Carlos",
            title: "¿Puedo construir dos redes en el mismo turno?",
            body: "Jugando a 3 jugadores no me queda claro el coste del carbón.");

        var answer1 = new RuleAnswer(
            questionId: question.Id,
            userId: "user-2",
            userName: "Elena",
            body: "Sí, siempre que pagues la cerveza adicional.",
            officialRuleReference: "Reglamento oficial pág. 10");

        // Act
        question.Upvote();
        question.Upvote();
        question.Downvote();
        question.AddAnswer(answer1);

        // Assert
        Assert.Equal(1, question.VotesCount);
        Assert.Single(question.Answers);
        Assert.Null(question.AcceptedAnswerId);
    }

    [Fact]
    public void MarkAcceptedAnswer_Author_Succeeds()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var question = new RuleQuestion(gameId, "user-author", "Laura", "¿Duda?", "Explicación");
        var answer1 = new RuleAnswer(question.Id, "user-expert", "Pablo", "Respuesta exacta");
        var answer2 = new RuleAnswer(question.Id, "user-other", "Ana", "Otra respuesta");
        question.AddAnswer(answer1);
        question.AddAnswer(answer2);

        // Act
        question.MarkAcceptedAnswer(answer1.Id, "user-author", isModerator: false);

        // Assert
        Assert.Equal(answer1.Id, question.AcceptedAnswerId);
        Assert.True(answer1.IsAccepted);
        Assert.False(answer2.IsAccepted);
    }

    [Fact]
    public void MarkAcceptedAnswer_Moderator_Succeeds()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var question = new RuleQuestion(gameId, "user-author", "Laura", "¿Duda?", "Explicación");
        var answer = new RuleAnswer(question.Id, "user-expert", "Pablo", "Respuesta exacta");
        question.AddAnswer(answer);

        // Act
        question.MarkAcceptedAnswer(answer.Id, "user-mod", isModerator: true);

        // Assert
        Assert.Equal(answer.Id, question.AcceptedAnswerId);
        Assert.True(answer.IsAccepted);
    }

    [Fact]
    public void MarkAcceptedAnswer_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var question = new RuleQuestion(gameId, "user-author", "Laura", "¿Duda?", "Explicación");
        var answer = new RuleAnswer(question.Id, "user-expert", "Pablo", "Respuesta exacta");
        question.AddAnswer(answer);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() =>
            question.MarkAcceptedAnswer(answer.Id, "user-intruder", isModerator: false));
    }

    [Fact]
    public void MarkAcceptedAnswer_SwitchingAnswers_UnmarksPrevious()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var question = new RuleQuestion(gameId, "user-author", "Laura", "¿Duda?", "Explicación");
        var answer1 = new RuleAnswer(question.Id, "user-1", "Pablo", "Solución 1");
        var answer2 = new RuleAnswer(question.Id, "user-2", "Marta", "Solución 2 corregida");
        question.AddAnswer(answer1);
        question.AddAnswer(answer2);

        // Act
        question.MarkAcceptedAnswer(answer1.Id, "user-author", isModerator: false);
        Assert.True(answer1.IsAccepted);

        question.MarkAcceptedAnswer(answer2.Id, "user-author", isModerator: false);

        // Assert
        Assert.False(answer1.IsAccepted);
        Assert.True(answer2.IsAccepted);
        Assert.Equal(answer2.Id, question.AcceptedAnswerId);
    }

    [Fact]
    public void RuleVote_Validations_WorkAsExpected()
    {
        var qId = Guid.NewGuid();
        var aId = Guid.NewGuid();

        // Valid question vote
        var voteQ = new RuleVote("user-1", questionId: qId);
        Assert.Equal(qId, voteQ.QuestionId);
        Assert.Null(voteQ.AnswerId);

        // Valid answer vote
        var voteA = new RuleVote("user-1", answerId: aId);
        Assert.Equal(aId, voteA.AnswerId);
        Assert.Null(voteA.QuestionId);

        // Invalid: both null
        Assert.Throws<ArgumentException>(() => new RuleVote("user-1"));

        // Invalid: both set
        Assert.Throws<ArgumentException>(() => new RuleVote("user-1", questionId: qId, answerId: aId));
    }
}
