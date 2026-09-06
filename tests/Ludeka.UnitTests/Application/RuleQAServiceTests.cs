using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Application.Features.Community;
using Ludeka.Core.Entities;
using Xunit;

namespace Ludeka.UnitTests.Application;

public class RuleQAServiceTests
{
    private class FakeRuleQARepository : IRuleQARepository
    {
        public List<RuleQuestion> Questions { get; } = new();
        public List<RuleAnswer> Answers { get; } = new();
        public List<RuleVote> Votes { get; } = new();

        public Task<IReadOnlyList<RuleQuestion>> GetQuestionsByGameIdAsync(Guid gameId, CancellationToken ct = default)
        {
            var result = Questions.Where(q => q.GameId == gameId).ToList();
            return Task.FromResult<IReadOnlyList<RuleQuestion>>(result);
        }

        public Task<RuleQuestion?> GetQuestionWithAnswersAsync(Guid questionId, CancellationToken ct = default)
        {
            return Task.FromResult(Questions.FirstOrDefault(q => q.Id == questionId));
        }

        public Task<RuleAnswer?> GetAnswerByIdAsync(Guid answerId, CancellationToken ct = default)
        {
            return Task.FromResult(Answers.FirstOrDefault(a => a.Id == answerId));
        }

        public Task<RuleVote?> GetUserVoteAsync(string userId, Guid? questionId, Guid? answerId, CancellationToken ct = default)
        {
            var vote = Votes.FirstOrDefault(v =>
                v.UserId == userId &&
                v.QuestionId == questionId &&
                v.AnswerId == answerId);
            return Task.FromResult(vote);
        }

        public Task<HashSet<Guid>> GetUserVotedQuestionIdsAsync(string userId, IEnumerable<Guid> questionIds, CancellationToken ct = default)
        {
            var idSet = questionIds.ToHashSet();
            var set = Votes.Where(v => v.UserId == userId && v.QuestionId.HasValue && idSet.Contains(v.QuestionId.Value))
                           .Select(v => v.QuestionId!.Value)
                           .ToHashSet();
            return Task.FromResult(set);
        }

        public Task<HashSet<Guid>> GetUserVotedAnswerIdsAsync(string userId, IEnumerable<Guid> answerIds, CancellationToken ct = default)
        {
            var idSet = answerIds.ToHashSet();
            var set = Votes.Where(v => v.UserId == userId && v.AnswerId.HasValue && idSet.Contains(v.AnswerId.Value))
                           .Select(v => v.AnswerId!.Value)
                           .ToHashSet();
            return Task.FromResult(set);
        }

        public Task AddQuestionAsync(RuleQuestion question, CancellationToken ct = default)
        {
            Questions.Add(question);
            return Task.CompletedTask;
        }

        public Task AddAnswerAsync(RuleAnswer answer, CancellationToken ct = default)
        {
            Answers.Add(answer);
            return Task.CompletedTask;
        }

        public Task AddVoteAsync(RuleVote vote, CancellationToken ct = default)
        {
            Votes.Add(vote);
            return Task.CompletedTask;
        }

        public Task RemoveVoteAsync(RuleVote vote, CancellationToken ct = default)
        {
            Votes.Remove(vote);
            return Task.CompletedTask;
        }

        public Task UpdateQuestionAsync(RuleQuestion question, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAnswerAsync(RuleAnswer answer, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task CreateQuestionAndAddAnswer_WorksCorrectly()
    {
        // Arrange
        var repo = new FakeRuleQARepository();
        var service = new RuleQAService(repo);
        var gameId = Guid.NewGuid();

        // Act 1: Crear duda
        var qRequest = new CreateRuleQuestionRequest(gameId, "¿Duda de turno?", "Explicación");
        var question = await service.CreateQuestionAsync(qRequest, "user-1", "Carlos");

        // Assert 1
        Assert.NotNull(question);
        Assert.Equal("¿Duda de turno?", question.Title);
        Assert.Single(repo.Questions);

        // Act 2: Añadir respuesta
        var aRequest = new CreateRuleAnswerRequest(question.Id, "Respuesta clara", "Pág. 5");
        var answer = await service.AddAnswerAsync(aRequest, "user-2", "Elena");

        // Assert 2
        Assert.NotNull(answer);
        Assert.Equal("Respuesta clara", answer.Body);
        Assert.Equal("Pág. 5", answer.OfficialRuleReference);
        Assert.Single(repo.Answers);
    }

    [Fact]
    public async Task ToggleVoteQuestionAsync_AlternatesVote()
    {
        // Arrange
        var repo = new FakeRuleQARepository();
        var service = new RuleQAService(repo);
        var question = new RuleQuestion(Guid.NewGuid(), "u1", "Carlos", "Duda", "Cuerpo");
        repo.Questions.Add(question);

        // Act 1: Votar
        var voted = await service.ToggleVoteQuestionAsync(question.Id, "voter-1");

        // Assert 1
        Assert.True(voted);
        Assert.Equal(1, question.VotesCount);
        Assert.Single(repo.Votes);

        // Act 2: Retirar voto
        var unvoted = await service.ToggleVoteQuestionAsync(question.Id, "voter-1");

        // Assert 2
        Assert.False(unvoted);
        Assert.Equal(0, question.VotesCount);
        Assert.Empty(repo.Votes);
    }

    [Fact]
    public async Task MarkAcceptedAnswerAsync_SortsAcceptedAnswerFirst()
    {
        // Arrange
        var repo = new FakeRuleQARepository();
        var service = new RuleQAService(repo);
        var gameId = Guid.NewGuid();
        var question = new RuleQuestion(gameId, "author-1", "Laura", "Duda", "Cuerpo");
        var answer1 = new RuleAnswer(question.Id, "u1", "Pablo", "Respuesta con más votos");
        answer1.Upvote();
        answer1.Upvote(); // 2 votos

        var answer2 = new RuleAnswer(question.Id, "u2", "Sara", "Solución oficial con 0 votos");

        question.AddAnswer(answer1);
        question.AddAnswer(answer2);
        repo.Questions.Add(question);
        repo.Answers.Add(answer1);
        repo.Answers.Add(answer2);

        // Act: Marcar answer2 como aceptada
        var result = await service.MarkAcceptedAnswerAsync(question.Id, answer2.Id, "author-1", isModerator: false);

        // Assert
        Assert.Equal(answer2.Id, result.AcceptedAnswerId);
        // Debe ser la primera respuesta en el DTO a pesar de tener menos votos
        Assert.Equal(answer2.Id, result.Answers[0].Id);
        Assert.True(result.Answers[0].IsAccepted);
        Assert.Equal(answer1.Id, result.Answers[1].Id);
    }
}
