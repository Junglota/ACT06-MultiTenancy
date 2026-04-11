using System.ComponentModel.DataAnnotations;

namespace ACT06_MultiTenancy.Api.DTos
{
    public class CreateLoanDto
    {
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
        public DateTime DueDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
