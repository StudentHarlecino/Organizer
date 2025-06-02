using Organizer.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Organizer
{
    // Форма для просмотра и управления задачами на выбранную дату
    public partial class TasksForm : Form
    {
        private readonly TeacherOrganizerContext _dbContext;
        private readonly DateTime _selectedDate;
        private bool _hasTasks = false;

        // Инициализация формы с передачей контекста БД и выбранной даты
        public TasksForm(TeacherOrganizerContext dbContext, DateTime selectedDate)
        {
            _dbContext = dbContext;
            _selectedDate = selectedDate;
            InitializeComponent();
            InitializeForm();
            LoadTasks();
        }

        // Настройка основных параметров формы
        private void InitializeForm()
        {
            Text = $"Задачи на {_selectedDate.ToShortDateString()}";
            Size = new Size(800, 500);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 255, 240);
            Font = new Font("Segoe UI", 10);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            Controls.Add(mainPanel);
        }

        // Загрузка задач из базы данных
        private void LoadTasks()
        {
            Controls[0].Controls.Clear();

            var startOfDayUtc = _selectedDate.Date.ToUniversalTime();
            var endOfDayUtc = startOfDayUtc.AddDays(1);

            // Получение задач с учетом категорий и файлов
            var tasks = _dbContext.Tasks
                .Include(t => t.Category)
                .Include(t => t.Files)
                .Where(t => t.CreatedAt >= startOfDayUtc && t.CreatedAt < endOfDayUtc)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.Completed)
                .ToList();

            _hasTasks = tasks.Any();
            if (_hasTasks) ShowTasksListView(tasks);
            else ShowNoTasksView();
        }

        // Отображение сообщения при отсутствии задач
        private void ShowNoTasksView()
        {
            var mainPanel = Controls[0];
            var centerPanel = new Panel
            {
                Size = new Size(400, 150),
                Location = new Point((mainPanel.Width - 400) / 2, (mainPanel.Height - 150) / 2),
                Anchor = AnchorStyles.None
            };

            var messageLabel = new Label
            {
                Text = "Нет задач на выбранный день",
                Font = new Font("Segoe UI", 14, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = false,
                Size = new Size(400, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var addButton = new Button
            {
                Text = "Добавить задачу",
                Size = new Size(200, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(50, 205, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(100, 60)
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) => AddNewTask();

            centerPanel.Controls.Add(messageLabel);
            centerPanel.Controls.Add(addButton);
            mainPanel.Controls.Add(centerPanel);
        }

        // Отображение списка задач в виде таблицы
        private void ShowTasksListView(List<Models.Task> tasks)
        {
            var mainPanel = Controls[0];
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                RowStyles = {
                    new RowStyle(SizeType.Percent, 100),
                    new RowStyle(SizeType.Absolute, 60)
                }
            };

            var tasksListView = CreateTasksListView(tasks);
            var buttonPanel = CreateButtonPanel();

            tableLayout.Controls.Add(tasksListView, 0, 0);
            tableLayout.Controls.Add(buttonPanel, 0, 1);
            mainPanel.Controls.Add(tableLayout);
        }

        // Создание и настройка ListView для отображения задач
        private ListView CreateTasksListView(List<Models.Task> tasks)
        {
            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                Font = new Font("Segoe UI", 9)
            };

            // Настройка колонок
            listView.Columns.AddRange(new[]
            {
                new ColumnHeader { Text = "Статус", Width = 100 },
                new ColumnHeader { Text = "Приоритет", Width = 80 },
                new ColumnHeader { Text = "Категория", Width = 120 },
                new ColumnHeader { Text = "Название", Width = 200 },
                new ColumnHeader { Text = "Дедлайн", Width = 100 },
                new ColumnHeader { Text = "Файлы", Width = 150 }
            });

            listView.DoubleClick += (s, e) => EditSelectedTask(listView);

            // Заполнение данными
            foreach (var task in tasks)
            {
                var item = new ListViewItem(GetTaskStatusText(task))
                {
                    Tag = task.TaskId,
                    SubItems = {
                        GetPriorityName(task.Priority),
                        task.Category?.Name ?? "Без категории",
                        task.Title,
                        task.DeadlineDate?.ToString("dd.MM.yyyy") ?? "Нет",
                        task.Files.Any() ? $"{task.Files.Count} файлов" : "Нет файлов"
                    }
                };

                ApplyTaskStyle(item, task);
                listView.Items.Add(item);
            }

            return listView;
        }

        // Определение текста статуса задачи
        private string GetTaskStatusText(Models.Task task)
        {
            if (task.Completed) return "✓ Выполнена";
            if (task.DeadlineDate.HasValue && task.DeadlineDate.Value.ToDateTime(TimeOnly.MinValue) < DateTime.Today)
                return "⚠ Просрочено";
            return "⏳ В работе";
        }

        // Применение стилей к задаче в зависимости от статуса
        private void ApplyTaskStyle(ListViewItem item, Models.Task task)
        {
            if (task.Completed)
            {
                item.BackColor = Color.FromArgb(220, 255, 220);
                item.Font = new Font(item.Font, FontStyle.Strikeout);
                return;
            }

            if (!task.DeadlineDate.HasValue) return;

            var daysUntilDeadline = (task.DeadlineDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Today);
            if (daysUntilDeadline.TotalDays <= 1 && daysUntilDeadline.TotalDays >= 0)
            {
                item.BackColor = Color.FromArgb(220, 53, 69);
                item.Font = new Font(item.Font, FontStyle.Bold);
            }
            else if (daysUntilDeadline.TotalDays < 0)
            {
                item.BackColor = Color.FromArgb(150, 150, 150);
                item.Font = new Font(item.Font, FontStyle.Italic);
            }
        }

        // Создание панели с кнопками действий
        private FlowLayoutPanel CreateButtonPanel()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };

            var closeButton = new Button
            {
                Text = "Закрыть",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            closeButton.FlatAppearance.BorderSize = 0;

            var addButton = new Button
            {
                Text = "Добавить задачу",
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(50, 205, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) => AddNewTask();

            panel.Controls.Add(closeButton);
            panel.Controls.Add(addButton);
            return panel;
        }

        // Получение названия приоритета по его числовому значению
        private string GetPriorityName(int priority)
        {
            return priority switch
            {
                1 => "Низкий",
                2 => "Средний",
                3 => "Высокий",
                _ => "Не указан"
            };
        }

        // Редактирование выбранной задачи
        private void EditSelectedTask(ListView listView)
        {
            if (listView.SelectedItems.Count == 0 || listView.SelectedItems[0].Tag == null) return;

            int taskId = (int)listView.SelectedItems[0].Tag;
            var task = _dbContext.Tasks
                .Include(t => t.Category)
                .Include(t => t.Files)
                .FirstOrDefault(t => t.TaskId == taskId);

            using (var form = new TaskEditForm(_dbContext, task))
            {
                var result = form.ShowDialog();
                if (result != DialogResult.Cancel) LoadTasks();
            }
        }

        // Добавление новой задачи
        private void AddNewTask()
        {
            var newTask = new Models.Task
            {
                CreatedAt = _selectedDate.Date.ToUniversalTime(),
                Title = "Новая задача",
                Priority = 2,
                Completed = false
            };

            _dbContext.Tasks.Add(newTask);

            using (var form = new TaskEditForm(_dbContext, newTask))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try { _dbContext.SaveChanges(); LoadTasks(); }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении задачи: {ex.InnerException?.Message ?? ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    _dbContext.Entry(newTask).State = EntityState.Detached;
                }
            }
        }
    }
}