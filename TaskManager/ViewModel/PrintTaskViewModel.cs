using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using TaskManager.Model;

namespace TaskManager.ViewModel
{
    public partial class PrintTaskViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<TaskModel> _filteredTasks;

        public PrintTaskViewModel(ObservableCollection<TaskModel> filteredTasks)
        {
            FilteredTasks = filteredTasks;
        }

        // Команда для печати
        [RelayCommand]
        public async Task PrintAsync()
        {
            try
            {
                // Открытие диалогового окна печати
                PrintDialog printDialog = new();

                // Если пользователь выбрал принтер и подтвердил печать
                if (printDialog.ShowDialog() == true)
                {
                    // Создаем документ с задачами для печати
                    FixedDocument document = await Application.Current.Dispatcher.InvokeAsync(() => CreatePrintDocument(printDialog));

                    // Отправляем документ на печать
                    await Task.Run(() =>
                    {
                        // Печать документа на выбранный принтер
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            printDialog.PrintDocument(document.DocumentPaginator, "Список задач");
                        });
                    });
                }
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ex, "Ошибка: передан пустой объект");
                MessageBox.Show("Ошибка при передаче данных. Пожалуйста, проверьте объекты для печати", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, "Ошибка при печати документа");
                MessageBox.Show("Произошла ошибка при печати документа. Возможно, проблемы с принтером.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                Log.Error(ex, "Ошибка при доступе к файлу");
                MessageBox.Show("Ошибка при доступе к файлу для печати. Пожалуйста, попробуйте снова", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex, "Ошибка доступа");
                MessageBox.Show("Недостаточно прав для печати. Пожалуйста, проверьте настройки доступа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Ошибка при запросе к серверу");
                MessageBox.Show("Произошла ошибка при подключении к серверу. Проверьте подключение к интернету", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Неизвестная ошибка");
                MessageBox.Show("Произошла неизвестная ошибка. Пожалуйста, попробуйте снова", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для создания документа для печати
        private FixedDocument CreatePrintDocument(PrintDialog printDialog)
        {
            try
            {
                // Создаем документ для печати
                FixedDocument document = new();
                FixedPage page = new()
                {
                    Width = printDialog.PrintableAreaWidth,
                    Height = printDialog.PrintableAreaHeight
                };

                // Создаем таблицу для печати
                Table table = new() { CellSpacing = 5 };
                table.Columns.Add(new TableColumn()); // Название задачи
                table.Columns.Add(new TableColumn()); // Описание
                table.Columns.Add(new TableColumn()); // Дата создания
                table.Columns.Add(new TableColumn()); // Дедлайн
                table.Columns.Add(new TableColumn()); // Статус

                // Добавляем заголовок таблицы
                TableRowGroup headerGroup = new();
                TableRow headerRow = new();
                headerRow.Cells.Add(CreateCell("Название", true));
                headerRow.Cells.Add(CreateCell("Описание", true));
                headerRow.Cells.Add(CreateCell("Дата создания", true));
                headerRow.Cells.Add(CreateCell("Дедлайн", true));
                headerRow.Cells.Add(CreateCell("Статус", true));
                headerGroup.Rows.Add(headerRow);
                table.RowGroups.Add(headerGroup);

                // Добавляем данные задач в таблицу
                TableRowGroup dataGroup = new();
                foreach (var task in FilteredTasks)
                {
                    TableRow row = new();
                    row.Cells.Add(CreateCell(task.Name));
                    row.Cells.Add(CreateCell(task.Description));
                    row.Cells.Add(CreateCell(task.CreateDate.ToString("dd.MM.yyyy")));
                    row.Cells.Add(CreateCell(task.Deadline.ToString("dd.MM.yyyy")));
                    row.Cells.Add(CreateCell(task.Status));
                    dataGroup.Rows.Add(row);
                }
                table.RowGroups.Add(dataGroup);

                // Добавляем таблицу в документ
                FlowDocument flowDocument = new();
                flowDocument.Blocks.Add(table);

                // Вставляем документ в страницу
                RichTextBox richTextBox = new() { Document = flowDocument, Width = printDialog.PrintableAreaWidth };
                page.Children.Add(richTextBox);

                // Создаем страницу и добавляем её в документ
                PageContent pageContent = new();
                ((IAddChild)pageContent).AddChild(page);
                document.Pages.Add(pageContent);

                return document;
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ex, "Ошибка: пустой объект при создании документа");
                throw new InvalidOperationException("Ошибка при создании документа. Пустой объект", ex);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, "Ошибка при создании документа");
                throw new InvalidOperationException("Ошибка при создании документа", ex);
            }
            catch (IOException ex)
            {
                Log.Error(ex, "Ошибка при создании документа (доступ к файлу)");
                throw new IOException("Ошибка при создании документа (доступ к файлу)", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при создании документа");
                throw new InvalidOperationException("Неизвестная ошибка при создании документа", ex);
            }
        }

        // Метод для создания ячейки таблицы для печати
        private static TableCell CreateCell(string? text, bool isHeader = false)
        {
            try
            {
                return new TableCell(new Paragraph(new Run(text ?? "")))
                {
                    FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                    BorderThickness = new Thickness(1),
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    Padding = new Thickness(5)
                };
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex, "Ошибка при создании ячейки таблицы");
                throw new InvalidOperationException("Ошибка при создании ячейки таблицы (неверный формат данных)", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при создании ячейки таблицы");
                MessageBox.Show("Произошла ошибка при создании ячейки таблицы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}