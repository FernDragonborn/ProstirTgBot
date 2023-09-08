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
using static ProstirTgBot.Interactions;
using static ProstirTgBot.TelegramButtons;
using static ProstirTgBot.TelegramMessages;

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
                        new KeyboardButton[] { BtnStart },
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
        private readonly ReceiverOptions _receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };
        private readonly CancellationTokenSource _cts = new();
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
                    player = new Player(message.From.Username, chatId);
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

                await CheckIfRelocationNeededOrApplyIt(player, context, messageText);

                //TODO add events to DB 
                if (player.State == Menus.Event)
                {
                    InGameEventChoice? choice = context.InGameEventChoice.FirstOrDefault(x => x.ChoiceName == messageText);
                    if (choice == null) return;
                    await ApplyEventEffect(player, choice);
                    await GameUpdateAndSetMenuAsync(player, context);
                    return;
                }
                bool isFinished = false;
                if (player.State != Menus.Start && player.State != Menus.GetName)
                    isFinished = TryCheckForEvents(player, _eventStringList, context, ref _eventKeyboard, ref _inGameEvent);
                if (isFinished)
                {
                    if (player.Day == 0) await SendMessageAsync(chatId, "⬇ Це повідомлення - івент. У нього є невеличкий опис, та кілкьа варіантів розвитку. Обери той, який вважаєш кращим. Кожен буде мати якісь наслідки");
                    await SetKeyboardAsync(player.ChatId, _eventKeyboard, $"Треба прийняти рішення!\n\n{_inGameEvent.EventDescription}");
                    return;
                }

                bool isApplied = false;
                // choose of menu 
                switch (messageText)
                {
                    case BtnStart:
                        {
                            player.State = Menus.GetName;
                            await SetKeyboardAsync(chatId, _menusDic[player.State], "Напиши як тебе називати протягом гри");
                            return;
                        }

                    case BtnWork:
                        player.State = Menus.Work;
                        await SetKeyboardAsync(chatId, _menusDic[player.State], MessageWork);
                        return;
                    case BtnLeisure:
                        player.State = Menus.Lesuire;
                        await SetKeyboardAsync(chatId, _menusDic[player.State], MessageLeisure);
                        return;
                    case BtnActivity:
                        player.State = Menus.Activity;
                        await SetKeyboardAsync(chatId, _menusDic[player.State], MessageActivities);
                        return;

                    case BtnWorkBarista:
                    case BtnWorkTutor:
                    case BtnWorkFreelance:
                    case BtnLeisureLake:
                    case BtnLeisureGym:
                    case BtnLeisureFriend:
                    case BtnActivityVolunteering:
                        isApplied = player.ApplyInteraction(InteractionsDic[messageText]);
                        if (!isApplied)
                        {
                            await SetKeyboardAsync(player.ChatId, _menusDic[player.State], "Ви не можете це зробити, сьогодні на це недостатньо часу. Спробуйте завтра");
                            return;
                        }
                        player.State = Menus.Day; await GameUpdateAndSetMenuAsync(player, context);
                        return;
                    case BtnActivityFillInForm:
                        isApplied = player.ApplyInteraction(InteractionsDic[messageText]);
                        if (!isApplied)
                        {
                            await SetKeyboardAsync(player.ChatId, _menusDic[player.State], "Ви не можете це зробити, сьогодні на це недостатньо часу. Спробуйте завтра");
                            return;
                        }
                        await SendMessageAsync(chatId, "Ви заповнили форму та буквально через годину вам відповіли, що ви підходите! Тепер ви можете переїхати на Д'Іскру!");
                        return;
                    case BtnActivitySearch:
                        {
                            isApplied = player.ApplyInteraction(InteractionsDic[messageText]);
                            if (!isApplied)
                            {
                                await SetKeyboardAsync(player.ChatId, _menusDic[player.State], "Ви не можете це зробити, сьогодні на це недостатньо часу. Спробуйте завтра");
                                return;
                            }
                            player.ActivitiesFound += 1;
                            string text = AddActivityButton(player, _menusDic);
                            await SendMessageAsync(chatId, text);
                            await GameUpdateAndSetMenuAsync(player, context);
                            return;
                        }
                }

                // Get name
                if (player.State == Menus.GetName && messageText != BtnStart)
                {
                    player.InGameName = messageText.Normalize().Trim();
                    if (player.InGameName.Length < 2 || !(Regex.IsMatch(player.InGameName, @"^[A-Za-zА-Яа-я][\p{L}\s]{1,19}$")))
                    {
                        await SendMessageAsync(chatId, "Перевірте чи ім'я більше 2х та менше 20ти символів, та чи воно складається із букв");
                        return;
                    }

                    player.State = Menus.Day;
                    context.Players.Update(player);
                    await context.SaveChangesAsync(cancellationToken);
                    await SendMessageAsync(chatId, $"Тепер тебе звати {player.InGameName}!");
                    await SetMoveOutMenuAndCheckWhereLivedAsync(player, context);
                    await SendMessageAsync(chatId, $"А ще ось твої характеристики:\n{StatsToString(player)}\n\nВсе це - твої життєві показники. Ну тобто, якщо <b>здоров'я</b> або <b>щастя</b> досягнуть 0, то ти програєш. Відновити їх можна поки ти відпочиваєш\n\n<b>Енергію</b> ти витрачаєш на кожну дію вдень, але вона відновлююється вночі, поки ти спиш\n\n<b>Час</b>, його ти маєш 4 одиниці на день. Можна було б зробити 24 години, але програміст сказав, що це заскладно та давайте не морочитись. Як тільки вони кінчаться ти підеш спати, і наступить новий день\n\nА <b>гроші</b> - це гроші. За них ти живеш десь, а отримуєш на роботі");
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

        private async Task CheckIfRelocationNeededOrApplyIt(Player player, ProstirTgBotContext context, string messageText)
        {
            if (player is not { Day: 0 or 7 or 14, State: Menus.Day or Menus.Relocation, Time: 4 }) return;

            if (player.State == Menus.Day) await SetMoveOutMenuAndCheckWhereLivedAsync(player, context);

            if (_relocationStringList.FirstOrDefault(x => x == messageText) == null) return;
            try
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
            catch (NotImplementedException ex)
            {
                await SendErrorAsync(player.ChatId, ex);
            }
        }

        private async Task ApplyMovingInEffect(Player player)
        {
            switch (player.Apartment)
            {
                case ApartmentEnum.Campus:
                    player.Time -= 3; player.Money -= 80; player.Energy -= 40; player.Happiness += 0; player.Health -= 10;
                    await SendMessageAsync(player.ChatId, "Ви переїхали\n\n-3 часу, -80 грошей, -40 енергіх, та -10 здоров'я (надірвались виносячи речі з таксі)");
                    break;
                case ApartmentEnum.Family:
                    player.Time -= 3; player.Money -= 10; player.Energy -= 40; player.Happiness += 0; player.Health -= 0;
                    await SendMessageAsync(player.ChatId, "Ви переїхали\n\n-3 часу, -0 грошей, -40 енергії");
                    break;
                case ApartmentEnum.SmallFlat:
                    player.Time -= 3; player.Money -= 150; player.Energy -= 40; player.Happiness += 0; player.Health -= 10;
                    await SendMessageAsync(player.ChatId, "Ви переїхали\n\n-3 часу, -150 грошей, -40 енергії, та -10 здоров'я (надірвались виносячи речі з таксі)");
                    break;
                case ApartmentEnum.Coliving:
                    player.Time -= 2; player.Money -= 100; player.Energy -= 20; player.Happiness += 15; player.Health -= 0;
                    await SendMessageAsync(player.ChatId, "Вам дуже допомогли із переїздом співмешканці, тому ви все зробили швидше та простіше\n\n-2 часу, -100 грошей, -20 енергії");
                    break;
            }
            if (player is { Apartment: ApartmentEnum.Family, IsLivedWithFamily: true } or { Apartment: ApartmentEnum.Campus, IsLivedInCampus: true })
            {
                player.Happiness -= 15;
                await SendMessageAsync(player.ChatId, "Ви переїхали, бо і не було особливого вибору, та не дуже цьому раді\n\n-15 щастя");
            }
        }

        private List<string> _relocationStringList = new();

        /// <summary>
        /// Modifies _relocationStringList. Sets telegram menu for relocation and marks if player lived with parents or in campus 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task SetMoveOutMenuAndCheckWhereLivedAsync(Player player, ProstirTgBotContext context)
        {
            //TODO write message for options
            player.State = Menus.Relocation;
            context.Update(player);
            context.SaveChanges();

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

            await SetKeyboardAsync(player.ChatId, relocationKeyboard, $"Це вітальне повідомлення, якщо граєш тільки перший раз, то прочитай, щоб розуміти що відбувається))\n\nТебе звати {player.InGameName}, та ти поступаєш на другий курс вузу. Звісно можна було б, як і минулого року просто навчатись, отримувати оцінки і на цьому все, але тобі це набридло. Тому ти вирішив трошки змінити своє життя цього року. Почати підробляти, може навіть переїхати. Ну, час покаже 😉\n\nЧас обирати де ти будеш жити!");
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
                await SetKeyboardAsync(player.ChatId, _menusDic[player.State], text);
            }
            if (player.Health == 0)
            {
                string text = Reset(player, context, GameOverEnum.Health);
                await SetKeyboardAsync(player.ChatId, _menusDic[player.State], text);
            }
            if (player.Happiness == 0)
            {
                string text = Reset(player, context, GameOverEnum.Happiness);
                await SendMessageAsync(player.ChatId, text);
            }
            if (player.Energy == 0)
            {
                //it's not duplication mistake. It's need to skip 2 days, not 1
                NextDay(player, context, out string a);
                NextDay(player, context, out string updateText);
                player.Health -= 15;
                await SendMessageAsync(player.ChatId, "Ви проспали увесь день, така сильна втома вплинула на ваше самопочуття. Але тепер ви не валитесь з ніг\n\n-15 здоров'я");
                await SendMessageAsync(player.ChatId, updateText);
                await CheckIfRelocationNeededOrApplyIt(player, context, "");
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
                parseMode: ParseMode.Html,
                cancellationToken: _cancellationToken);
        }

        private async Task SendErrorAsync(long chatId, NotImplementedException ex)
        {
            await SendMessageAsync(chatId, DotNetEnv.Env.GetString("ERROR_MESSAGE") + $"Stack strace:   {ex.StackTrace}\n\nError message:   {ex.Message}");
        }

    }
}