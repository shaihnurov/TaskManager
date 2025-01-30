using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;
using System.Data.Common;
using System.Windows;
using TaskManager.Model;

namespace TaskManager.Services
{
    /// <summary>
    /// Сервис для работы с задачами в базе данных
    /// </summary>
    public class TaskService
    {
        /// <summary>
        /// Статический конструктор класса
        /// Гарантирует создание базы данных при первом обращении к сервису
        /// </summary>
        static TaskService()
        {
            try
            {
                using var db = new ApplicationContextDb();
                db.Database.EnsureCreated(); // Проверяет, создана ли база, и создаёт её при необходимости
                Log.Information("База данных успешно инициализирована");
            }
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка подключения к базе данных");
                MessageBox.Show("Ошибка подключения к базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Неизвестная ошибка при инициализации базы данных");
                MessageBox.Show("Неизвестная ошибка при запуске", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавляет новую задачу в базу данных
        /// </summary>
        public static async Task AddTask(TaskModel task)
        {
            try
            {
                using var db = new ApplicationContextDb();
                await db.Tasks.AddAsync(task);
                await db.SaveChangesAsync();
                Log.Information($"Добавлена новая задача: {task}");
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "Ошибка при добавлении задачи в базу данных");
                MessageBox.Show("Ошибка сохранения задачи в базе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, "Неверное состояние объекта при добавлении задачи");
                MessageBox.Show("Ошибка данных! Проверьте корректность ввода.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Неизвестная ошибка при добавлении задачи");
                MessageBox.Show("Произошла непредвиденная ошибка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавляет список задач в базу данных, избегая дубликатов
        /// </summary>
        public static async Task AddTasksWithoutDuplicates(List<TaskModel> newTasks)
        {
            try
            {
                using var db = new ApplicationContextDb();

                // Получаем список существующих задач по имени
                var existingTasks = db.Tasks.Select(t => t.Name).ToHashSet();

                // Фильтруем новые задачи (исключаем дубликаты)
                var tasksToAdd = newTasks.Where(task => !existingTasks.Contains(task.Name)).ToList();

                if (tasksToAdd.Count > 0)
                {
                    db.Tasks.AddRange(tasksToAdd);
                    await db.SaveChangesAsync();
                    Log.Information($"Добавлено {tasksToAdd.Count} задач без дубликатов");
                    MessageBox.Show($"Добавлено {tasksToAdd.Count} новых задач", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Log.Warning("Все задачи уже существуют в базе");
                    MessageBox.Show("Все задачи уже существуют в базе данных", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "Ошибка при массовом добавлении задач");
                MessageBox.Show("Ошибка сохранения нескольких задач в базу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Неизвестная ошибка при добавлении списка задач");
                MessageBox.Show("Произошла непредвиденная ошибка при добавлении задач", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обновляет существующую задачу в базе данных
        /// </summary>
        public static async Task UpdateTask(TaskModel updatedTask)
        {
            try
            {
                using var db = new ApplicationContextDb();
                var existingTask = await db.Tasks.FindAsync(updatedTask.Id);

                if (existingTask != null)
                {
                    // Обновляем данные задачи
                    existingTask.Name = updatedTask.Name;
                    existingTask.Description = updatedTask.Description;
                    existingTask.Deadline = updatedTask.Deadline;
                    existingTask.Status = updatedTask.Status;

                    await db.SaveChangesAsync();
                    Log.Information($"Обновлена задача: {updatedTask}");
                }
                else
                {
                    Log.Warning($"Задача с ID={updatedTask.Id} не найдена");
                    MessageBox.Show("Задача не найдена в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Log.Error(ex, "Конфликт обновления данных");
                MessageBox.Show("Конфликт при обновлении задачи. Попробуйте снова", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Неизвестная ошибка при обновлении задачи");
                MessageBox.Show("Произошла ошибка при обновлении задачи!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Получает список всех задач из базы данных
        /// </summary>
        public static async Task<List<TaskModel>> GetTasks()
        {
            try
            {
                using var db = new ApplicationContextDb();
                var tasks = await db.Tasks.ToListAsync();
                Log.Information($"Загружено {tasks.Count} задач из базы данных");
                return tasks;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при получении списка задач");
                MessageBox.Show("Ошибка загрузки задач!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        /// <summary>
        /// Удаляет задачу из базы данных
        /// </summary>
        public static async Task DeleteTask(TaskModel task)
        {
            try
            {
                using var db = new ApplicationContextDb();
                var taskToDelete = await db.Tasks.FindAsync(task.Id);

                if (taskToDelete != null)
                {
                    db.Tasks.Remove(taskToDelete);
                    await db.SaveChangesAsync();
                    Log.Information($"Удалена задача: {task}");
                }
                else
                {
                    Log.Warning($"Задача с ID={task.Id} не найдена");
                    MessageBox.Show("Задача не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "Ошибка при удалении задачи");
                MessageBox.Show("Ошибка удаления задачи из базы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Неизвестная ошибка при удалении задачи");
                MessageBox.Show("Произошла ошибка при удалении задачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}