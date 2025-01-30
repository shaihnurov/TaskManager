using Microsoft.EntityFrameworkCore;
using TaskManager.Model;

namespace TaskManager.Services
{
    public class TaskService
    {
        public TaskService()
        {
            using var db = new ApplicationContextDb();
            db.Database.EnsureCreated(); // Проверяет, создана ли база, и создаёт её, если нет
        }

        public static async Task AddTask(TaskModel task)
        {
            using var db = new ApplicationContextDb();
            await db.Tasks.AddAsync(task);
            await db.SaveChangesAsync();
        }
        public static async Task AddTasksWithoutDuplicates(List<TaskModel> newTasks)
        {
            using var db = new ApplicationContextDb();

            // Загружаем существующие задачи в БД
            var existingTasks = db.Tasks.Select(t => t.Name).ToHashSet();

            // Фильтруем новые задачи, исключая дубликаты по имени
            var tasksToAdd = newTasks.Where(task => !existingTasks.Contains(task.Name)).ToList();

            if (tasksToAdd.Count != 0)
            {
                db.Tasks.AddRange(tasksToAdd);
                await db.SaveChangesAsync();
            }
        }
        public static async Task UpdateTask(TaskModel updatedTask)
        {
            using var db = new ApplicationContextDb();
            var existingTask = await db.Tasks.FindAsync(updatedTask.Id);

            if (existingTask != null)
            {
                existingTask.Name = updatedTask.Name;
                existingTask.Description = updatedTask.Description;
                existingTask.Deadline = updatedTask.Deadline;
                existingTask.Status = updatedTask.Status;

                await db.SaveChangesAsync();
            }
        }
        public static async Task<List<TaskModel>> GetTasks()
        {
            using var db = new ApplicationContextDb();
            return await db.Tasks.ToListAsync();
        }
        public static async Task DeleteTask(TaskModel task)
        {
            using var db = new ApplicationContextDb();

            var taskToDelete = await db.Tasks.FindAsync(task.Id);

            if (taskToDelete != null)
            {
                db.Tasks.Remove(taskToDelete);
                await db.SaveChangesAsync();
            }
        }
    }
}