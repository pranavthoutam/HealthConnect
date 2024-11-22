using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthConnect.Models
{
    public class MedicineAlternatives
    {
        [Key]
        public int Id { get; set; } // Single primary key

        [ForeignKey("Medicine")]
        public int MedicineId { get; set; }

        [ForeignKey("Alternative")]
        public int AlternativeId { get; set; }

        public virtual Medicine Medicine { get; set; } // Reference to the main medicine
        public virtual Medicine Alternative { get; set; } // Reference to the alternative medicine
    }
}
