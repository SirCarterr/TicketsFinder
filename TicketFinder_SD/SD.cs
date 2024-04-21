namespace TicketFinder_Common
{
    public class SD
    {
        // /search
        public static readonly string search_command = "/search";
        public static readonly int search_command_steps = 3;

        public static readonly string getRoute_message = "Введдіть місто *відбуття* та *прибуття*\n(Наприкад: `Київ - Львів`)";
        public static readonly string getDate_message = "Введдіть *дату* відправлення\n(Наприкад: `01.01.2024`)";
        public static readonly string getTime_message = "Введдіть *час*";
        public static readonly string proccesing_message = "Шукаю квитки...";
        public static readonly string resut_message = "Ось всі квитки за вказаними параметрами:";
    }
}
