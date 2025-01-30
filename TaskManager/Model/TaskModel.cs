using System.ComponentModel.DataAnnotations;

namespace TaskManager.Model
{
    public class TaskModel
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime CreateDate { get; set; }
        public string? Status { get; set; }
    }
}