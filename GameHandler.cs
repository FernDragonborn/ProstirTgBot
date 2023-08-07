using ProstirTgBot.Data;
using ProstirTgBot.Enums;
using ProstirTgBot.Models;
using Telegram.Bot.Types.ReplyMarkups;
using static ProstirTgBot.TelegramButtons;

namespace ProstirTgBot
{
    internal partial class GameHandler
    {
        /// <summary>
        /// Resets the progression
        /// </summary>
        /// <param name="player"></param>
        /// <param name="context"></param>
        /// <param name="gameOverType"></param>
        /// <returns>message of GameOver to send to player</returns>
        internal static string Reset(Player player, ProstirTgBotContext context, GameOverEnum gameOverType)
        {
            player.State = Menus.Start;
            player.InGameName = "";
            player.Day = 0;
            player.Time = 4;
            player.Energy = 80;
            player.Health = 80;
            player.Happiness = 80;
            player.Money = 100;
            player.Apartment = ApartmentEnum.Family;
            player.ActivitiesFound = 0;
            player.IsFormFilled = false;
            player.IsLivedInCampus = false;
            player.IsLivedWithFamily = false;
            player.IsSearchedForActivitiesToday = false;
            context.Players.Update(player);
            context.SaveChanges();
            switch (gameOverType)
            {
                case GameOverEnum.Manual: return "Прогрес зброшений! 🤠";
                case GameOverEnum.Happiness: return "Вас повезли в дурку, навчання відкладається на невизначений термін";
                case GameOverEnum.Health: return "Вас поклали в стаціонар, навчання відкладається на невизначений термін";
                default: return "У нас технічні шоколадки, напишіть @FernDragonborn, якщо побаили це повідомлення, а ще, скоріш за все, ваш прогрес зброшено, сподіваюсь ви не далеко пройшли 😅";
            }
        }

        internal static void NextDay(Player player, ProstirTgBotContext context, out string message)
        {
            switch (player.Apartment)
            {
                case ApartmentEnum.Family: player.Energy += 60; player.Happiness -= 10; player.Health += 5; break;
                case ApartmentEnum.Campus: player.Energy += 40; player.Happiness -= 5; player.Health -= 10; break;
                case ApartmentEnum.SmallFlat: player.Energy += 75; player.Happiness += 5; player.Health += 5; break;
                case ApartmentEnum.Coliving: player.Energy += 60; player.Happiness += 5; player.Health += 5; break;
            }

            player.Day += 1;
            player.Time = 4;
            player.IsSearchedForActivitiesToday = false;
            context.Players.Update(player);
            context.SaveChanges();
            message = $"Новий день! {player.Day}й\n\n{StatsToString(player)}";
        }

        internal static string StatsToString(Player player)
        {
            return $"Час: {player.Time}\nГроші: {player.Money}\nЕнергія: {player.Energy}\nЗдоров'я: {player.Health}\nЩастя: {player.Happiness}";
        }

        /// <summary>
        /// If relocationKeyboard is not full adds helpMessage button to new activity
        /// </summary>
        /// <param name="player"></param>
        /// <param name="menusDic"></param>
        /// <returns>string with text about result of searching, needs to be sended to the player</returns>
        internal static string AddActivityButton(Player player, Dictionary<Menus, ReplyKeyboardMarkup> menusDic)
        {
            player.Time -= 2;
            player.Money -= 0;
            player.Energy -= 30;
            player.Happiness -= 10;
            player.Health += 0;

            if (player.IsSearchedForActivitiesToday) return "Поки нічого нового, спробуйте завтра";
            if (player.Day <= 3) return "Поки ви навіть не встигли ні з ким познайомитись, спробуйте через пару днів";

            //if relocationKeyboard does not have BtnActivityVolunteering button add it and return
            if (player.ActivitiesFound == 1)
            {
                menusDic[Menus.Activity] = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton(BtnActivitySearch),
                    new KeyboardButton(BtnActivityVolunteering )
                });
                return "Ви знайшли волонтерську групу, можете спробувати доєднатись до них наступного заходу!";
            }
            //if relocationKeyboard does not have BtnActivityFillInForm button add it and return
            if (player.ActivitiesFound == 2)
            {
                menusDic[Menus.Activity] = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton(BtnActivitySearch),
                    new KeyboardButton(BtnActivityVolunteering ),
                    new KeyboardButton(BtnActivityFillInForm )
                });
                return "Ви знайшли колівінг Д'Іскра. Ви можете заповнити гугл-форму та рпойти співбесіду, щоб переїхати до них";
            }
            return "Ви не знайшли нових тусовок";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="eventStringList"></param>
        /// <param name="context"></param>
        /// <param name="keyboard"></param>
        /// <param name="inGameEventRef"></param>
        /// <returns>True if event exsits and ref _eventKeyboard for event</returns>
        internal static bool TryCheckForEvents(Player player, List<string> eventStringList, ProstirTgBotContext context, ref ReplyKeyboardMarkup keyboard, ref InGameEvent inGameEventRef)
        {
            InGameEvent inGameEvent = context.InGameEvents.FirstOrDefault(x => x.Day == player.Day && x.Apartment == player.Apartment);
            if (inGameEvent == null) return false;

            bool isChoiceNeeded = inGameEvent.DependsOnChoice != -1;
            bool isChoiceNotChosen = !player.ChosenChoices.Contains(inGameEvent.DependsOnChoice);
            //if is inverted
            if (isChoiceNeeded && isChoiceNotChosen) return false;

            var a = context.InGameEventChoice.Where(x => x.InGameEventId == inGameEvent.Id).ToList();
            //a.ForEach(x => inGameEvent.inGameEventChoices.Add(x));
            inGameEventRef = inGameEvent;

            player.State = Menus.Event;
            context.Update(player);
            context.SaveChanges();

            inGameEvent.inGameEventChoices.ForEach(x => eventStringList.Add(x.ChoiceName));
            List<KeyboardButton> eventButtonsList = new();
            inGameEvent.inGameEventChoices.ForEach(x => eventButtonsList.Add(x.ChoiceName));
            keyboard = new ReplyKeyboardMarkup(eventButtonsList);
            return true;
        }

        /// <summary>
        /// activate if money == 0
        /// </summary>
        /// <param name="player"></param>
        /// <param name="context"></param>
        /// <returns>helpMessage string for message of buncruption for player</returns>
        internal static string Banckrupt(Player player, ProstirTgBotContext context)
        {
            player.Money = 0;
            player.Apartment = ApartmentEnum.Family;
            context.Players.Update(player);
            context.SaveChanges();
            return "Ви витратили свої останні пожитки та вас виселили. Звернутись можна було тільки до батьків, то ви так і поступили. Тепер ви живете з ними";
        }
    }
}
