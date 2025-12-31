using System;

namespace LoanApp.Infrastructure.Data;

public partial class LoanType
{
    public int LoanTypeId { get; set; }

    public string LoanTypeName { get; set; } = null!;

    public decimal? InterestRatePct { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
