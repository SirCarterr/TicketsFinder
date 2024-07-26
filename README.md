# TicketsFinder
This is the bachelor diploma work for National Aviation University on theme **"Telegram bot for searching and monitoring train tickets"**.

## Description
The project is consist of 2 parts: one represents functionlity for managing user inputs in Telegram bot and the other for managing business logic (perform CRUD operations in db, make calls to external service, etc.). The project allows users to search the train tickets for **Укрзалізниця** railway operator right in the Telegram messenger using different search parameter (route, departure date and time). Also, users may set up notifications to monitor the available tickets for the specified routes. To get the data about available tickets **TicketFinder_API** runs the Selenium chrome driver to access the railway operator website and scrap the available data from it.

Techologies used: C#, ASP.Net, REST API, MS SQL Server, EntityFramework, Selenium, Telegram.Bot, NUnit, Moq.

## Installation
1. Clone repository
2. Get bot token for your bot from **Bot Father**
3. Add the `secrets.json` file for created user secret id in `TicketFinder_Bot.csproj` by path `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
4. Add lines in `secrets.json`:
```
"Telegram:bot_token": "<your token>",
"API:key": "<key is local and located in /TicketFinder_API/Helper/ApiKeyProvider.cs>",
"API:url": "https://localhost:7196/api/"
```
6. Set up multiple running projects for `TicketFinder_Bot` and `TicketFinder_API`
7. Start projects

## How to use
To communicate with the bot you can use such commands:
```
/start - shows greeting message

/search - search tickets for specified parameters; returns trains with link to the operator site for booking the seat of chosen car class

/history - look for last 5 searches; can perfom route search by clickinh on inline button

/notifications - look for created notifications for routes with specified parameters; have inline buttons to edit or delete it

/notificationCreate - create new notification with specified parameters
```
Created notification will search for tickets on specified days, time and days ahead (date of the ticket), and show the exatly same response as `/search` command.
