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
        internal static string Reset(Models.User user, Data.ProstirTgBotContext context, )
        {
            user.Day = 0;
            user.State = Menus.Start;
            user.Day = 0;
            user.Energy = 80;
            user.Health = 80;
            user.Happiness = 80;
            context.Users.Update(user);
            context.SaveChanges();
            return "";
        }

        internal static void banckrupt(Models.User user, Data.ProstirTgBotContext context)
        {
            user.Apartment = ApartmentEnum.Family;
            context.Users.Update(user);
            context.SaveChanges();
        }

        public enum GameOverEnum
        {
            manual,
            money,
            healt,
            happiness,
        }
    }
}
