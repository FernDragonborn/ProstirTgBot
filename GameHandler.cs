using ProstirTgBot.Enums;

namespace ProstirTgBot
{
    internal class GameHandler
    {
        /// <summary>
        /// Resets the progression
        /// </summary>
        /// <param name="user"></param>
        /// <param name="context"></param>
        /// <returns>message of GameOver to send to user</returns>
        internal static string Reset(Models.User user, Data.ProstirTgBotContext context, GameOverEnum gameOverType)
        {
            user.Day = 0;
            user.State = Menus.Start;
            user.Day = 0;
            user.Energy = 80;
            user.Health = 80;
            user.Happiness = 80;
            context.Users.Update(user);
            context.SaveChanges();
            switch (gameOverType)
            {
                case GameOverEnum.manual: return "Прогрес зброшений! 🤠";
                case GameOverEnum.happiness: return "Вас повезли в дурку, навчання відкладається на невизначений термін";
                case GameOverEnum.health: return "Вас поклали в стаціонар, навчання відкладається на невизначений термін";
                default: return "У нас технічні шоколадки, напишіть @FernDragonborn, якщо побаили це повідомлення, а ще, скоріш за все, ваш прогрес зброшено, сподіваюсь ви не далеко пройшли 😅";
            }
        }

        /// <summary>
        /// activate if money == 0
        /// </summary>
        /// <param name="user"></param>
        /// <param name="context"></param>
        /// <returns>helpMessage string for message of buncruption for player</returns>
        internal static string Banckrupt(Models.User user, Data.ProstirTgBotContext context)
        {
            user.Money = 0;
            user.Apartment = ApartmentEnum.Family;
            context.Users.Update(user);
            context.SaveChanges();
            return "Ви витратили свої останні пожитки та вас виселили. Звернутись можна було тільки до батьків, то ви так і поступили. Тепер ви живете з ними";
        }

        internal static void NextDay(Models.User user, Data.ProstirTgBotContext context, out string message)
        {
            switch (user.Apartment)
            {
                case ApartmentEnum.Family: user.Energy += 60; user.Happiness -= 10; user.Health += 5; break;
                case ApartmentEnum.Campus: user.Energy += 40; user.Happiness -= 5; user.Health -= 10; break;
                case ApartmentEnum.SmallFlat: user.Energy += 75; user.Happiness += 5; user.Health += 5; break;
                case ApartmentEnum.Coliving: user.Energy += 60; user.Happiness += 5; user.Health += 5; break;
            }

            user.Day += 1;
            user.Time = 4;
            user.IsSearchedForActivitiesToday = false;
            context.Users.Update(user);
            context.SaveChanges();
            message = $"День: {user.Day}\n{StatsToString(user)}";
        }

        internal static string StatsToString(Models.User user)
        {
            return $"Час: {user.Time}\nГроші: {user.Money}\nЕнергія: {user.Energy}\nЗдоров'я: {user.Health}\nЩастя: {user.Happiness}";
        }

        public enum GameOverEnum
        {
            manual,
            health,
            happiness,
        }
    }
}
