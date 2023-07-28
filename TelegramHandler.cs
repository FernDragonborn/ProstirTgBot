using log4net;
using ProstirTgBot.Enums;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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
                        new KeyboardButton[] { },
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
                        new KeyboardButton[] { BtnSearchActivity },
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
        //private static Dictionary<long, Models.User> _usersDict = new();

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

                if (messageText == btnStart)
                {
                    user.State = Menus.GetName;
                    await SetKeyboard(chatId, _menusDic[user.State], "Напиши як тебе називати протягом гри");
                    return;
                }

                // user commands
                if (messageText == "/help" && chatId != AdminToken)
                {
                    //TODO переписати хелпу
                    await SendMessageAsync(chatId, "Тут можна надати фідбек або отримтаи допомогу. Слідуйте меню знинзу 🥰\n\nКоманди:\n/reset - скидає прогрес до першого дня");
                    return;
                }
                if (messageText == "/reset")
                {
                    GameHandler.Reset(user, context);
                    await SendMessageAsync(chatId, "Прогрес зброшений! 🤠");
                }
                // choose of menu
                switch (messageText)
                {
                    case BtnWork: user.State = Menus.Work; await SetKeyboard(chatId, _menusDic[user.State]); break;
                    case BtnWorkBarista:
                        user.Time -= 1; user.Energy -= 30; user.Mon

                    case BtnActivity: user.State = Menus.Activity; await SetKeyboard(chatId, _menusDic[user.State]); break;
                    case BtnLeisure: user.State = Menus.Lesuire; await SetKeyboard(chatId, _menusDic[user.State]); break;

                }


                context.Update(user);
                await context.SaveChangesAsync(cancellationToken);


                // якщо не пункт меню

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

        public async Task SendMessageAsync(long chatId, string messageText)
        {
            _ = await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                parseMode: ParseMode.Html,
                cancellationToken: _cancellationToken);
        }

        public async Task SetKeyboard(long chatId, ReplyKeyboardMarkup replyKeyboardMarkup, string message)
        {
            _ = await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: _cancellationToken);
        }
        public async Task SetKeyboard(long chatId, ReplyKeyboardMarkup replyKeyboardMarkup)
        {
            const string message = "";
            _ = await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: _cancellationToken);
        }

    }
}