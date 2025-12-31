namespace LoanApp.Application.Dtos;

public sealed record CheckLoanEligibilityRequest(
     int LoanTypeId,
     decimal RequestedAmount,
     int TenureInMonths,
     DateOnly DateOfBirth,
     string EmploymentType,
     decimal MonthlyIncome,
     decimal ExistingEmI,
     int CreditScore);

public sealed record CheckLoanEligibilityResponse(
     bool IsEligible,
     string EligibilityStatus,
     string Remarks,
     decimal? CalculatedEmI,
     decimal? EmiToIncomePct);
