using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketFinder_Models;

namespace TicketFinder_Bot.Service.IService
{
    public interface IUserHistoryService
    {
        public Task<ResponseModelDTO> UpdateUserHistory(UserHistoryDTO userHistoryDTO);
        public Task<ResponseModelDTO> GetUserHistory(int chatId);
    }
}
