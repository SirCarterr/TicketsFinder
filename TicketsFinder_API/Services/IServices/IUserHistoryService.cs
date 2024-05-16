using TicketFinder_Models;

namespace TicketsFinder_API.Services.IServices
{
    public interface IUserHistoryService
    {
        public Task<int> CreateHistory(UserHistoryDTO userHistoryDTO);
        public Task<int> UpdateHistory(UserHistoryDTO userHistoryDTO);
        public Task<UserHistoryDTO> GetHistory(int chatId);
    }
}
