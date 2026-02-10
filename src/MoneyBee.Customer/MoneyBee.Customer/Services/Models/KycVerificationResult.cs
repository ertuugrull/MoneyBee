namespace MoneyBee.Customer.Services.Models;

public class KycVerificationResult
{
    public bool IsVerified { get; set; }
    public string? RejectionReason { get; set; }
    public string? VerificationId { get; set; }
    public string? Level { get; set; }
    public int? VerificationScore { get; set; }
}
