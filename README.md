This is the bachelor diploma work for National Aviation University on theme **"Telegram bot for searching and monitoring train tickets"**.

The project is consist of 2 parts: one represents functionlity for managing user inputs in Telegram bot and the other for managing business logic (perform CRUD operations in db, make calls to external service, etc.). The project allows users to search the train tickets for **Укрзалізниця** railway operator right in the Telegram messenger using different search parameter (route, departure date and time). Also, users may set up notifications to monitor the available tickets for the specified routes. To get the data about available tickets **TicketFinder_API** runs the Selenium chrome driver to access the railway operator website and scrap the available data from it.

Techologies used: C#, ASP.Net, REST API, MS SQL Server, EntityFramework, Selenium, Telegram.Bot, NUnit, Moq.
