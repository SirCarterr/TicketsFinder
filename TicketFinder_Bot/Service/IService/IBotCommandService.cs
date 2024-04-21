﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TicketFinder_Bot.Service.IService
{
    public interface IBotCommandService
    {
        public (bool, object?) SearchCommand(string message, int commandStep);
    }
}