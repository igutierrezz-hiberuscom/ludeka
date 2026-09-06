using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeca.Core.Entities;

namespace Ludeca.Application.Contracts;

public interface IGameLoanRepository
{
    Task<GameLoan?> GetByIdAsync(Guid loanId, CancellationToken cancellationToken = default);
    Task<GameLoan?> GetActiveLoanByUserAndGameAsync(string userId, Guid gameId, CancellationToken cancellationToken = default);
    Task<List<GameLoan>> GetActiveLoansByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<GameLoan>> GetLoanHistoryByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetActiveLoansCountAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(GameLoan loan, CancellationToken cancellationToken = default);
    Task UpdateAsync(GameLoan loan, CancellationToken cancellationToken = default);
}
