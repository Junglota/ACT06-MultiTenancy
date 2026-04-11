using ACT06_MultiTenancy.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace ACT06_MultiTenancy.Api.Models
{
    public class Loan
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenantId { get; set; } = string.Empty;

        [Required]
        public Guid ArticleId { get; set; }

        [Required]
        [MaxLength(150)]
        public string BorrowerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string BorrowerEmail { get; set; } = string.Empty;

        [Required]
        public DateTime LoanDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? ReturnedAt { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Active"; // Active, Returned, Overdue

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Articulo? Article { get; set; }
    }
}
