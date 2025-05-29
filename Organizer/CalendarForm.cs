using Organizer.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Organizer
{
    public partial class CalendarForm : Form
    {
        private DateTime currentDate;
        private TableLayoutPanel calendarTable;
        private Label monthLabel;
        private TeacherOrganizerContext dbContext;

        public CalendarForm()
        {
            InitializeComponent();
            InitializeDatabase();
            InitializeCalendarComponents();
            currentDate = DateTime.Today;
            UpdateCalendar();
        }

        private void InitializeDatabase()
        {
            dbContext = new TeacherOrganizerContext();
        }

        private void InitializeCalendarComponents()
        {
            // Настройка формы
            this.Text = "Календарь задач";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.White;

            // Главный контейнер
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(10),
                BackColor = Color.White
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Навигация
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Заголовки дней недели
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Календарь

            // Панель навигации по месяцам
            var navigationPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(10),
                BackColor = Color.LightSteelBlue
            };

            var prevMonthButton = new Button
            {
                Text = "◄",
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Margin = new Padding(5)
            };
            prevMonthButton.Click += (s, e) => { currentDate = currentDate.AddMonths(-1); UpdateCalendar(); };

            monthLabel = new Label
            {
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue,
                Margin = new Padding(20, 10, 20, 10)
            };

            var nextMonthButton = new Button
            {
                Text = "►",
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Margin = new Padding(5)
            };
            nextMonthButton.Click += (s, e) => { currentDate = currentDate.AddMonths(1); UpdateCalendar(); };

            navigationPanel.Controls.Add(prevMonthButton);
            navigationPanel.Controls.Add(monthLabel);
            navigationPanel.Controls.Add(nextMonthButton);

            // Заголовки дней недели
            var weekdaysPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Height = 40,
                ColumnCount = 7,
                BackColor = Color.SteelBlue

            };

            for (int i = 0; i < 7; i++)
            {
                weekdaysPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28f));

                var lblDay = new Label()
                {
                    Text = GetWeekdayShortName(i),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                weekdaysPanel.Controls.Add(lblDay, i, 0);
            }

            // Таблица календаря
            calendarTable = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 6,
                BackColor = Color.WhiteSmoke,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            for (int i = 0; i < 7; i++)
            {
                calendarTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28f));
            }
            for (int i = 0; i < 6; i++)
            {
                calendarTable.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66f));
            }

            // Собираем все вместе в главный контейнер
            mainPanel.Controls.Add(navigationPanel, 0, 0);
            mainPanel.Controls.Add(weekdaysPanel, 0, 1);
            mainPanel.Controls.Add(calendarTable, 0, 2);

            this.Controls.Add(mainPanel);
        }

        private string GetWeekdayShortName(int index)
        {
            string[] days = { "Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб" };
            return days[index];
        }

        private void UpdateCalendar()
        {
            monthLabel.Text = currentDate.ToString("MMMM yyyy").ToUpper();

            calendarTable.Controls.Clear();

            DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            int dayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            // Добавляем пустые ячейки перед первым днем месяца
            for (int i = 0; i < dayOfWeek; i++)
            {
                var emptyCell = new Panel() { BackColor = Color.Transparent };
                calendarTable.Controls.Add(emptyCell);
            }

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime dateLocal = new DateTime(currentDate.Year, currentDate.Month, day);

                var dayButton = new Button()
                {
                    Text = day.ToString(),
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2),
                    Tag = dateLocal,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    Font = new Font("Segoe UI", 10)
                };

                if (dateLocal.Date == DateTime.Today.Date)
                {
                    dayButton.BackColor = Color.LightSkyBlue;
                    dayButton.Font = new Font(dayButton.Font, (FontStyle.Bold));
                }

                // Подсчет задач на день (используем UTC границы)
                var startOfDayUtc = dateLocal.Date.ToUniversalTime();
                var endOfDayUtc = startOfDayUtc.AddDays(1);

                int tasksCount = dbContext.Tasks.Count(t =>
                    t.CreatedAt.HasValue &&
                    t.CreatedAt.Value.ToUniversalTime() >= startOfDayUtc &&
                    t.CreatedAt.Value.ToUniversalTime() < endOfDayUtc);

                if (tasksCount > 0)
                {
                    dayButton.Text += $"\n({tasksCount} задач)";
                    dayButton.ForeColor = Color.DarkGreen;
                    dayButton.Font = new Font(dayButton.Font.FontFamily, dayButton.Font.Size - 1);
                }

                dayButton.Click += DayButton_Click;

                calendarTable.Controls.Add(dayButton);
            }
        }

        private void DayButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            DateTime selectedDate = (DateTime)button.Tag;

            foreach (Control ctrl in calendarTable.Controls)
            {
                if (ctrl is Button btn)
                {
                    if (btn.Tag is DateTime dt && dt.Date == selectedDate.Date)
                    {
                        // Выделяем выбранный день
                        btn.BackColor = Color.LightSteelBlue;
                    }
                    else if (btn.Tag is DateTime dtOther && dtOther.Date == DateTime.Today.Date)
                    {
                        // Восстанавливаем цвет для сегодняшнего дня
                        btn.BackColor = Color.LightSkyBlue;
                    }
                    else
                    {
                        // Восстанавливаем стандартный цвет
                        btn.BackColor = Color.White;
                    }
                }
            }

            ShowTasksForDate(selectedDate);
        }

        private void ShowTasksForDate(DateTime date)
        {
            MessageBox.Show($"Выбрана дата: {date.ToShortDateString()}\n" +
                $"Здесь будут отображаться задачи на этот день",
                "Задачи на день",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}