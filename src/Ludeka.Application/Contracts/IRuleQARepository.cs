using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface IRuleQARepository
{
    Task<IReadOnlyList<RuleQuestion>> GetQuestionsByGameIdAsync(Guid gameId, CancellationToken ct = default);
    Task<RuleQuestion?> GetQuestionWithAnswersAsync(Guid questionId, CancellationToken ct = default);
    Task<RuleAnswer?> GetAnswerByIdAsync(Guid answerId, CancellationToken ct = default);
    Task<RuleVote?> GetUserVoteAsync(string userId, Guid? questionId, Guid? answerId, CancellationToken ct = default);
    Task<HashSet<Guid>> GetUserVotedQuestionIdsAsync(string userId, IEnumerable<Guid> questionIds, CancellationToken ct = default);
    Task<HashSet<Guid>> GetUserVotedAnswerIdsAsync(string userId, IEnumerable<Guid> answerIds, CancellationToken ct = default);
    Task AddQuestionAsync(RuleQuestion question, CancellationToken ct = default);
    Task AddAnswerAsync(RuleAnswer answer, CancellationToken ct = default);
    Task AddVoteAsync(RuleVote vote, CancellationToken ct = default);
    Task RemoveVoteAsync(RuleVote vote, CancellationToken ct = default);
    Task UpdateQuestionAsync(RuleQuestion question, CancellationToken ct = default);
    Task UpdateAnswerAsync(RuleAnswer answer, CancellationToken ct = default);
}
