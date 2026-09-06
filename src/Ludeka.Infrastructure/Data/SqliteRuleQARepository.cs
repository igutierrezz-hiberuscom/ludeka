using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class SqliteRuleQARepository : IRuleQARepository
{
    private readonly LudekaDbContext _context;

    public SqliteRuleQARepository(LudekaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<RuleQuestion>> GetQuestionsByGameIdAsync(Guid gameId, CancellationToken ct = default)
    {
        var list = await _context.RuleQuestions
            .Include(q => q.Answers)
            .Where(q => q.GameId == gameId)
            .ToListAsync(ct);

        // Ordenar en memoria por votos descendente y luego por fecha
        return list
            .OrderByDescending(q => q.VotesCount)
            .ThenByDescending(q => q.CreatedAt)
            .ToList();
    }

    public async Task<RuleQuestion?> GetQuestionWithAnswersAsync(Guid questionId, CancellationToken ct = default)
    {
        return await _context.RuleQuestions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == questionId, ct);
    }

    public async Task<RuleAnswer?> GetAnswerByIdAsync(Guid answerId, CancellationToken ct = default)
    {
        return await _context.RuleAnswers
            .FirstOrDefaultAsync(a => a.Id == answerId, ct);
    }

    public async Task<RuleVote?> GetUserVoteAsync(string userId, Guid? questionId, Guid? answerId, CancellationToken ct = default)
    {
        return await _context.RuleVotes.FirstOrDefaultAsync(v =>
            v.UserId == userId &&
            v.QuestionId == questionId &&
            v.AnswerId == answerId, ct);
    }

    public async Task<HashSet<Guid>> GetUserVotedQuestionIdsAsync(string userId, IEnumerable<Guid> questionIds, CancellationToken ct = default)
    {
        var idList = questionIds.ToList();
        var votedIds = await _context.RuleVotes
            .Where(v => v.UserId == userId && v.QuestionId.HasValue && idList.Contains(v.QuestionId.Value))
            .Select(v => v.QuestionId!.Value)
            .ToListAsync(ct);

        return votedIds.ToHashSet();
    }

    public async Task<HashSet<Guid>> GetUserVotedAnswerIdsAsync(string userId, IEnumerable<Guid> answerIds, CancellationToken ct = default)
    {
        var idList = answerIds.ToList();
        var votedIds = await _context.RuleVotes
            .Where(v => v.UserId == userId && v.AnswerId.HasValue && idList.Contains(v.AnswerId.Value))
            .Select(v => v.AnswerId!.Value)
            .ToListAsync(ct);

        return votedIds.ToHashSet();
    }

    public async Task AddQuestionAsync(RuleQuestion question, CancellationToken ct = default)
    {
        await _context.RuleQuestions.AddAsync(question, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddAnswerAsync(RuleAnswer answer, CancellationToken ct = default)
    {
        await _context.RuleAnswers.AddAsync(answer, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddVoteAsync(RuleVote vote, CancellationToken ct = default)
    {
        await _context.RuleVotes.AddAsync(vote, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RemoveVoteAsync(RuleVote vote, CancellationToken ct = default)
    {
        _context.RuleVotes.Remove(vote);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateQuestionAsync(RuleQuestion question, CancellationToken ct = default)
    {
        _context.RuleQuestions.Update(question);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAnswerAsync(RuleAnswer answer, CancellationToken ct = default)
    {
        _context.RuleAnswers.Update(answer);
        await _context.SaveChangesAsync(ct);
    }
}
