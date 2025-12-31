using LoanApp.Application.Dtos;

namespace LoanApp.Application.Interfaces;

public interface ILoanTypeQuery
{
    Task<IReadOnlyList<LoanTypeDto>> GetActiveLoanTypesAsync(CancellationToken cancellationToken);
}
