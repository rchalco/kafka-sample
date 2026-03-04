using Api.Transaction.Core.Enums;

namespace Api.Transaction.Core.Entities;

public class TransactionEntity
{
    public int Id { get; set; }
    public Guid TransactionExternalId { get; set; }
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public int TransferTypeId { get; set; }
    public decimal Value { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
