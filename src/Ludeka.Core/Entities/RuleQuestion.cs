using System;
using System.Collections.Generic;

namespace Ludeka.Core.Entities;

public class RuleQuestion
{
    private readonly List<RuleAnswer> _answers = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid GameId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public int VotesCount { get; private set; }
    public Guid? AcceptedAnswerId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    public virtual Game? Game { get; private set; }
    public virtual IReadOnlyCollection<RuleAnswer> Answers => _answers.AsReadOnly();

    private RuleQuestion() { }

    public RuleQuestion(
        Guid gameId,
        string userId,
        string userName,
        string title,
        string body)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException("El ID del juego no puede estar vacío.", nameof(gameId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));

        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("El nombre del usuario no puede estar vacío.", nameof(userName));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título de la duda no puede estar vacío.", nameof(title));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("El cuerpo de la duda no puede estar vacío.", nameof(body));

        GameId = gameId;
        UserId = userId.Trim();
        UserName = userName.Trim();
        Title = title.Trim();
        Body = body.Trim();
        VotesCount = 0;
        AcceptedAnswerId = null;
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

    public void AddAnswer(RuleAnswer answer)
    {
        ArgumentNullException.ThrowIfNull(answer);
        _answers.Add(answer);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAcceptedAnswer(Guid answerId, string requestingUserId, bool isModerator)
    {
        if (string.IsNullOrWhiteSpace(requestingUserId))
            throw new ArgumentException("El ID del usuario solicitante no puede estar vacío.", nameof(requestingUserId));

        if (!isModerator && !string.Equals(UserId, requestingUserId, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Solo el autor de la pregunta o un moderador pueden marcar una respuesta como aceptada.");

        // Desmarcar cualquier otra respuesta previamente aceptada
        foreach (var answer in _answers)
        {
            if (answer.Id == answerId)
            {
                answer.SetAccepted(true);
            }
            else if (answer.IsAccepted)
            {
                answer.SetAccepted(false);
            }
        }

        AcceptedAnswerId = answerId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UnmarkAcceptedAnswer(string requestingUserId, bool isModerator)
    {
        if (string.IsNullOrWhiteSpace(requestingUserId))
            throw new ArgumentException("El ID del usuario solicitante no puede estar vacío.", nameof(requestingUserId));

        if (!isModerator && !string.Equals(UserId, requestingUserId, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Solo el autor de la pregunta o un moderador pueden desmarcar una respuesta aceptada.");

        foreach (var answer in _answers)
        {
            if (answer.IsAccepted)
            {
                answer.SetAccepted(false);
            }
        }

        AcceptedAnswerId = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
