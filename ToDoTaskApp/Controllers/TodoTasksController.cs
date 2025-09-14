using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Controllers
{
    public class TodoTasksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly BlobService _blobService;
        private readonly EmailService _emailService;

        public TodoTasksController(AppDbContext context, BlobService blobService, EmailService emailService)
        {
            _context = context;
            _blobService = blobService;
            _emailService = emailService;
        }

        // GET: TodoTasks
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.TodoTask.ToListAsync();
            return View(tasks); // Task model includes AttachmentPath
        }

        // GET: TodoTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todoTask = await _context.TodoTask
                .FirstOrDefaultAsync(m => m.Id == id);
            if (todoTask == null)
            {
                return NotFound();
            }

            return View(todoTask);
        }

        // GET: TodoTasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TodoTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoTask task, IFormFile? attachment)
        {
            if (ModelState.IsValid)
            {
                if (attachment != null && attachment.Length > 0)
                {
                    using var stream = attachment.OpenReadStream();
                    task.FileUrl = await _blobService.UploadFileAsync(stream, attachment.FileName);
                }

                task.Id = Guid.NewGuid();
                _context.Add(task);
                await _context.SaveChangesAsync();

                await _emailService.SendEmailAsync(
                        task.ReminderEmail,
                        "New Task Added",
                        $"Your task '{task.Title}' is successfully added!"
                    );

                return RedirectToAction(nameof(Index));
            }

            return View(task);
        }


        // GET: TodoTasks/Edit/{id}
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var todoTask = await _context.TodoTask.FindAsync(id);
            if (todoTask == null) return NotFound();

            return View(todoTask);
        }

        // POST: TodoTasks/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TodoTask task, IFormFile? attachment)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTask = await _context.TodoTask.FindAsync(id);
                    if (existingTask == null)
                        return NotFound();

                    // update basic fields
                    existingTask.Title = task.Title;
                    existingTask.Category = task.Category;
                    existingTask.ReminderEmail = task.ReminderEmail;
                    existingTask.DueDate = task.DueDate;
                    existingTask.IsCompleted = task.IsCompleted;

                    // handle file replacement
                    if (attachment != null && attachment.Length > 0)
                    {
                        // delete old file if it exists
                        if (!string.IsNullOrEmpty(existingTask.FileUrl))
                        {
                            var oldFileName = Path.GetFileName(new Uri(existingTask.FileUrl).LocalPath);
                            await _blobService.DeleteFileAsync(oldFileName);
                        }

                        // upload new file
                        using var stream = attachment.OpenReadStream();
                        existingTask.FileUrl = await _blobService.UploadFileAsync(stream, attachment.FileName);
                    }

                    _context.Update(existingTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TodoTask.Any(e => e.Id == task.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }


        // GET: TodoTasks/Download/{id}
        public async Task<IActionResult> Download(Guid id)
        {
            var task = await _context.TodoTask.FindAsync(id);
            if (task == null || string.IsNullOrEmpty(task.FileUrl)) return NotFound();

            var fileName = Path.GetFileName(new Uri(task.FileUrl).LocalPath);
            var stream = await _blobService.DownloadFileAsync(fileName);
            return File(stream, "application/octet-stream", fileName);
        }

        // GET: TodoTasks/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todoTask = await _context.TodoTask
                .FirstOrDefaultAsync(m => m.Id == id);
            if (todoTask == null)
            {
                return NotFound();
            }

            return View(todoTask);
        }

        // POST: TodoTasks/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var todoTask = await _context.TodoTask.FindAsync(id);
            if (todoTask == null)
            {
                return NotFound();
            }

            // delete file from blob storage if it exists
            if (!string.IsNullOrEmpty(todoTask.FileUrl))
            {
                var fileName = Path.GetFileName(new Uri(todoTask.FileUrl).LocalPath);
                await _blobService.DeleteFileAsync(fileName);
            }

            _context.TodoTask.Remove(todoTask);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
