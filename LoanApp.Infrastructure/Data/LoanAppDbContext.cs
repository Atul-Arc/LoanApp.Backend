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

 OnModelCreatingPartial(modelBuilder);
 }

 partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
