using System;

namespace Ludeka.Core.ValueObjects;

/// <summary>
/// Registra la modificación de un campo específico (valor previo y nuevo) para la auditoría editorial.
/// </summary>
public record AuditFieldChange
{
    public string FieldName { get; init; } = string.Empty;
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }

    // Constructor sin parámetros para deserialización EF Core / JSON
    public AuditFieldChange() { }

    public AuditFieldChange(string fieldName, string? oldValue, string? newValue)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("El nombre del campo no puede estar vacío.", nameof(fieldName));

        FieldName = fieldName.Trim();
        OldValue = oldValue;
        NewValue = newValue;
    }
}
