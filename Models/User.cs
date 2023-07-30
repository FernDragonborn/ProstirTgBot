using ProstirTgBot.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProstirTgBot.Models
{
    public class User
    {
        public User(string username, long chatId, Menus state)
        {
            Id = Guid.NewGuid();
            Username = username;
            ChatId = chatId;
            State = Menus.Start;
            Day = 0;
            Time = 4;
            Energy = 80;
            Health = 80;
            Happiness = 80;
            Money = 100;
        }
        [Key]
        public Guid Id { get; set; }
        [Required]
        public long ChatId { get; init; }
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        private string? _inGameName;
        [Required]
        [MaxLength(50)]
        public string? InGameName
        {
            get => _inGameName;
            set
            {
                if (value.Length <= 50) _inGameName = value;
                else if (value.Length > 50) throw new OverflowException("Ім'я було задовге");
            }
        }
        [Required]
        public Menus State { get; set; }

        private int _day;
        [Required]
        public int Day
        {
            get => _day;
            set
            {
                if (value > 0 && value <= 14) _day = value;
                if (value > 14) _day = 14;
            }
        }

        private int _time;
        [Required]
        public int Time
        {
            get => _time;
            set
            {
                if (value < 0) ;
                if (value >= 0 && value <= 4) _time = value;
                if (value > 4) _time = 4;
            }
        }

        private int _energy;
        [Required]
        public int Energy
        {
            get => _energy;
            set
            {
                if (value <= 0) _energy = 0;
                if (value >= 0 && value <= 100) _energy = value;
                if (value > 100) _energy = 100;
            }
        }

        private int _health;
        [Required]
        public int Health
        {
            get => _health;
            set
            {
                if (value <= 0) _health = 0;
                if (value >= 0 && value <= 100) _health = value;
                if (value > 100) _health = 100;
            }
        }

        private int _happiness;
        [Required]
        public int Happiness
        {
            get => _happiness;
            set
            {
                if (value <= 0) _happiness = 0;
                if (value > 0 && value <= 100) _happiness = value;
                if (value > 100) _happiness = 100;
            }
        }

        [Required]
        public int Money { get; set; }
        [Required]
        public ApartmentEnum Apartment { get; set; }

        [Required]
        public int ActivitiesFound { get; set; }

        [Required]
        public bool IsSearchedForActivitiesToday { get; set; }

        [Required]
        public bool IsFormFilled { get; set; }

    }

}
