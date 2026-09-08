using System;
using System.Collections.Generic;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Core.Entities;

/// <summary>
/// Representa una entrada inmutable en la bitácora de auditoría editorial de Ludeka.
/// Registra qué usuario ejecutó la acción, en qué momento exacto (UTC), sobre qué entidad
/// y el detalle estructurado de campos modificados (diff de valores).
/// </summary>
public class AuditLogEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.UtcNow;
    public AuditAction Action { get; private set; }
    public AuditEntityType EntityType { get; private set; }
    public string EntityId { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public List<AuditFieldChange> Changes { get; private set; } = [];

    // Constructor privado para EF Core
    private AuditLogEntry() { }

    public AuditLogEntry(
        string userId,
        string userName,
        AuditAction action,
        AuditEntityType entityType,
        string entityId,
        string entityName,
        string summary,
        IEnumerable<AuditFieldChange>? changes = null,
        Guid? id = null,
        DateTimeOffset? timestamp = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("El identificador de usuario no puede estar vacío.", nameof(userId));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("El identificador de la entidad no puede estar vacío.", nameof(entityId));

        Id = id ?? Guid.NewGuid();
        UserId = userId.Trim();
        UserName = string.IsNullOrWhiteSpace(userName) ? userId.Trim() : userName.Trim();
        Action = action;
        EntityType = entityType;
        EntityId = entityId.Trim();
        EntityName = string.IsNullOrWhiteSpace(entityName) ? entityId.Trim() : entityName.Trim();
        Summary = string.IsNullOrWhiteSpace(summary) ? $"{action} en {entityType} '{EntityName}'" : summary.Trim();
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;

        if (changes != null)
        {
            Changes.AddRange(changes);
        }
    }
}
