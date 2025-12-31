using LoanApp.Application.Dtos;
using LoanApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LoanApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LoanController : ControllerBase
{
    private readonly ILoanTypeQuery _loanTypes;

    public LoanController(ILoanTypeQuery loanTypes)
    {
        _loanTypes = loanTypes;
    }

    // GET /api/Loan/loantypes
    [HttpGet("loantypes")]
    public async Task<ActionResult<IReadOnlyList<LoanTypeDto>>> GetLoanTypes(CancellationToken cancellationToken)
    {
        var items = await _loanTypes.GetActiveLoanTypesAsync(cancellationToken);
        return Ok(items);
    }
}
