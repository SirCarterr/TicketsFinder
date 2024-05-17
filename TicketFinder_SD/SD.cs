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
            { 2, "Введдіть <b>час</b> в годинах\n(Наприклад: <i>18:00</i>))" },
            { 3, "Шукаю квитки..." }
        };

        public static readonly string resut_message = "Квитки за вказаними параметрами:";

        public static string ConstructTicketMessage(TicketDTO ticketDTO)
        {
            StringBuilder text = new();

            text.Append($"<b>Поїзд</b> #{ticketDTO.Num}\n");
            text.Append($"<b>{ticketDTO.From}</b> -> <b>{ticketDTO.To}</b>\n");
            text.Append($"<b>Відправлення</b>: {ticketDTO.Departure}\n");
            text.Append($"<b>Прибуття</b>: {ticketDTO.Arrival}\n");
            text.Append($"<b>Час подорожі</b>: {ticketDTO.Duration}\n\n");

            return text.ToString();
        }
    }
}
