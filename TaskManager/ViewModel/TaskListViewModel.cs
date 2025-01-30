using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskManager.Model;
using TaskManager.Services;

namespace TaskManager.ViewModel
{
    public partial class TaskListViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        #region ObservableProperty
        [ObservableProperty]
        private ObservableCollection<TaskModel> _tasks = []; // Полный список задач

        [ObservableProperty]
        private ObservableCollection<TaskModel> _filteredTasks = []; // Отфильтрованный список задач

        [ObservableProperty]
        private ObservableCollection<string> _statusList = ["Все", "В процессе", "Завершено"];
        #endregion

        #region Свойства для фильтрации
        private string? _selectedName;
        private string? _selectedStatus = "Все";
        private DateTime? _selectedCreationDate = null;
        private DateTime? _selectedDeadline = null;

        public string? SelectedName
        {
            get => _selectedName;
            set
            {
                SetProperty(ref _selectedName, value);
                FilterTasks();
            }
        }
        public string? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                SetProperty(ref _selectedStatus, value);
                FilterTasks();
            }
        }
        public DateTime? SelectedCreationDate
        {
            get => _selectedCreationDate;
            set
            {
                SetProperty(ref _selectedCreationDate, value);
                FilterTasks();
            }
        }
        public DateTime? SelectedDeadline
        {
            get => _selectedDeadline;
            set
            {
                SetProperty(ref _selectedDeadline, value);
                FilterTasks();
            }
        }
        #endregion

        public TaskListViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _ = LoadTasks();
        }

        internal async Task LoadTasks()
        {
            var tasks = await TaskService.GetTasks();

            Tasks.Clear();
            foreach (var task in tasks)
            {
                Tasks.Add(task);
            }

            FilterTasks();
        }
        private void FilterTasks()
        {
            if (Tasks == null) return;

            var filtered = Tasks.AsEnumerable();

            // Фильтрация по имени
            if (!string.IsNullOrEmpty(SelectedName))
            {
                filtered = filtered.Where(task => task.Name!.Contains(SelectedName, StringComparison.OrdinalIgnoreCase));
            }

            // Фильтрация по статусу
            if (SelectedStatus != "Все")
            {
                filtered = filtered.Where(task => task.Status!.Equals(SelectedStatus, StringComparison.OrdinalIgnoreCase));
            }

            // Фильтрация по дате создания
            if (SelectedCreationDate.HasValue)
            {
                filtered = filtered.Where(task => task.CreateDate.Date == SelectedCreationDate.Value.Date);
            }

            // Фильтрация по дедлайну
            if (SelectedDeadline.HasValue)
            {
                filtered = filtered.Where(task => task.Deadline.Date == SelectedDeadline.Value.Date);
            }

            FilteredTasks.Clear();
            foreach (var task in filtered)
            {
                FilteredTasks.Add(task);
            }
        }
        [RelayCommand]
        private async Task DeleteTask(object parameter)
        {
            if (parameter is TaskModel currentTask)
            {
                // Очищаем ссылки на объект перед удалением
                currentTask.Name = null;
                currentTask.Description = null;
                currentTask.Status = null;

                Tasks.Remove(currentTask);
                FilteredTasks.Remove(currentTask);

                await TaskService.DeleteTask(currentTask);

                // Запускаем сборщик мусора вручную
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }
        [RelayCommand]
        private void EditTask(object parameter)
        {
            if (parameter is TaskModel currentTask)
            {
                EditTaskViewModel editTaskViewModel = new(currentTask, _mainViewModel, this);
                _mainViewModel.CurrentView = editTaskViewModel;
            }
        }
        public void UpdateTask(TaskModel updatedTask)
        {
            if (updatedTask == null) return;

            // Ищем задачу в коллекции по Id
            var existingTask = Tasks.FirstOrDefault(t => t.Id == updatedTask.Id);

            if (existingTask != null)
            {
                // Обновляем свойства
                existingTask.Name = updatedTask.Name;
                existingTask.Description = updatedTask.Description;
                existingTask.Status = updatedTask.Status;
                existingTask.CreateDate = updatedTask.CreateDate;
                existingTask.Deadline = updatedTask.Deadline;
            }

            // Обновляем отфильтрованный список
            FilterTasks();
        }
    }
}