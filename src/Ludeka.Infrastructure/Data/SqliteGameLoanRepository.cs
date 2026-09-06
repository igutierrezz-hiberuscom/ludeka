using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class SqliteGameLoanRepository : IGameLoanRepository
{
    private readonly LudekaDbContext _context;

    public SqliteGameLoanRepository(LudekaDbContext context)
    {
        _context = context;
    }

    public async Task<GameLoan?> GetByIdAsync(Guid loanId, CancellationToken cancellationToken = default)
    {
        return await _context.Loans
            .Include(l => l.Game)
            .FirstOrDefaultAsync(l => l.Id == loanId, cancellationToken);
    }

    public async Task<GameLoan?> GetActiveLoanByUserAndGameAsync(string userId, Guid gameId, CancellationToken cancellationToken = default)
    {
        return await _context.Loans
            .Include(l => l.Game)
            .FirstOrDefaultAsync(l => l.UserId == userId && l.GameId == gameId && !l.IsReturned, cancellationToken);
    }

    public async Task<List<GameLoan>> GetActiveLoansByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var list = await _context.Loans
            .Include(l => l.Game)
            .Where(l => l.UserId == userId && !l.IsReturned)
            .ToListAsync(cancellationToken);

        return list.OrderByDescending(l => l.LoanDate).ToList();
    }

    public async Task<List<GameLoan>> GetLoanHistoryByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var list = await _context.Loans
            .Include(l => l.Game)
            .Where(l => l.UserId == userId)
            .ToListAsync(cancellationToken);

        return list.OrderByDescending(l => l.LoanDate).ToList();
    }

    public async Task<int> GetActiveLoansCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Loans
            .CountAsync(l => l.UserId == userId && !l.IsReturned, cancellationToken);
    }

    public async Task AddAsync(GameLoan loan, CancellationToken cancellationToken = default)
    {
        await _context.Loans.AddAsync(loan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(GameLoan loan, CancellationToken cancellationToken = default)
    {
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
