using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TaskManager.Model;

namespace TaskManager.ViewModel
{
    public partial class TasksStatisticsViewModel : ObservableObject
    {
        #region ObservableProperty
        [ObservableProperty]
        private int _totalTasks;

        [ObservableProperty]
        private int _tasksInProgress;

        [ObservableProperty]
        private int _completedTasks;

        [ObservableProperty]
        private int _tasksDueIn24Hours;

        [ObservableProperty]
        private double _completionPercentage;
        #endregion

        public TasksStatisticsViewModel(ObservableCollection<TaskModel> taskModels)
        {
            UpdateStatistics(taskModels);
        }

        //Метод для рассчета статистики
        public void UpdateStatistics(ObservableCollection<TaskModel> taskModels)
        {
            if (taskModels == null || taskModels.Count == 0)
            {
                TotalTasks = 0;
                TasksInProgress = 0;
                CompletedTasks = 0;
                TasksDueIn24Hours = 0;
                CompletionPercentage = 0;
                return;
            }

            TotalTasks = taskModels.Count;
            TasksInProgress = taskModels.Count(t => t.Status == "В процессе");
            CompletedTasks = taskModels.Count(t => t.Status == "Завершено");

            var now = DateTime.Now;
            var nextDay = now.AddHours(24);
            TasksDueIn24Hours = taskModels.Count(t => t.Deadline <= nextDay && t.Deadline >= now && t.Status != "Завершено");

            CompletionPercentage = TotalTasks > 0 ? (CompletedTasks / (double)TotalTasks) * 100 : 0;
        }
    }
}