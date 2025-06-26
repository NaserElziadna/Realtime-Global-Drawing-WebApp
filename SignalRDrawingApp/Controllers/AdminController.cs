using Microsoft.AspNetCore.Mvc;
using SignalRDrawingApp.Data.UnitOfWork;
using SignalRDrawingApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            // Get statistics for the dashboard
            var sessions = await _unitOfWork.DrawingSessions.GetAllAsync();
            var totalSessions = sessions.Count();
            
            var totalStrokes = 0;
            var totalChatMessages = 0;
            
            foreach (var session in sessions)
            {
                var strokes = await _unitOfWork.DrawingStrokes.GetBySessionIdAsync(session.Id);
                var messages = await _unitOfWork.ChatMessages.GetBySessionIdAsync(session.Id);
                
                totalStrokes += strokes.Count();
                totalChatMessages += messages.Count();
            }

            ViewBag.TotalSessions = totalSessions;
            ViewBag.TotalStrokes = totalStrokes;
            ViewBag.TotalChatMessages = totalChatMessages;
            ViewBag.ConnectedUsers = GetConnectedUsersCount();

            return View();
        }

        public async Task<IActionResult> Sessions()
        {
            var sessions = await _unitOfWork.DrawingSessions.GetAllAsync();
            return View(sessions);
        }

        public async Task<IActionResult> SessionDetails(int id)
        {
            var session = await _unitOfWork.DrawingSessions.GetByIdAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            var strokes = await _unitOfWork.DrawingStrokes.GetBySessionIdAsync(id);
            var messages = await _unitOfWork.ChatMessages.GetBySessionIdAsync(id);

            ViewBag.Strokes = strokes;
            ViewBag.Messages = messages;

            return View(session);
        }

        public async Task<IActionResult> ChatMessages()
        {
            var allMessages = await _unitOfWork.ChatMessages.GetAllAsync();
            return View(allMessages);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var session = await _unitOfWork.DrawingSessions.GetByIdAsync(id);
            if (session != null)
            {
                _unitOfWork.DrawingSessions.Remove(session);
                await _unitOfWork.CompleteAsync();
            }
            return RedirectToAction(nameof(Sessions));
        }

        [HttpPost]
        public async Task<IActionResult> ClearChatMessages(int sessionId)
        {
            var messages = await _unitOfWork.ChatMessages.GetBySessionIdAsync(sessionId);
            foreach (var message in messages)
            {
                _unitOfWork.ChatMessages.Remove(message);
            }
            await _unitOfWork.CompleteAsync();
            
            return RedirectToAction(nameof(SessionDetails), new { id = sessionId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _unitOfWork.ChatMessages.GetByIdAsync(id);
            if (message != null)
            {
                _unitOfWork.ChatMessages.Remove(message);
                await _unitOfWork.CompleteAsync();
            }
            return RedirectToAction(nameof(ChatMessages));
        }

        // API endpoints for real-time data
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var sessions = await _unitOfWork.DrawingSessions.GetAllAsync();
            var totalSessions = sessions.Count();
            
            var totalStrokes = 0;
            var totalChatMessages = 0;
            
            foreach (var session in sessions)
            {
                var strokes = await _unitOfWork.DrawingStrokes.GetBySessionIdAsync(session.Id);
                var messages = await _unitOfWork.ChatMessages.GetBySessionIdAsync(session.Id);
                
                totalStrokes += strokes.Count();
                totalChatMessages += messages.Count();
            }

            return Json(new
            {
                totalSessions,
                totalStrokes,
                totalChatMessages,
                connectedUsers = GetConnectedUsersCount()
            });
        }

        private int GetConnectedUsersCount()
        {
            // Get the connected users count from the DrawingHub
            return SignalRDrawingApp.Hubs.DrawingHub.GetConnectedUsersCount();
        }
    }
} 