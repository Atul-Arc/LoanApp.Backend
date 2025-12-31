using LoanApp.Application.Dtos;

namespace LoanApp.Application.Interfaces;

public interface ILoanEligibilityService
{
    Task<CheckLoanEligibilityResponse> CheckEligibilityAsync(
    CheckLoanEligibilityRequest request,
    CancellationToken cancellationToken);
}
