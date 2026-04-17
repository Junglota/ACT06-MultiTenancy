namespace ACT06_MultiTenancy.Api.Models
{
    public class Notification
    {
        public Guid Id { get; set; }

        public string TenantId { get; set; } = string.Empty;
        public Guid UserId { get; set; }

        public int? LoanId { get; set; }
        public Guid? ArticleId { get; set; }

        public string Type { get; set; } = string.Empty;
        // LoanCreated, LoanReturned, LoanDueSoon, LoanOverdue

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAtUtc { get; set; }
    }
}
