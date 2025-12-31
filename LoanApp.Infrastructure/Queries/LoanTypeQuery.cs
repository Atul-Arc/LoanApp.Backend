using LoanApp.Application.Dtos;
using LoanApp.Application.Interfaces;
using LoanApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanApp.Infrastructure.Queries;

public sealed class LoanTypeQuery : ILoanTypeQuery
{
    private readonly LoanAppDbContext _db;

    public LoanTypeQuery(LoanAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LoanTypeDto>> GetActiveLoanTypesAsync(CancellationToken cancellationToken)
    {
        return await _db.LoanTypes
        .AsNoTracking()
        .Where(x => x.IsActive)
        .OrderBy(x => x.LoanTypeName)
        .Select(x => new LoanTypeDto(x.LoanTypeId, x.LoanTypeName))
        .ToListAsync(cancellationToken);
    }
}
