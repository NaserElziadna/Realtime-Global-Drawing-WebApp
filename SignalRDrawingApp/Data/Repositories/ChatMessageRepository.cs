using Microsoft.EntityFrameworkCore;
using SignalRDrawingApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Data.Repositories
{
    public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(ApplicationDbContext context) 
            : base(context)
        {
        }
        
        private ApplicationDbContext AppDbContext => (ApplicationDbContext)Context;
        
        public async Task<IEnumerable<ChatMessage>> GetBySessionIdAsync(int sessionId, int maxMessages = 100)
        {
            return await AppDbContext.ChatMessages
                .Where(c => c.DrawingSessionId == sessionId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(maxMessages)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<ChatMessage> AddMessageAsync(string username, string message, int sessionId)
        {
            var chatMessage = new ChatMessage
            {
                Username = username,
                Message = message,
                DrawingSessionId = sessionId
            };
            
            await AddAsync(chatMessage);
            return chatMessage;
        }
    }
} 