using System.ComponentModel.DataAnnotations;

namespace ProstirTgBot.Models
{
    public class InGameEventChoice
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        [Required]
        public string ChoiceName { get; set; }
        [Required]
        public string ChoiceDescription { get; set; }
        [Required]
        public int Time { get; set; }
        [Required]
        public int Energy { get; set; }
        [Required]
        public int Health { get; set; }
        [Required]
        public int Happiness { get; set; }
        [Required]
        public int Money { get; set; }
    }
}
