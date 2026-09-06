using System;

namespace Ludeka.Core.Entities;

public class RuleVote
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public Guid? QuestionId { get; private set; }
    public Guid? AnswerId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private RuleVote() { }

    public RuleVote(string userId, Guid? questionId = null, Guid? answerId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));

        if (!questionId.HasValue && !answerId.HasValue)
            throw new ArgumentException("El voto debe estar asociado a una pregunta o a una respuesta.");

        if (questionId.HasValue && answerId.HasValue)
            throw new ArgumentException("El voto no puede estar asociado simultáneamente a una pregunta y a una respuesta.");

        UserId = userId.Trim();
        QuestionId = questionId;
        AnswerId = answerId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
