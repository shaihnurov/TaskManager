using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;
using TaskManager.Model;
using TaskManager.Services;

namespace TaskManager.ViewModel
{
    /// <summary>
    /// ViewModel для добавления новой задачи.
    /// </summary>
    public partial class AddTaskViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TaskListViewModel _taskListViewModel;

        #region ObservableProperty
        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private string? _description;

        [ObservableProperty]
        private DateTime _deadline = DateTime.Now.AddDays(1);

        [ObservableProperty]
        private string? _selectedStatus = "В процессе";

        [ObservableProperty]
        private ObservableCollection<string> _statusList = ["В процессе", "Завершено"];
        #endregion

        // Доступные статусы задач

        public AddTaskViewModel(MainViewModel mainViewModel, TaskListViewModel taskListViewModel)
        {
            _mainViewModel = mainViewModel;
            _taskListViewModel = taskListViewModel;
        }

        [RelayCommand]
        private async Task AddTask()
        {
            try
            {
                // Проверяем, заполнены ли обязательные поля
                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Description) || string.IsNullOrWhiteSpace(SelectedStatus))
                {
                    MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newTask = new TaskModel
                {
                    Name = Name,
                    Description = Description,
                    Deadline = Deadline,
                    CreateDate = DateTime.Now,
                    Status = SelectedStatus
                };

                // Добавляем задачу в базу данных
                await TaskService.AddTask(newTask);

                // Добавляем задачу в коллекции списка (для обновления UI)
                _taskListViewModel.Tasks.Add(newTask);

                // Переключаем представление на список задач
                _mainViewModel.CurrentView = _taskListViewModel;

                // Очищаем поля формы после добавления
                Name = string.Empty;
                Description = string.Empty;
                Deadline = DateTime.Now.AddDays(1);
                SelectedStatus = "В процессе";
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show("Ошибка при сохранении задачи в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error($"DbUpdateException: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Ошибка валидации данных. Проверьте введённые значения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                Log.Error($"InvalidOperationException: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла непредвиденная ошибка. Попробуйте снова", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error($"Exception: {ex.Message}");
            }
        }
    }
}