using SignalRDrawingApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Data.Repositories
{
    public interface IChatMessageRepository : IRepository<ChatMessage>
    {
        Task<IEnumerable<ChatMessage>> GetBySessionIdAsync(int sessionId, int maxMessages = 100);
        Task<ChatMessage> AddMessageAsync(string username, string message, int sessionId);
    }
} 