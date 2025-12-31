using Microsoft.EntityFrameworkCore;

namespace LoanApp.Infrastructure.Data;

public partial class LoanAppDbContext : DbContext
{
    public LoanAppDbContext()
    {
    }

    public LoanAppDbContext(DbContextOptions<LoanAppDbContext> options)
    : base(options)
    {
    }

    public virtual DbSet<LoanType> LoanTypes { get; set; }
    public virtual DbSet<LoanEligibilityRule> LoanEligibilityRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoanType>(entity =>
        {
            entity.HasKey(e => e.LoanTypeId);

            entity.ToTable("LoanType");

            entity.Property(e => e.LoanTypeName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.InterestRatePct)
                .HasColumnType("decimal(5,2)");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<LoanEligibilityRule>(entity =>
        {
            entity.HasKey(e => e.RuleId);

            entity.ToTable("LoanEligibilityRule");

            entity.Property(e => e.MinMonthlyIncome)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.MaxEmiToIncomePct)
                .HasColumnType("decimal(5,2)");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(e => e.LoanType)
                .WithMany()
                .HasForeignKey(e => e.LoanTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoanEligibilityRule_LoanType");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
