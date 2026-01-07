namespace Core.Entities;

public class Payment : BaseEntity
{
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
}

public enum PaymentMethod
{
    CreditCard = 0,
    BankTransfer = 1,
    PayPal = 2,
    Stripe = 3
}

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}