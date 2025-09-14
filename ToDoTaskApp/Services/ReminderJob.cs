using Microsoft.EntityFrameworkCore;
using TodoApp.Data;

namespace TodoApp.Services
{
    public class ReminderJob
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public ReminderJob(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendDailyReminders()
        {
            var today = DateTime.UtcNow.Date;

            var tasks = await _context.TodoTask
                .Where(t => t.DueDate != null && t.DueDate.Value.Date == today)
                .ToListAsync();

            foreach (var task in tasks)
            {
                if (!string.IsNullOrEmpty(task.ReminderEmail))
                {
                    await _emailService.SendEmailAsync(
                        task.ReminderEmail,
                        "Task Reminder",
                        $"Reminder: Your task '{task.Title}' is due today!"
                    );
                }
            }
        }
    }
}
