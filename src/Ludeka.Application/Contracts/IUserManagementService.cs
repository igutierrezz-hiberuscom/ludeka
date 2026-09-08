using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

/// <summary>
/// Servicio para la administración centralizada de usuarios, roles y permisos de moderación.
/// Exclusivo para la gobernanza por parte de la Mesa Fundadora.
/// </summary>
public interface IUserManagementService
{
    Task<IReadOnlyList<AppUserDto>> GetUsersAsync(UserFilterDto? filter = null, CancellationToken ct = default);
    Task<AppUserDto?> GetUserByIdAsync(string id, CancellationToken ct = default);
    Task<AppUserDto> CreateUserAsync(CreateUserCommand command, CancellationToken ct = default);
    Task<AppUserDto> UpdateUserRoleAndPermissionsAsync(UpdateUserRoleAndPermissionsCommand command, CancellationToken ct = default);
    Task<AppUserDto> UpdateUserStatusAsync(UpdateUserStatusCommand command, CancellationToken ct = default);
}
