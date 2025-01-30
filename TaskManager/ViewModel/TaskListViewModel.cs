using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskManager.Model;
using TaskManager.Services;
using Serilog;
using System.Windows;

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
        private bool _showUrgentTasks;
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
        public bool ShowUrgentTasks
        {
            get => _showUrgentTasks;
            set
            {
                SetProperty(ref _showUrgentTasks, value);
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

        //Загрузка задач из БД
        internal async Task LoadTasks()
        {
            try
            {
                var tasks = await TaskService.GetTasks();

                Tasks.Clear();
                foreach (var task in tasks)
                {
                    Tasks.Add(task);
                }

                FilterTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при загрузке задач - {ex.Message}");
                Log.Error(ex, "Ошибка при загрузке задач");
            }
        }
        //Метод для фильтрации списка задач
        private void FilterTasks()
        {
            try
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

                // Фильтрация по "горящим" задачам
                if (ShowUrgentTasks)
                {
                    var now = DateTime.Now;
                    filtered = filtered.Where(task => (task.Deadline - now).TotalHours <= 24 && task.Status == "В процессе");
                }

                FilteredTasks.Clear();
                foreach (var task in filtered)
                {
                    FilteredTasks.Add(task);
                }
            }
            catch (ArgumentNullException nullEx)
            {
                MessageBox.Show($"Ошибка при фильтрации задач: найдено null значение - {nullEx.Message}");
                Log.Error(nullEx, "Ошибка при фильтрации задач: найдено null значение");
            }
            catch (InvalidOperationException invEx)
            {
                MessageBox.Show($"Ошибка при фильтрации задач: некорректная операция - {invEx.Message}");
                Log.Error(invEx, "Ошибка при фильтрации задач: некорректная операция");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при фильтрации задач - {ex.Message}");
                Log.Error(ex, "Ошибка при фильтрации задач");
            }
        }
        //Метод удаления задач
        [RelayCommand]
        private async Task DeleteTask(object parameter)
        {
            try
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
            catch (ArgumentNullException nullEx)
            {
                MessageBox.Show($"Возникла ошибка при удалении задачи: передан null объект задачи - {nullEx.Message}");
                Log.Error(nullEx, "Ошибка при удалении задачи: передан null объект задачи");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при удалении задачи - {ex.Message}");
                Log.Error(ex, "Ошибка при удалении задачи");
            }
        }
        //Метод для вызова View редактирования
        [RelayCommand]
        private void EditTask(object parameter)
        {
            try
            {
                if (parameter is TaskModel currentTask)
                {
                    EditTaskViewModel editTaskViewModel = new(currentTask, _mainViewModel, this);
                    _mainViewModel.CurrentView = editTaskViewModel;
                }
            }
            catch (ArgumentException argEx)
            {
                MessageBox.Show($"Возникла ошибка при редактировании задачи: передан некорректный параметр - {argEx.Message}");
                Log.Error(argEx, "Ошибка при редактировании задачи: передан некорректный параметр");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при редактировании задачи - {ex.Message}");
                Log.Error(ex, "Ошибка при редактировании задачи");
            }
        }
        //Метод для обновления изменённой задачи
        public void UpdateTask(TaskModel updatedTask)
        {
            try
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
            catch (InvalidOperationException invOpEx)
            {
                Log.Error(invOpEx, "Ошибка обновления коллекции");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при обновлении задачи - {ex.Message}");
                Log.Error(ex, "Ошибка при обновлении задачи");
            }
        }
    }
}