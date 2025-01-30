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
    /// ViewModel для редактирования существующей задачи
    /// </summary>
    public partial class EditTaskViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TaskListViewModel _taskListViewModel;

        #region ObservableProperty
        private readonly int _id;
        private readonly DateTime _createDate;

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private string? _description;

        [ObservableProperty]
        private DateTime _deadline;

        [ObservableProperty]
        private ObservableCollection<string> _statusList = ["В процессе", "Завершено"];

        [ObservableProperty]
        private string? _selectedStatus;
        #endregion

        public EditTaskViewModel(TaskModel task, MainViewModel mainViewModel, TaskListViewModel taskListViewModel)
        {
            _mainViewModel = mainViewModel;
            _taskListViewModel = taskListViewModel;

            _id = task.Id;
            _createDate = task.CreateDate;
            Name = task.Name;
            Description = task.Description;
            Deadline = task.Deadline;

            // Устанавливаем выбранный статус из переданной задачи
            SelectedStatus = task.Status;
        }

        [RelayCommand]
        private async Task UpdateTask()
        {
            try
            {
                // Проверяем, заполнены ли все обязательные поля
                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Description) || string.IsNullOrWhiteSpace(SelectedStatus))
                {
                    MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var updatedTask = new TaskModel
                {
                    Id = _id,
                    Name = Name,
                    Description = Description,
                    Deadline = Deadline,
                    Status = SelectedStatus, // Передаём выбранный статус как строку
                    CreateDate = _createDate,
                };

                // Отправляем изменения в базу данных
                await TaskService.UpdateTask(updatedTask);

                // Обновляем задачу в списке, обновление UI
                _taskListViewModel.UpdateTask(updatedTask);

                // Переключаемся обратно на список задач
                _mainViewModel.CurrentView = _taskListViewModel;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                MessageBox.Show("Ошибка обновления задачи. Попробуйте снова", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                Log.Error($"DbUpdateConcurrencyException: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show("Ошибка сохранения изменений в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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