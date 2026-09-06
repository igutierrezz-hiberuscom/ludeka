using System;

namespace Ludeka.Core.Entities;

public class RuleAnswer
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid QuestionId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? OfficialRuleReference { get; private set; }
    public int VotesCount { get; private set; }
    public bool IsAccepted { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    public virtual RuleQuestion? Question { get; private set; }

    private RuleAnswer() { }

    public RuleAnswer(
        Guid questionId,
        string userId,
        string userName,
        string body,
        string? officialRuleReference = null)
    {
        if (questionId == Guid.Empty)
            throw new ArgumentException("El ID de la pregunta no puede estar vacío.", nameof(questionId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));

        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("El nombre del usuario no puede estar vacío.", nameof(userName));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("El cuerpo de la respuesta no puede estar vacío.", nameof(body));

        QuestionId = questionId;
        UserId = userId.Trim();
        UserName = userName.Trim();
        Body = body.Trim();
        OfficialRuleReference = string.IsNullOrWhiteSpace(officialRuleReference) ? null : officialRuleReference.Trim();
        VotesCount = 0;
        IsAccepted = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Upvote()
    {
        VotesCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Downvote()
    {
        if (VotesCount > 0)
        {
            VotesCount--;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void SetAccepted(bool accepted)
    {
        IsAccepted = accepted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
