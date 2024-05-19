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
            { 0, "Введдіть місто <b>відбуття</b> та <b>прибуття</b>\n(Наприкад: <i>Київ - Львів</i>)" },
            { 1, "Введдіть <b>дату</b> відправлення\n(Наприкад: <i>01.01.2024</i>)" },
            { 2, "Введдіть <b>час</b> відправлення від:\n(Наприклад: <i>18:00</i>)" },
            { 3, "Шукаю квитки..." }
        };

        public static readonly string resut_message = "Квитки за вказаними параметрами:";

        // notifications command
        public static readonly string notifications_empty = "У вас немає створених сповіщень";
        public static readonly string notifications_any = "Ваші сповішення:";

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
            text.Append($"Дні сповіщення: <b>{notificationDTO.Days}</b>");
            text.Append($"Час сповіщення: <b>{notificationDTO.Time}</b>");
            text.Append($"Пошук на дні(в) вперед: <b>{notificationDTO.DaysToTrip}</b>");
            text.Append($"Час відправлення від: <b>{notificationDTO.TicketTime}</b>");

            return text.ToString();
        }
    }
}
