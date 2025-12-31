using LoanApp.Application.Dtos;
using LoanApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LoanApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LoanController : ControllerBase
{
    private readonly ILoanTypeQuery _loanTypes;
    private readonly ILoanEligibilityService _eligibility;

    public LoanController(ILoanTypeQuery loanTypes, ILoanEligibilityService eligibility)
    {
        _loanTypes = loanTypes;
        _eligibility = eligibility;
    }

    // GET /api/Loan/loantypes
    [HttpGet("loantypes")]
    public async Task<ActionResult<IReadOnlyList<LoanTypeDto>>> GetLoanTypes(CancellationToken cancellationToken)
    {
        var items = await _loanTypes.GetActiveLoanTypesAsync(cancellationToken);
        return Ok(items);
    }

    // POST /api/Loan/check-eligibility
    [HttpPost("check-eligibility")]
    public async Task<ActionResult<CheckLoanEligibilityResponse>> CheckEligibility(
        [FromBody] CheckLoanEligibilityRequest request, CancellationToken cancellationToken)
    {
        var result = await _eligibility.CheckEligibilityAsync(request, cancellationToken);
        return Ok(result);
    }
}
