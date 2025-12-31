namespace LoanApp.Infrastructure.Data;

public sealed class LoanEligibilityRule
{
    public int RuleId { get; set; }

    public int LoanTypeId { get; set; }

    public int MinAge { get; set; }

    public int MaxAge { get; set; }

    public decimal MinMonthlyIncome { get; set; }

    public int? MinCreditScore { get; set; }

    public decimal? MaxEmiToIncomePct { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public LoanType LoanType { get; set; } = null!;
}
