using System.ComponentModel.DataAnnotations;

namespace ACT06_MultiTenancy.Api.DTos
{
    public class CreateLoanDto
    {
        [Required]
        public Guid ArticleId { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
        [MaxLength(500)]
        public string? BorrowerName { get; set; }
        [MaxLength(500)]
        public string? BorrowerEmail { get; set; }
    }
}
