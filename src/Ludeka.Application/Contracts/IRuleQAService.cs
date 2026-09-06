using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface IRuleQAService
{
    Task<IReadOnlyList<RuleQuestionDto>> GetQuestionsByGameIdAsync(Guid gameId, string? currentUserId = null, CancellationToken ct = default);
    Task<RuleQuestionDto?> GetQuestionByIdAsync(Guid questionId, string? currentUserId = null, CancellationToken ct = default);
    Task<RuleQuestionDto> CreateQuestionAsync(CreateRuleQuestionRequest request, string userId, string userName, CancellationToken ct = default);
    Task<RuleAnswerDto> AddAnswerAsync(CreateRuleAnswerRequest request, string userId, string userName, CancellationToken ct = default);
    Task<bool> ToggleVoteQuestionAsync(Guid questionId, string userId, CancellationToken ct = default);
    Task<bool> ToggleVoteAnswerAsync(Guid answerId, string userId, CancellationToken ct = default);
    Task<RuleQuestionDto> MarkAcceptedAnswerAsync(Guid questionId, Guid answerId, string requestingUserId, bool isModerator, CancellationToken ct = default);
    Task<RuleQuestionDto> UnmarkAcceptedAnswerAsync(Guid questionId, string requestingUserId, bool isModerator, CancellationToken ct = default);
}
