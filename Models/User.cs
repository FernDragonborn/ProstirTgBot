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
        public long ChatId { get; init; }
        [MaxLength(50)]
        public string Username { get; set; }
        private string? _inGameName;
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
        public Menus State { get; set; }

        private int _day;
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

        public int Money { get; set; }

        public bool IsFormFilled { get; set; }

        public ApartmentEnum Apartment { get; set; }
    }

}
