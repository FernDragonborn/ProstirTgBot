using log4net;
using ProstirTgBot.Enums;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static ProstirTgBot.GameHandler;
using static ProstirTgBot.TelegramButtons;

namespace ProstirTgBot
{
    internal class TelegramHandler
    {
        #region Initializtion fields

        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static readonly string Token = DotNetEnv.Env.GetString("TG_TOKEN");
        private static readonly long AdminToken = Convert.ToInt64(DotNetEnv.Env.GetString("ADMIN_TOKEN"));
        private static Dictionary<Menus, ReplyKeyboardMarkup> _menusDic = new()
        {
            {

                Menus.Start,
                new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { btnStart },
                    })
                    { ResizeKeyboard = true }
            },
            {

                Menus.GetName,
                new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { },
                    })
                    { ResizeKeyboard = true }
            },
            {
                Menus.Day,
                new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { BtnWork, BtnLeisure },
                        new KeyboardButton[] { BtnActivity },
                    })
                    { ResizeKeyboard = true }
            },
            {
                Menus.Work,
                new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { BtnWorkBarista, BtnWorkTutor },
                        new KeyboardButton[] { BtnWorkFreelance },
                    })
                    { ResizeKeyboard = true }
            },
            {
                Menus.Lesuire,
                new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { BtnLeisureLake, BtnLeisureGym },
                        new KeyboardButton[] { BtnLeisureFriend },
                    })
                    { ResizeKeyboard = true }
            },
            {
                //TODO add extensiobn of keyboard
                Menus.Activity,
                new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton(BtnActivitySearch),
                    })
                    { ResizeKeyboard = true }
            },
            {
                //TODO add extension of keyboard
                Menus.Relocation,
                new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { BtnRelocationFlat, BtnRelocationBigFlat },
                        new KeyboardButton[] { BtnRelocationCampus, BtnRelocationFamily },
                    })
                    { ResizeKeyboard = true }
            }
        };

        private const string helpMessage = "Тут можна надати фідбек або отримтаи допомогу. Слідуйте меню знинзу 🥰\n\nКоманди:\n/reset - скидає прогрес до першого дня";

        private readonly TelegramBotClient _botClient = new(Token);
        public Func<ITelegramBotClient, Exception, CancellationToken, Task> HandlePollingErrorAsync { get; private set; } = null!;
        public Func<ITelegramBotClient, Update, CancellationToken, Task> HandleUpdateAsync { get; private set; }
        readonly ReceiverOptions _receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };
        readonly CancellationTokenSource _cts = new();
        private readonly CancellationToken _cancellationToken;
        #endregion

        internal async Task Init(Data.ProstirTgBotContext context)
        {
            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: _receiverOptions,
                cancellationToken: _cts.Token
            );

            var me = await _botClient.GetMeAsync(cancellationToken: _cancellationToken);
            Log.Info($"Start listening for @{me.Username}");

            //LoadUsers(context);

            await SendMessageAsync(Convert.ToInt64(AdminToken), $"bot initialized\n{DateTime.Now}");

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                #region 
                // Only process Message updates: https://core.telegram.org/bots/api#message
                if (update.Message is not { } message)
                    return;
                // Only process text messages
                if (message.Text is not { } messageText)
                    return;
                if (message.From is null || message.From.Username is null)
                    return;

                long chatId = message.Chat.Id;

                Log.Info($"[TG]: In chat {chatId} received: {messageText}");
                #endregion

                var user = context.Users.FirstOrDefault(x => x.ChatId == chatId);

                // if user not created
                if (user == null)
                {
                    user = new Models.User(message.From.Username, chatId, Menus.Start);
                    context.Users.Add(user);
                    await context.SaveChangesAsync(cancellationToken);
                    await SetKeyboard(chatId, _menusDic[user.State], "Я не знайшов ваш акаунт у існуючих користувачах, тому створив новий 😊\n\nВи напевне прийшли пограти? Настискайте на меню знизу, там все ваше управління 👇");
                    return;
                }

                //TODO add events
                if (user.Day == 0)
                {

                }
                if (user.Apartment == ApartmentEnum.Family)
                    switch (user.Day)
                    {
                        case 0: break;
                    }


                // choose of menu of commands
                switch (messageText)
                {
                    case "/help": await SendMessageAsync(chatId, helpMessage); return;
                    case "/reset":
                        {
                            Reset(user, context, GameOverEnum.manual);
                            await SendMessageAsync(chatId, "Прогрес зброшений! 🤠");
                            return;
                        }
                    case btnStart:
                        {
                            user.State = Menus.GetName;
                            await SetKeyboard(chatId, _menusDic[user.State], "Напиши як тебе називати протягом гри");
                            return;
                        }

                    case BtnWork: user.State = Menus.Work; await SetKeyboard(chatId, _menusDic[user.State], "Доступні роботи:"); return;
                    case BtnWorkBarista:
                        if (user.Time < 2) await NotEnoughTime(user);
                        user.Time -= 2; user.Money += 20; user.Energy -= 30; user.Happiness -= 7; user.Health -= 5;
                        user.State = Menus.Day; await SendGameUpdate(user); return;
                    case BtnWorkTutor:
                        if (user.Time < 2) await NotEnoughTime(user);
                        user.Time -= 2; user.Money += 25; user.Energy -= 30; user.Happiness -= 12; user.Health -= 0;
                        user.State = Menus.Day; await SendGameUpdate(user); return;
                    case BtnWorkFreelance:
                        if (user.Time < 2) await NotEnoughTime(user);
                        user.Time -= 2; user.Money += 15; user.Energy -= 20; user.Happiness -= 5; user.Health -= 0;
                        user.State = Menus.Day; await SendGameUpdate(user); return;

                    case BtnLeisure: user.State = Menus.Lesuire; await SetKeyboard(chatId, _menusDic[user.State], "Достпний відпочинок: "); return;
                    case BtnLeisureLake:
                        user.Time -= 1; user.Money -= 0; user.Energy -= 5; user.Happiness += 10; user.Health += 5;
                        user.State = Menus.Day; await SendGameUpdate(user); return;
                    case BtnLeisureGym:
                        user.Time -= 1; user.Money -= 5; user.Energy -= 10; user.Happiness += 15; user.Health += 0;
                        user.State = Menus.Day; await SendGameUpdate(user); return;
                    case BtnLeisureFriend:
                        user.Time -= 1; user.Money -= 10; user.Energy -= 20; user.Happiness += 10; user.Health += 20;
                        user.State = Menus.Day; await SendGameUpdate(user); return;

                    case BtnActivity: user.State = Menus.Activity; await SetKeyboard(chatId, _menusDic[user.State], "Доступні активності:"); return;
                    case BtnActivitySearch:
                        {
                            string text = AddActivityButton(user);
                            await SendMessageAsync(chatId, text);
                            return;
                        }
                    case BtnActivityVolunteering:
                        user.Time -= 2; user.Money -= 0; user.Energy -= 25; user.Happiness += 25; user.Health += 0;
                        user.State = Menus.Day; await SendGameUpdate(user); return;
                    case BtnActivityFillInForm:
                        user.IsFormFilled = true; user.Energy -= 5;
                        user.State = Menus.Day; await SendMessageAsync(chatId, "Ви заповнили форму та буквально через годину вам відповіли, що ви підходите! Тепер ви можете переїхати на Д'Іскру!");
                        return;
                }

                context.Update(user);
                await context.SaveChangesAsync(cancellationToken);
                //if (user.State != Menus.Start || user.State != Menus.GetName) await SendMessageAsync(chatId, StatsToString(user));

                //check stats
                if (user.Money < 0)
                {
                    string text = banckrupt(user, context);
                    await SendMessageAsync(chatId, text);
                }
                if (user.Health == 0)
                {
                    string text = Reset(user, context, GameOverEnum.health);
                    await SendMessageAsync(chatId, text);
                }
                if (user.Happiness == 0)
                {
                    string text = Reset(user, context, GameOverEnum.happiness);
                    await SendMessageAsync(chatId, text);
                }
                if (user.Energy == 0)
                {
                    //it's not a mistake
                    nextDay(user, context, out string a);
                    nextDay(user, context, out string updateText);
                    user.Health -= 15;
                    await SendMessageAsync(chatId, "Ви проспали увесь день, така сильна втома вплинула на ваше самопочуття (-15 здоров'я). Але тепер ви не валитесь з ніг");
                    await SendMessageAsync(chatId, updateText);
                }

                if (user.Time == 0)
                {
                    nextDay(user, context, out string updateText);
                    await SendMessageAsync(chatId, updateText);
                }

                // якщо був використаний не визначений стан
                if (!(Enum.IsDefined(user.State)))
                {
                    await SendMessageAsync(chatId, "Виникла внутрішня помилка. Спробуйте обрати пункт із меню, якщо помилка не пропаде, то зверністья у підтримку 😢");
                    Log.Error($"Помилка: {chatId} не мав визначеного user.State та відправив повідомлення із текстом:\n{messageText}");
                    return;
                }
                // Get name
                if (user.State == Menus.GetName && messageText != btnStart)
                {
                    user.InGameName = messageText.Normalize().Trim();
                    if (user.InGameName.Length < 2 || !(Regex.IsMatch(user.InGameName, @"^[A-Za-zА-Яа-я][\p{L}\s]{1,19}$")))
                    {
                        await SendMessageAsync(chatId, "Перевірте чи ім'я не менше 2х символів, та чи воно складається із букв");
                        return;
                    }

                    user.State = Menus.Day;
                    context.Users.Update(user);
                    await context.SaveChangesAsync(cancellationToken);
                    await SendMessageAsync(chatId, $"Тепер тебе звати {user.InGameName}!");
                    await SetKeyboard(chatId, _menusDic[user.State], $"Знизу з'явилось ігрове меню, спробуй ним скористатись\n\nА ще ось твої характеристики: {StatsToString(user)}");
                    return;
                }
                else
                {
                    //if (chatId == AdminToken) { return; }
                    //await SendMessageAsync(ADMIN_TOKEN, CreateRequestMessage(usersDict[chatId], message, usersDict[chatId].State));
                    //await SendMessageAsync(chatId, "Дякую за звернення, я передав ваше повідомлення в гуманітарний штаб 😊");
                    user.State = Menus.GetName;
                    await SendMessageAsync(chatId, "Якась помилка, будь ласка, знову оберіть пункт меню та повторіть запит 😥");
                }
                // admin commands
                if (chatId == AdminToken)
                {
                    if (messageText == "/help")
                    {
                        await SendMessageAsync(chatId, "Список команд поки пустий");
                    }
                    else
                    {
                        //await SendMessageAsync(chatId, "Не вдалося опрацювати команду, перевірте чи нема помилок");
                    }

                }


            }
            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var errorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Log.Error(errorMessage);
                return Task.CompletedTask;
            }
        }
        /// <summary>
        /// If keyboard is not full adds helpMessage button to new activity
        /// </summary>
        /// <param name="user"></param>
        /// <returns>string with text about result of searching, needs to be sended to the player</returns>
        private static string AddActivityButton(Models.User user)
        {
            ReplyKeyboardMarkup keyboard = _menusDic[Menus.Activity];
            user.Time -= 2;
            user.Money -= 0;
            user.Energy -= 40;
            user.Happiness -= 10;
            user.Health += 0;

            //if keyboard does not have BtnActivityVolunteering button add it and return
            if (!(keyboard.Keyboard.SelectMany(row => row)
                        .Any(button => button.Text == BtnActivityVolunteering)))
            {
                _menusDic[Menus.Activity] = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton(BtnActivitySearch),
                    new KeyboardButton(BtnActivityVolunteering )
                });
                return "Ви знайшли волонтерську групу, можете српобувати доєднатись до них наступного заходу!";
            }
            //if keyboard does not have BtnActivityFillInForm button add it and return
            else if (!(keyboard.Keyboard.SelectMany(row => row)
                    .Any(button => button.Text == BtnActivityFillInForm)))
            {
                _menusDic[Menus.Activity] = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton(BtnActivitySearch),
                    new KeyboardButton(BtnActivityVolunteering ),
                    new KeyboardButton(BtnActivityFillInForm )
                });
                return "Ви знайшли колівінг Д'Іскра. Ви можете заповнити гугл-форму та рпойти співбесіду, щоб переїхати до них";
            }
            return "Ви не знайшли нових тусовок";
        }

        internal async Task NotEnoughTime(Models.User user)
        {
            await SendMessageAsync(user.ChatId, "Ви не можете це зробити, сьогодні на це недостатньо часу. Спробуйте завтра");
        }

        private async Task SendGameUpdate(Models.User user)
        {
            await SendMessageAsync(user.ChatId, StatsToString(user));
        }

        private async Task SendMessageAsync(long chatId, string messageText)
        {
            _ = await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                parseMode: ParseMode.Html,
                cancellationToken: _cancellationToken);
        }

        private async Task SetKeyboard(long chatId, ReplyKeyboardMarkup replyKeyboardMarkup, string message)
        {
            _ = await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: _cancellationToken);
        }

    }
}