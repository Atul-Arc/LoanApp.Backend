using LoanApp.Application.Dtos;
using LoanApp.Application.Interfaces;
using LoanApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanApp.Infrastructure.Services;

public sealed class LoanEligibilityService : ILoanEligibilityService
{
    private readonly LoanAppDbContext _db;

    public LoanEligibilityService(LoanAppDbContext db)
    {
        _db = db;
    }

    public async Task<CheckLoanEligibilityResponse> CheckEligibilityAsync(
    CheckLoanEligibilityRequest request,
    CancellationToken cancellationToken)
    {
        if (request.TenureInMonths <= 0)
        {
            return NotEligible("Tenure must be greater than 0");
        }

        if (request.MonthlyIncome <= 0)
        {
            return NotEligible("Monthly income must be greater than 0");
        }

        if (request.RequestedAmount <= 0)
        {
            return NotEligible("Requested amount must be greater than 0");
        }

        if (string.IsNullOrWhiteSpace(request.EmploymentType))
        {
            return NotEligible("Employment type is required");
        }

        var loanType = await _db.LoanTypes
            .AsNoTracking()
            .Where(x => x.LoanTypeId == request.LoanTypeId && x.IsActive)
            .Select(x => new { x.LoanTypeId, x.LoanTypeName, x.InterestRatePct })
            .SingleOrDefaultAsync(cancellationToken);

        if (loanType is null)
        {
            return NotEligible("Invalid or inactive loan type");
        }

        if (!EmploymentTypeMatchesLoanTypeName(request.EmploymentType, loanType.LoanTypeName))
        {
            return NotEligible("Employment type does not match selected loan type");
        }

        var rule = await _db.LoanEligibilityRules
            .AsNoTracking()
            .Where(x => x.LoanTypeId == request.LoanTypeId && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (rule is null)
        {
            return NotEligible("No eligibility rules configured for selected loan type");
        }

        var age = CalculateAge(request.DateOfBirth, DateOnly.FromDateTime(DateTime.UtcNow));
        if (age < rule.MinAge || age > rule.MaxAge)
        {
            return NotEligible($"Age must be between {rule.MinAge} and {rule.MaxAge}");
        }

        if (request.MonthlyIncome < rule.MinMonthlyIncome)
        {
            return NotEligible("Monthly income below minimum required threshold");
        }

        if (rule.MinCreditScore is int minCreditScore && request.CreditScore < minCreditScore)
        {
            return NotEligible("Credit score below minimum required threshold");
        }

        // EMI calculation uses loan type's interest rate if present; otherwise falls back to 0%.
        var annualRatePct = loanType.InterestRatePct ?? 0m;
        var calculatedEmi = CalculateEmi(request.RequestedAmount, annualRatePct, request.TenureInMonths);

        var totalEmi = request.ExistingEmI + calculatedEmi;
        var emiToIncomePct = (totalEmi / request.MonthlyIncome) * 100m;

        if (rule.MaxEmiToIncomePct is decimal maxRatio && emiToIncomePct > maxRatio)
        {
            return NotEligible("EMI to income ratio exceeds allowed threshold");
        }

        return new CheckLoanEligibilityResponse(
        IsEligible: true,
        EligibilityStatus: "Eligible",
        Remarks: "Meets age, income, credit score and EMI ratio criteria",
        CalculatedEmI: decimal.Round(calculatedEmi, 2),
        EmiToIncomePct: decimal.Round(emiToIncomePct, 2));
    }

    private static bool EmploymentTypeMatchesLoanTypeName(string employmentType, string loanTypeName)
    {
        // Convention: LoanTypeName contains suffix like "(Salaried)" or "(Self Employed)" and may have leading/trailing spaces.
        var normalizedEmploymentType = employmentType.Trim();

        if (loanTypeName.Contains("(Salaried)", StringComparison.OrdinalIgnoreCase))
        {
            return normalizedEmploymentType.Equals("Salaried", StringComparison.OrdinalIgnoreCase);
        }

        if (loanTypeName.Contains("(Self Employed)", StringComparison.OrdinalIgnoreCase) ||
        loanTypeName.Contains("(Self-Employed)", StringComparison.OrdinalIgnoreCase))
        {
            return normalizedEmploymentType.Equals("Self Employed", StringComparison.OrdinalIgnoreCase)
            || normalizedEmploymentType.Equals("Self-Employed", StringComparison.OrdinalIgnoreCase);
        }

        // If not encoded in the name, do not block eligibility.
        return true;
    }

    private static CheckLoanEligibilityResponse NotEligible(string remarks) =>
        new(IsEligible: false,
        EligibilityStatus: "Not Eligible",
        Remarks: remarks,
        CalculatedEmI: null,
        EmiToIncomePct: null);

    private static int CalculateAge(DateOnly dob, DateOnly today)
    {
        var age = today.Year - dob.Year;
        if (today < dob.AddYears(age)) age--;
        return age;
    }

    private static decimal CalculateEmi(decimal principal, decimal annualRatePct, int tenureMonths)
    {
        // Standard EMI formula: P * r * (1+r)^n / ((1+r)^n -1)
        // where r is monthly interest rate.
        var r = annualRatePct / 12m / 100m;
        var n = tenureMonths;

        if (r == 0m)
        {
            return principal / n;
        }

        // Use double for power calculation; convert back to decimal.
        var onePlusR = 1d + (double)r;
        var pow = Math.Pow(onePlusR, n);
        var emi = (double)principal * (double)r * pow / (pow - 1d);
        return (decimal)emi;
    }
}
