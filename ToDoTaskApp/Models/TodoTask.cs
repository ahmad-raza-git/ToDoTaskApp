using System;
using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models
{
    public class TodoTask
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Category { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsCompleted { get; set; }
        public string? FileUrl { get; set; }
        public string? ReminderEmail { get; set; }
    }
}
