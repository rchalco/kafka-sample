using Api.Transaction.Core.Entities;
using Api.Transaction.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Transaction.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionEntity>(e =>
        {
            e.ToTable("transactions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
            e.Property(x => x.TransactionExternalId).HasColumnName("transaction_external_id");
            e.HasIndex(x => x.TransactionExternalId).IsUnique();
            e.Property(x => x.SourceAccountId).HasColumnName("source_account_id");
            e.Property(x => x.TargetAccountId).HasColumnName("target_account_id");
            e.Property(x => x.TransferTypeId).HasColumnName("transfer_type_id");
            e.Property(x => x.Value).HasColumnName("value").HasColumnType("decimal(18,2)");
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });
    }
}
