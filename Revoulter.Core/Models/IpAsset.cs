using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Revoulter.Core.Models
{
    public class IpAsset
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // Optional: set default in code

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public Category Category { get; set; }

        [Required]
        [StringLength(450)]
        public string OwnerId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100.00)]
        public decimal OwnershipPercentage { get; set; } = 100.00m;

        // Additional useful fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? Hash { get; set; }
        public string? ArweaveTxId { get; set; }
        public string? StoryProtocolId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ApplicationUser Owner { get; set; } = null!;
    }

    public enum Category
    {
        Music,
        Art,
        Film,
        Fashion,
        Literature
    }
}
