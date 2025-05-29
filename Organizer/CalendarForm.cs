using Organizer.Models;

namespace Organizer
{
    public partial class CalendarForm : Form
    {
        private DateTime currentDate;
        private TableLayoutPanel calendarTable;
        private Label monthLabel;
        private Label userNameLabel;
        private TeacherOrganizerContext dbContext;
        private Panel selectedDayPanel = null;
        private Button prevMonthButton;
        private Button nextMonthButton;
        private EventHandler userNameLabelClickHandler;
        private EventHandler avatarClickHandler;

        public CalendarForm()
        {
            InitializeComponent();
            InitializeDatabase();
            InitializeCalendarComponents();
            currentDate = DateTime.Today;
            LoadUserName();
            UpdateCalendar();
            this.Resize += CalendarForm_Resize;
        }

        private void CalendarForm_Resize(object sender, EventArgs e)
        {
            UpdateCalendar();
        }

        private void InitializeDatabase()
        {
            dbContext = new TeacherOrganizerContext();
        }

        private void InitializeCalendarComponents()
        {
            this.Text = "Календарь задач";
            this.Size = new Size(900, 650);
            this.MinimumSize = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.FromArgb(240, 255, 240); // Светло-зеленый фон формы

            // Главный контейнер
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Color.FromArgb(240, 255, 240)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Навигация + имя пользователя
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Дни недели
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Календарь

            // 1. Панель навигации
            var navPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(34, 139, 34), // Лесной зеленый
                Padding = new Padding(0, 10, 0, 0)
            };

            // Контейнер для кнопок и месяца
            var navContainer = new Panel
            {
                Size = new Size(400, 50),
                Location = new Point(10, 10),
                BackColor = Color.Transparent
            };

