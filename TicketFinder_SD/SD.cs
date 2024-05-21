using System.Text;
using TicketFinder_Models;

namespace TicketFinder_Common
{
    public class SD
    {
        //API url
        public static readonly string api_url = "https://localhost:7196/api/";

        // search command
        public static readonly string search_command = "/search";
        public static readonly int search_command_steps = 3;

        public static readonly Dictionary<int, string> search_command_messages = new()
        {
            { 0, "Введдіть міста <b>відбуття</b> та <b>прибуття</b>\n(Наприкад: <i>Київ - Львів</i>)" },
            { 1, "Введдіть <b>дату</b> відправлення\n(Наприкад: <i>01.01.2024</i>)" },
            { 2, "Введдіть <b>час</b> відправлення від:\n(Наприклад: <i>18:00</i>)" },
            { 3, "Шукаю квитки..." }
        };

        public static readonly string resut_message = "Квитки за вказаними параметрами:";

        // notifications command
        public static readonly string notifications_empty = "У вас немає створених сповіщень";
        public static readonly string notifications_any = "Ваші сповіщення:";

        // notification-create command
        public static readonly string notificationCreate_command = "/notification-create";
        public static readonly int notificationCreate_command_steps = 5;

        public static readonly Dictionary<int, string> notification_command_messages = new()
        {
            { 0, "Введдіть міста <b>відбуття</b> та <b>прибуття</b>\n(Наприкад: <i>Київ - Львів</i>)" },
            { 1, "Введдіть <b>час</b> відправлення від:\n(Наприклад: <i>18:00</i>)" },
            { 2, "Введіть <b>день (дні)</b> сповіщення:\n(Щоб ввести декілька днів, вводьте їх через кому)" },
            { 3, "Введіть <b>час</b> сповіщення в годинах:\n(Наприклад: <i>18:00</i>)" },
            { 4, "Введіть наскільки днів вперед виконувати пошук квитків:" }
        };

        // notification-update command
        public static readonly string notificationUpdate_command = "/notification-update";
        public static readonly int notificationUpdate_command_steps = 5;

        // notification-delete command
        public static readonly string notificationDelete_command = "/notification-delete";

        public static string ConstructTicketMessage(TicketDTO ticketDTO)
        {
            StringBuilder text = new();

            text.Append($"<b>Поїзд #{ticketDTO.Num}</b>\n");
            text.Append($"<b>{ticketDTO.From}</b> -> <b>{ticketDTO.To}</b>\n");
            text.Append($"Відправлення: <b>{ticketDTO.Departure}</b>\n");
            text.Append($"Прибуття: <b>{ticketDTO.Arrival}</b>\n");
            text.Append($"Час подорожі: <b>{ticketDTO.Duration}</b>");

            return text.ToString();
        }

        public static string ConstructNotificationMessage(NotificationDTO notificationDTO)
        {
            StringBuilder text = new();

            text.Append($"Маршрут: <b>{notificationDTO.From}</b> -> <b>{notificationDTO.To}</b>\n");
            text.Append($"Час відправлення від: <b>{notificationDTO.TicketTime}</b>\n");
            text.Append($"Дні сповіщення: <b>{notificationDTO.Days}</b>\n");
            text.Append($"Час сповіщення: <b>{notificationDTO.Time}</b>\n");
            text.Append($"Пошук на дні(в) вперед: <b>{notificationDTO.DaysToTrip}</b>");

            return text.ToString();
        }
    }
}
