using static ProstirTgBot.Interactions;

namespace ProstirTgBot
{
    internal class TelegramMessages
    {
        internal static readonly string MessageWork = "Доступні роботи:" +
                                               $"\n\n<b>{IntrActivitySearch.Name}</b>\nЧас: {IntrActivitySearch.Time}\nГроші: {IntrActivitySearch.Money}\nЕнергія:  {IntrActivitySearch.Energy}\nЩастя: {IntrActivitySearch.Happiness}\nЗдоров'я: {IntrActivitySearch.Health}" +
                                               $"\n\n<b>{IntrWorkTutor.Name}</b>\nЧас: {IntrWorkTutor.Time}\nГроші: {IntrWorkTutor.Money}\nЕнергія:  {IntrWorkTutor.Happiness}\nЩастя: {IntrWorkTutor.Health}\nЗдоров'я: {IntrWorkTutor.Health}" +
                                               $"\n\n<b>{IntrWorkFreelance.Name}</b> \nЧас: {IntrWorkFreelance.Time}\nГроші: {IntrWorkFreelance.Money}\nЕнергія:  {IntrWorkFreelance.Energy}\nЩастя: {IntrActivitySearch.Happiness}\nnЗдоров'я: {IntrActivitySearch.Health}";

        internal static readonly string MessageLeisure = "Доступний відпочинок:" +
                                                        $"\n\n<b>{IntrLeisureLake.Name}</b> \nЧас: {IntrLeisureLake.Time}\nГроші: {IntrLeisureLake.Money}\nЕнергія:  {IntrLeisureLake.Energy}\nЩастя: {IntrLeisureLake.Happiness}\nnЗдоров'я: {IntrLeisureLake.Health}" +
                                                        $"\n\n<b>{IntrLeisureGym.Name}</b>\nЧас: {IntrLeisureGym.Time}\nГроші: {IntrLeisureGym.Money}\nЕнергія:  {IntrLeisureGym.Energy}\nЩастя: {IntrLeisureGym.Happiness}\nЗдоров'я: {IntrLeisureGym.Health}" +
                                                        $"\n\n<b>{IntrLeisureFriend.Name}</b>\nЧас: {IntrLeisureFriend.Time}\nГроші: {IntrLeisureFriend.Money}\nЕнергія:  {IntrLeisureFriend.Energy}\nЩастя: {IntrLeisureFriend.Happiness}\nЗдоров'я: {IntrLeisureFriend.Health}";

        internal static readonly string MessageActivities = "Доступні активності:" +
                                                           $"\n\n<b>{IntrActivityFillInForm.Name}</b>\nЧас: {IntrActivityFillInForm.Time}\nГроші: {IntrActivityFillInForm.Money}\nЕнергія:  {IntrActivityFillInForm.Energy}\nЩастя: {IntrActivityFillInForm.Happiness}\nЗдоров'я: {IntrActivityFillInForm.Health}" +
                                                           $"\n\n<b>{IntrActivitySearch.Name}</b>\nЧас: {IntrActivitySearch.Time}\nГроші: {IntrActivitySearch.Money}\nЕнергія:  {IntrActivitySearch.Energy}\nЩастя: {IntrActivitySearch.Happiness}\nЗдоров'я: {IntrActivitySearch.Health}" +
                                                           $"\n\n<b>{IntrActivityVolunteering.Name}</b> \nЧас: {IntrActivityVolunteering.Time}\nГроші: {IntrActivityVolunteering.Money}\nЕнергія:  {IntrActivityVolunteering.Energy}\nЩастя: {IntrActivityVolunteering.Happiness}\nnЗдоров'я: {IntrActivityVolunteering.Health}";
    }
}