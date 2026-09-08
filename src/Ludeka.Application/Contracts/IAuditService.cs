using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

/// <summary>
/// Motor central de auditoría y trazabilidad editorial de Ludeka.
/// Registra operaciones de mutación con diff estructurado y permite su consulta a la Mesa Fundadora.
/// </summary>
public interface IAuditService
{
    Task RecordChangeAsync(RecordAuditCommand command, CancellationToken ct = default);
    Task<AuditLogPageDto> GetAuditLogsAsync(AuditLogFilterDto filter, CancellationToken ct = default);
}
