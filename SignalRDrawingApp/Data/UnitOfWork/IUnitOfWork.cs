using SignalRDrawingApp.Data.Repositories;
using System;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IDrawingSessionRepository DrawingSessions { get; }
        IDrawingStrokeRepository DrawingStrokes { get; }
        IChatMessageRepository ChatMessages { get; }
        
        Task<int> CompleteAsync();
    }
} 