            // Кнопка "Назад"
            prevMonthButton = new Button
            {
                Text = "◄",
                Size = new Size(50, 40),
                Location = new Point(0, 0),
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(144, 238, 144), // Светло-зеленый
                ForeColor = Color.DarkGreen,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            prevMonthButton.Click += (s, e) => { currentDate = currentDate.AddMonths(-1); UpdateCalendar(); };

            // Надпись месяца
            monthLabel = new Label
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(300, 40),
                Location = new Point(50, 0),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            // Кнопка "Вперед"
            nextMonthButton = new Button
            {
                Text = "►",
                Size = new Size(50, 40),
                Location = new Point(350, 0),
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(144, 238, 144), // Светло-зеленый
                ForeColor = Color.DarkGreen,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            nextMonthButton.Click += (s, e) => { currentDate = currentDate.AddMonths(1); UpdateCalendar(); };

            navContainer.Controls.Add(prevMonthButton);
            navContainer.Controls.Add(monthLabel);
            navContainer.Controls.Add(nextMonthButton);
            navPanel.Controls.Add(navContainer);

            // Имя пользователя справа
            userNameLabel = new Label
            {
                Text = "Петров Алексей",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // Добавляем имя пользователя в navPanel с правильным расположением
            navPanel.Controls.Add(userNameLabel);
            userNameLabel.Location = new Point(navPanel.Width - userNameLabel.Width - 20, 15);

            // 2. Заголовки дней недели
            var weekdaysPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                BackColor = Color.FromArgb(50, 205, 50) // Лаймовый зеленый
            };
            for (int i = 0; i < 7; i++)
            {
                weekdaysPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28f));
                var lblDay = new Label
                {
                    Text = GetWeekdayShortName(i),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                weekdaysPanel.Controls.Add(lblDay, i, 0);
            }

            // 3. Таблица календаря
            calendarTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 6,
                BackColor = Color.FromArgb(240, 255, 240), // Светло-зеленый фон
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Margin = new Padding(5)
            };
            for (int i = 0; i < 7; i++)
            {
                calendarTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28f));
            }
            for (int i = 0; i < 6; i++)
            {
                calendarTable.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66f));
            }

            // Собираем все вместе
            mainPanel.Controls.Add(navPanel, 0, 0);
            mainPanel.Controls.Add(weekdaysPanel, 0, 1);
            mainPanel.Controls.Add(calendarTable, 0, 2);

            this.Controls.Add(mainPanel);
        }
        private Image GetUserAvatar()
        {
            var user = dbContext.UserProfiles.FirstOrDefault();
            if (user == null) return CreateDefaultAvatar(null);

            // Если есть путь к аватару и файл существует
            if (!string.IsNullOrEmpty(user.AvatarPath) && System.IO.File.Exists(user.AvatarPath))
            {
                try
                {
                    return Image.FromFile(user.AvatarPath);
                }
                catch
                {
                    // Если не удалось загрузить, вернуть аватар по умолчанию
                    return CreateDefaultAvatar(user);
                }
            }

            // Если нет аватара, создать по умолчанию
            return CreateDefaultAvatar(user);
        }
        private Image CreateDefaultAvatar(UserProfile user)
        {
            var bmp = new Bitmap(40, 40);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(Brushes.LightGray, 0, 0, 39, 39);

                string initials = user != null ? $"{user.LastName?[0]}{user.FirstName?[0]}".ToUpper() : "?";
                using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
                {
                    var size = g.MeasureString(initials, font);
                    g.DrawString(initials, font, Brushes.DarkGreen,
                        (bmp.Width - size.Width) / 2,
                        (bmp.Height - size.Height) / 2);
                }
            }
            return bmp;
        }

        private void LoadUserName()
        {
            var user = dbContext.UserProfiles.FirstOrDefault();

            // Удаляем старые элементы и обработчики
            foreach (Control c in userNameLabel.Parent.Controls.OfType<PictureBox>().ToList())
            {
                if (avatarClickHandler != null)
                {
                    c.Click -= avatarClickHandler;
                }
                userNameLabel.Parent.Controls.Remove(c);
            }

            // Удаляем старый обработчик с метки
            if (userNameLabelClickHandler != null)
            {
                userNameLabel.Click -= userNameLabelClickHandler;
            }

            if (user != null)
            {
                userNameLabel.Text = $"{user.LastName} {user.FirstName}";

                // Добавляем аватарку
                var avatarBox = new PictureBox
                {
                    Image = GetUserAvatar(),
                    Size = new Size(40, 40),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };

                // Позиционирование
                avatarBox.Location = new Point(
                    userNameLabel.Parent.Width - avatarBox.Width - 20,
                    (userNameLabel.Parent.Height - avatarBox.Height) / 2);

                userNameLabel.Location = new Point(
                    avatarBox.Left - userNameLabel.Width - 10,
                    (userNameLabel.Parent.Height - userNameLabel.Height) / 2);

                // Создаем и добавляем новые обработчики
                avatarClickHandler = (s, e) => ShowUserProfileForm(user);
                userNameLabelClickHandler = (s, e) => ShowUserProfileForm(user);

                avatarBox.Click += avatarClickHandler;
                userNameLabel.Click += userNameLabelClickHandler;

                userNameLabel.Parent.Controls.Add(avatarBox);
            }
            else
            {
                userNameLabel.Text = "Регистрация";
                userNameLabel.Location = new Point(
                    userNameLabel.Parent.Width - userNameLabel.Width - 20,
                    (userNameLabel.Parent.Height - userNameLabel.Height) / 2);

                userNameLabelClickHandler = (s, e) => ShowUserProfileForm(null);
                userNameLabel.Click += userNameLabelClickHandler;
            }
        }

        private void ShowUserProfileForm(UserProfile user)
        {
            using (var form = new UserProfileForm(dbContext, user))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadUserName(); // Обновляем интерфейс
                }
            }
        }

        private string GetWeekdayShortName(int index)
        {
            string[] days = { "Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб" };
            return days[index];
        }

        private void UpdateCalendar()
        {
            monthLabel.Text = currentDate.ToString("MMMM yyyy").ToUpper();

            // Сохраняем текущий выбранный день
            DateTime? selectedDate = selectedDayPanel?.Tag as DateTime?;

            calendarTable.SuspendLayout();
            calendarTable.Controls.Clear();

            DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            int dayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            // Заполняем пустые ячейки перед первым днем месяца
            for (int i = 0; i < dayOfWeek; i++)
            {
                var emptyPanel = new Panel
                {
                    BackColor = Color.Transparent,
                    Dock = DockStyle.Fill
                };
                calendarTable.Controls.Add(emptyPanel);
            }

            // Заполняем дни месяца
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(currentDate.Year, currentDate.Month, day);
                var dayPanel = CreateDayPanel(day, date);

                // Восстанавливаем выделение, если это выбранный день
                if (selectedDate.HasValue && date.Date == selectedDate.Value.Date)
                {
                    dayPanel.BackColor = Color.FromArgb(152, 251, 152); // Пастельный зеленый
                    selectedDayPanel = dayPanel;
                }

                calendarTable.Controls.Add(dayPanel);
            }

            // Заполняем оставшиеся ячейки
            int totalCells = 42; // 6 строк * 7 столбцов
            int filledCells = dayOfWeek + daysInMonth;
            for (int i = filledCells; i < totalCells; i++)
            {
                var emptyPanel = new Panel
                {
                    BackColor = Color.Transparent,
                    Dock = DockStyle.Fill
                };
                calendarTable.Controls.Add(emptyPanel);
            }

            calendarTable.ResumeLayout();
        }

        private Panel CreateDayPanel(int day, DateTime date)
        {
            var dayPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                Tag = date,
                Cursor = Cursors.Hand,
                BackColor = Color.White, // Белый фон для лучшей читаемости
                BorderStyle = BorderStyle.FixedSingle // Добавляем границу
            };

            // Если это сегодня - светло-голубой фон с темной границей
            if (date.Date == DateTime.Today.Date)
            {
                dayPanel.BackColor = Color.FromArgb(173, 216, 230);
                dayPanel.BorderStyle = BorderStyle.Fixed3D;
            }

            // Метка с числом - делаем более заметной
            var dayLabel = new Label
            {
                Text = day.ToString(),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 2, 5, 0),
                Font = new Font("Segoe UI", 10, FontStyle.Bold), // Жирный шрифт
                ForeColor = Color.Black // Черный цвет для лучшей видимости
            };

            // Проверяем задачи на этот день
            var startOfDayUtc = date.Date.ToUniversalTime();
            var endOfDayUtc = startOfDayUtc.AddDays(1);
            int tasksCount = dbContext.Tasks.Count(t => t.CreatedAt.HasValue &&
                t.CreatedAt.Value.ToUniversalTime() >= startOfDayUtc &&
                t.CreatedAt.Value.ToUniversalTime() < endOfDayUtc);

            if (tasksCount > 0)
            {
                var tasksLabel = new Label
                {
                    Text = $"Задач: {tasksCount}",
                    Dock = DockStyle.Bottom,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(0, 80, 0), // Темно-зеленый
                    Font = new Font("Segoe UI", 8, FontStyle.Bold), // Жирный шрифт
                    BackColor = Color.FromArgb(220, 255, 220) // Светло-зеленый фон
                };
                dayPanel.Controls.Add(tasksLabel);
            }

            dayPanel.Controls.Add(dayLabel);
            dayPanel.Click += (s, e) => SelectDay(dayPanel, date);

            return dayPanel;
        }

        private void SelectDay(Panel dayPanel, DateTime date)
        {
            // Сбрасываем предыдущий выбор
            if (selectedDayPanel != null)
            {
                if (((DateTime)selectedDayPanel.Tag).Date == DateTime.Today.Date)
                {
                    selectedDayPanel.BackColor = Color.FromArgb(173, 216, 230);
                    selectedDayPanel.BorderStyle = BorderStyle.Fixed3D;
                }
                else
                {
                    selectedDayPanel.BackColor = Color.White;
                    selectedDayPanel.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            // Выделяем текущий день
            dayPanel.BackColor = Color.FromArgb(200, 255, 200); // Яркий светло-зеленый
            dayPanel.BorderStyle = BorderStyle.Fixed3D; // Объемная граница
            selectedDayPanel = dayPanel;

            ShowTasksForDate(date);
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