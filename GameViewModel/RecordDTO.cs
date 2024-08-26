using System.ComponentModel.DataAnnotations;

namespace KFCSharedData {
    public class RecordDTO
    {
        [Required]
        public string? Name { get; set; }
        
        [Required]
        public string? Date { get; set; }

        [Required]
        public string? Picture { get; set; }

        [Required]
        public string? Address { get; set; }

        [Required]
        public long Population { get; set; }
    }
}
