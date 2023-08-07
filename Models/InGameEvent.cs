using ProstirTgBot.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProstirTgBot.Models
{
    public class InGameEvent
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        public string? EventName { get; set; }
        [Required]
        public string EventDescription { get; set; }
        [Required]
        public int Day { get; set; }
        [Required]
        public int Time { get; set; }
        [Required]
        public ApartmentEnum Apartment { get; set; }
        [Required]
        public List<InGameEventChoice> inGameEventChoices { get; set; }
        [Required]
        public int DependsOnChoice { get; set; }
    }
}
