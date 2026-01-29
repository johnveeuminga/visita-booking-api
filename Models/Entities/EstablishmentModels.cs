using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EstablishmentCategoryEnum = visita_booking_api.Models.Enums.EstablishmentCategory;

namespace visita_booking_api.Models.Entities
{
    [Table("EstablishmentCategories")]
    public class EstablishmentCategoryEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<EstablishmentSubcategory> Subcategories { get; set; } =
            new List<EstablishmentSubcategory>();
    }

    [Table("EstablishmentSubcategories")]
    public class EstablishmentSubcategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual EstablishmentCategoryEntity Category { get; set; } = null!;
    }

    [Table("EstablishmentImages")]
    public class EstablishmentImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EstablishmentId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string S3Key { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;

        [MaxLength(255)]
        public string? Caption { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(EstablishmentId))]
        public virtual Establishment Establishment { get; set; } = null!;
    }

    [Table("EstablishmentMenuItems")]
    public class EstablishmentMenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EstablishmentId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Price { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(EstablishmentId))]
        public virtual Establishment Establishment { get; set; } = null!;
    }
}
