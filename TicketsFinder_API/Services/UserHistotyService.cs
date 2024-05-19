using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TicketFinder_Models;
using TicketsFinder_API.Models;
using TicketsFinder_API.Models.Data;
using TicketsFinder_API.Services.IServices;

namespace TicketsFinder_API.Services
{
    public class UserHistotyService : IUserHistoryService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public UserHistotyService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<int> CreateHistory(UserHistoryDTO userHistoryDTO)
        {
            await _db.UserHistories.AddAsync(_mapper.Map<UserHistoryDTO, UserHistory>(userHistoryDTO));
            return await _db.SaveChangesAsync();
        }

        public async Task<UserHistoryDTO> GetHistory(long chatId)
        {
            UserHistory? userHistory = await _db.UserHistories.FirstOrDefaultAsync(h => h.ChatId == chatId);
            if (userHistory != null)
                return _mapper.Map<UserHistory, UserHistoryDTO>(userHistory);
            return new();
        }

        public async Task<int> UpdateHistory(UserHistoryDTO userHistoryDTO)
        {
            UserHistory? userHistory = await _db.UserHistories.FirstOrDefaultAsync(h => h.ChatId == userHistoryDTO.ChatId);
            if (userHistory != null)
            {
                userHistory.History = HistoryHelper(userHistory.History, userHistoryDTO.History);
                _db.UserHistories.Update(userHistory);
                return await _db.SaveChangesAsync();
            }
            return 0;
        }

        private static string HistoryHelper(string origin, string addition)
        {
            var originList = origin.Split(';').ToList();

            originList.Remove(addition);
            originList.Insert(0, addition);

            return string.Join(";", originList.Take(5));
        }
    }
}
