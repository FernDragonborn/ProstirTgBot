using log4net;
using ProstirTgBot.Data;
using ProstirTgBot.Enums;
using ProstirTgBot.Models;
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
        //TODO Could be bug if try to pass Menus.Event
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
                //TODO add extensiobn of relocationKeyboard
                Menus.Activity,
                new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton(BtnActivitySearch),
                    })
                    { ResizeKeyboard = true }
            },
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
        private List<string> _eventStringList = new();
        private InGameEvent _inGameEvent = new();
        private ReplyKeyboardMarkup _eventKeyboard = new(new KeyboardButton(""));
        #endregion

        internal async Task Init(ProstirTgBotContext context)
        {

            #region Start receving messages. Send message to admin and log initialization of bot 
            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: _receiverOptions,
                cancellationToken: _cts.Token
            );

            var me = await _botClient.GetMeAsync(cancellationToken: _cancellationToken);
            Log.Info($"---- Start listening for @{me.Username} ----");

            await SendMessageAsync(Convert.ToInt64(AdminToken), $"bot initialized\n{DateTime.Now}");
            #endregion

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                #region Check if messagee is null

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

                //TODO скидывается менюшка с активностями при перезапуске
                var player = context.Players.FirstOrDefault(x => x.ChatId == chatId);

                // if player account not created
                if (player == null)
                {
                    player = new Models.Player(message.From.Username, chatId, Menus.Start);
                    context.Players.Add(player);
                    await context.SaveChangesAsync(cancellationToken);
                    await SetKeyboardAsync(chatId, _menusDic[player.State],
                        "Я не знайшов ваш акаунт у існуючих користувачах, тому створив новий 😊\n\nВи напевне прийшли пограти? Настискайте на меню знизу, там все ваше управління 👇");
                    return;
                }

                //ONLY commands
                switch (messageText)
                {
                    case "/help":
                        await SendMessageAsync(chatId, helpMessage);
                        return;
                    case "/reset":
                        {
                            Reset(player, context, GameOverEnum.Manual);
                            await SetKeyboardAsync(chatId, _menusDic[player.State], "Прогрес зброшений! 🤠");
                            return;
                        }
                }

                if (player is { Day: 0 or 7 or 14, State: Menus.Day or Menus.Relocation, Time: 4 })
                {
                    if (player.State == Menus.Day) await SetMoveOutMenuAndCheckWhereLivedAsync(player);

                    if (_relocationStringList.FirstOrDefault(x => x == messageText) != null)
                    {
                        player.Apartment = messageText switch
                        {
                            BtnRelocationColiving => ApartmentEnum.Coliving,
                            BtnRelocationFlat => ApartmentEnum.SmallFlat,
                            BtnRelocationFamily => ApartmentEnum.Family,
                            BtnRelocationCampus => ApartmentEnum.Campus,
                            _ => throw new NotImplementedException(
                                "ApartmentEnum or _relocationButtonList contains something that not implemented in SetMoveOutMenuAndCheckWhereLivedAsync")
                        };
                        //TODO Fix return in method
                        await ApplyMovingInEffect(player);
                        player.State = Menus.Day;
                        await GameUpdateAndSetMenuAsync(player, context);
                    }
                }

                //TODO add events to DB 
                if (player.State == Menus.Event)
                {
                    InGameEventChoice? choice = context.InGameEventChoice.FirstOrDefault(x => x.ChoiceName == messageText);
                    if (choice == null) return;
                    await ApplyEventEffect(player, choice);
                    await GameUpdateAndSetMenuAsync(player, context);
                    return;
                }
                bool isFinished = TryCheckForEvents(player, _eventStringList, context, ref _eventKeyboard, ref _inGameEvent);
                if (isFinished) { await SetKeyboardAsync(player.ChatId, _eventKeyboard, $"Треба прийняти рішення!\n\n{_inGameEvent.EventDescription}"); return; }

                // choose of menu 
                switch (messageText)
                {
                    case btnStart:
                        {
                            player.State = Menus.GetName;
                            await SetKeyboardAsync(chatId, _menusDic[player.State], "Напиши як тебе називати протягом гри");
                            return;
                        }

                    case BtnWork: player.State = Menus.Work; await SetKeyboardAsync(chatId, _menusDic[player.State], "Доступні роботи:"); return;
                    case BtnWorkBarista:
                        if (player.Time < 2) await SendMessageNotEnoughTimeAsync(player);
                        player.Time -= 2; player.Money += 20; player.Energy -= 30; player.Happiness -= 7; player.Health -= 5;
                        player.State = Menus.Day; await GameUpdateAndSetMenuAsync(player, context); return;
                    case BtnWorkTutor:
                        if (player.Time < 2) await SendMessageNotEnoughTimeAsync(player);
                        player.Time -= 2; player.Money += 25; player.Energy -= 30; player.Happiness -= 12; player.Health -= 0;
                        player.State = Menus.Day; await GameUpdateAndSetMenuAsync(player, context); return;
                    case BtnWorkFreelance:
                        if (player.Time < 2) await SendMessageNotEnoughTimeAsync(player);
                        player.Time -= 2; player.Money += 15; player.Energy -= 20; player.Happiness -= 5; player.Health -= 0;
                        player.State = Menus.Day; await GameUpdateAndSetMenuAsync(player, context); return;

                    case BtnLeisure: player.State = Menus.Lesuire; await SetKeyboardAsync(chatId, _menusDic[player.State], "Доступний відпочинок: "); return;
                    case BtnLeisureLake:
                        player.Time -= 1; player.Money -= 0; player.Energy -= 5; player.Happiness += 10; player.Health += 5;
                        player.State = Menus.Day; await GameUpdateAndSetMenuAsync(player, context); return;
                    case BtnLeisureGym:
                        player.Time -= 1; player.Money -= 5; player.Energy -= 10; player.Happiness += 15; player.Health += 15;
                        player.State = Menus.Day; await GameUpdateAndSetMenuAsync(player, context); return;
                    case BtnLeisureFriend:
                        player.Time -= 1; player.Money -= 10; player.Energy -= 20; player.Happiness += 10; player.Health += 20;
                        player.State = Menus.Day; await GameUpdateAndSetMenuAsync(player, context); return;

                    case BtnActivity: player.State = Menus.Activity; await SetKeyboardAsync(chatId, _menusDic[player.State], "Доступні активності:"); return;
                    case BtnActivitySearch:
                        {
                            if (player.Time < 2) { await SendMessageNotEnoughTimeAsync(player); return; }
                            player.State = Menus.Day;
                            player.ActivitiesFound += 1;
                            string text = AddActivityButton(player, _menusDic);
                            await SendMessageAsync(chatId, text);
                            await GameUpdateAndSetMenuAsync(player, context);
                            return;
                        }
                    case BtnActivityVolunteering:
                        player.Time -= 2; player.Money -= 0; player.Energy -= 25; player.Happiness += 25; player.Health += 0;
                        player.State = Menus.Day; await GameUpdateAndSetMenuAsync(player, context); return;
                    case BtnActivityFillInForm:
                        player.IsFormFilled = true; player.Energy -= 5;
                        player.State = Menus.Day; await SendMessageAsync(chatId, "Ви заповнили форму та буквально через годину вам відповіли, що ви підходите! Тепер ви можете переїхати на Д'Іскру!");
                        return;
                }

                // Get name
                if (player.State == Menus.GetName && messageText != btnStart)
                {
                    player.InGameName = messageText.Normalize().Trim();
                    if (player.InGameName.Length < 2 || !(Regex.IsMatch(player.InGameName, @"^[A-Za-zА-Яа-я][\p{L}\s]{1,19}$")))
                    {
                        await SendMessageAsync(chatId, "Перевірте чи ім'я не менше 2х символів, та чи воно складається із букв");
                        return;
                    }

                    player.State = Menus.Day;
                    context.Players.Update(player);
                    await context.SaveChangesAsync(cancellationToken);
                    await SendMessageAsync(chatId, $"Тепер тебе звати {player.InGameName}!");
                    //TODO rewrite message
                    await SetMoveOutMenuAndCheckWhereLivedAsync(player);
                    await SendMessageAsync(chatId, $"А ще ось твої характеристики:\n{StatsToString(player)}");
                    return;
                }

                #region error handling and admin commands
                //TODO fix and remake entirely
                // якщо був використаний не визначений стан
                if (!(Enum.IsDefined(player.State)))
                {
                    await SendMessageAsync(chatId, "Виникла внутрішня помилка. Спробуйте обрати пункт із меню, якщо помилка не пропаде, то зверністья у підтримку 😢");
                    Log.Error($"Помилка: {chatId} не мав визначеного player.State та відправив повідомлення із текстом:\n{messageText}");
                    return;
                }
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

                //if (chatId == AdminToken) { return; }
                //await SendMessageAsync(ADMIN_TOKEN, CreateRequestMessage(usersDict[chatId], message, usersDict[chatId].State));
                //await SendMessageAsync(chatId, "Дякую за звернення, я передав ваше повідомлення в гуманітарний штаб 😊");

                player.State = Menus.Day;
                context.Players.Update(player);
                await SendMessageAsync(chatId, "Якась помилка, будь ласка, знову оберіть пункт меню та повторіть запит 😥");
                Log.Error($"in chat {chatId}\nreceined message: {messageText}\nplayer state: {player.State}\n{StatsToString(player)}");

                #endregion

            }
            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var errorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Log.Error(errorMessage + "\nStack Trace:\t" + exception.StackTrace);
                return Task.CompletedTask;
            }
        }

        private async Task ApplyMovingInEffect(Player player)
        {
            switch (player.Apartment)
            {
                case ApartmentEnum.Campus:
                    player.Time -= 3; player.Money -= 80; player.Energy -= 40; player.Happiness += 0; player.Health -= 10;
                    await SendMessageAsync(player.ChatId, "Ви переїхали та витратили на це -3 Ч, -80 Г, -40 Е, та -10 З (надірвались виносячи речі з таксі)");
                    break;
                case ApartmentEnum.Family:
                    player.Time -= 3; player.Money -= 10; player.Energy -= 40; player.Happiness += 0; player.Health -= 0;
                    await SendMessageAsync(player.ChatId, "Ви переїхали та витратили на це -3 Ч, -0 Г, -40 Е");
                    break;
                case ApartmentEnum.SmallFlat:
                    player.Time -= 3; player.Money -= 150; player.Energy -= 40; player.Happiness += 0; player.Health -= 10;
                    await SendMessageAsync(player.ChatId, "Ви переїхали та витратили на це -3 Ч, -150 Г, -40 Е, та -10 З (надірвались виносячи речі з таксі)");
                    break;
                case ApartmentEnum.Coliving:
                    player.Time -= 2; player.Money -= 100; player.Energy -= 20; player.Happiness += 15; player.Health -= 0;
                    await SendMessageAsync(player.ChatId, "Вам дуже допомогли із переїздом співмешканці, тому ви все зробили швидше та за менший час");
                    break;
            }
            if (player is { Apartment: ApartmentEnum.Family, IsLivedWithFamily: true } or { Apartment: ApartmentEnum.Campus, IsLivedInCampus: true })
            {
                player.Happiness -= 15;
                await SendMessageAsync(player.ChatId, "Ви переїхали, бо і не було особливого вибору, та не дуже цьому раді. -15 Щ");
            }
        }


        private List<string> _relocationStringList = new();

        /// <summary>
        /// Sets telegram menu for relocation and marks if player lived with parents or in campus
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private async Task SetMoveOutMenuAndCheckWhereLivedAsync(Player player)
        {
            player.State = Menus.Relocation;
            if (player.Apartment == ApartmentEnum.Family) player.IsLivedWithFamily = true;
            else if (player.Apartment == ApartmentEnum.Campus) player.IsLivedInCampus = true;

            if (player.IsFormFilled)
            {
                _relocationStringList = new List<string>{
                    BtnRelocationColiving,
                    BtnRelocationFlat,
                    BtnRelocationCampus,
                    BtnRelocationFamily
                };
            }
            else
            {
                _relocationStringList = new List<string>
                {
                    BtnRelocationFlat,
                    BtnRelocationCampus,
                    BtnRelocationFamily
                };
            }

            List<KeyboardButton> relocationButtonList = new();
            _relocationStringList.ForEach(x => relocationButtonList.Add(x));
            ReplyKeyboardMarkup relocationKeyboard = new(relocationButtonList)
            {
                ResizeKeyboard = true
            };

            await SetKeyboardAsync(player.ChatId, relocationKeyboard, "Час обирати куди переїхати!");
        }

        private async Task ApplyEventEffect(Player player, InGameEventChoice choice)
        {
            player.ChosenChoices.Add(choice.Id);

            player.Time += choice.Time;
            player.Money += choice.Money;
            player.Energy += choice.Energy;
            player.Happiness += choice.Happiness;
            player.Health += choice.Health;
            player.State = Menus.Day;

            await SendMessageAsync(player.ChatId, choice.ChoiceDescription);
        }

        internal async Task SendMessageNotEnoughTimeAsync(Player player)
        {
            player.State = Menus.Day;
            await SetKeyboardAsync(player.ChatId, _menusDic[player.State], "Ви не можете це зробити, сьогодні на це недостатньо часу. Спробуйте завтра");
        }

        private async Task GameUpdateAndSetMenuAsync(Player player, ProstirTgBotContext context)
        {
            await SetKeyboardAsync(player.ChatId, _menusDic[player.State], StatsToString(player));
            context.Update(player);
            await context.SaveChangesAsync(_cancellationToken);

            //check stats
            if (player.Money < 0)
            {
                string text = Banckrupt(player, context);
                await SendMessageAsync(player.ChatId, text);
            }
            if (player.Health == 0)
            {
                string text = Reset(player, context, GameOverEnum.Health);
                await SendMessageAsync(player.ChatId, text);
            }
            if (player.Happiness == 0)
            {
                string text = Reset(player, context, GameOverEnum.Happiness);
                await SendMessageAsync(player.ChatId, text);
            }
            if (player.Energy == 0)
            {
                //it's not duplication mistake
                NextDay(player, context, out string a);
                NextDay(player, context, out string updateText);
                player.Health -= 15;
                await SendMessageAsync(player.ChatId, "Ви проспали увесь день, така сильна втома вплинула на ваше самопочуття (-15 З). Але тепер ви не валитесь з ніг");
                await SendMessageAsync(player.ChatId, updateText);
            }

            if (player.Time == 0)
            {
                NextDay(player, context, out string updateText);
                await SendMessageAsync(player.ChatId, updateText);
            }
        }

        private async Task SendMessageAsync(long chatId, string messageText)
        {
            _ = await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                parseMode: ParseMode.Html,
                cancellationToken: _cancellationToken);
        }

        private async Task SetKeyboardAsync(long chatId, IReplyMarkup replyKeyboardMarkup, string message)
        {
            _ = await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: _cancellationToken);
        }

    }
}