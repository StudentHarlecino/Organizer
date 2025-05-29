using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using Organizer.Models;

namespace Organizer
{
    public partial class UserProfileForm : Form
    {
        private readonly TeacherOrganizerContext _dbContext;
        private readonly UserProfile _userProfile;
        private PictureBox avatarPictureBox;
        private string _avatarPath;

        public UserProfileForm(TeacherOrganizerContext dbContext, UserProfile userProfile = null)
        {
            InitializeComponent();
            _dbContext = dbContext;
            _userProfile = userProfile;
            _avatarPath = userProfile?.AvatarPath;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Редактирование профиля";
            this.Size = new Size(400, 420); // Увеличили высоту формы
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.FromArgb(240, 255, 240);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 2,
                RowCount = 7
            };

            // Настройка стилей столбцов и строк
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Основные данные пользователя (4 строки)
            for (int i = 0; i < 4; i++)
            {
                mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            }

            // Строка для аватара
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));

            // Пустое пространство и кнопки
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            // Элементы формы (ФИО и email)
            var lblLastName = new Label { Text = "Фамилия:", TextAlign = ContentAlignment.MiddleRight };
            var txtLastName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5) };

            var lblFirstName = new Label { Text = "Имя:", TextAlign = ContentAlignment.MiddleRight };
            var txtFirstName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5) };

            var lblMiddleName = new Label { Text = "Отчество:", TextAlign = ContentAlignment.MiddleRight };
            var txtMiddleName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5) };

            var lblEmail = new Label { Text = "Email:", TextAlign = ContentAlignment.MiddleRight };
            var txtEmail = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5) };

            // Добавляем элементы на панель (первые 4 строки)
            mainPanel.Controls.Add(lblLastName, 0, 0);
            mainPanel.Controls.Add(txtLastName, 1, 0);
            mainPanel.Controls.Add(lblFirstName, 0, 1);
            mainPanel.Controls.Add(txtFirstName, 1, 1);
            mainPanel.Controls.Add(lblMiddleName, 0, 2);
            mainPanel.Controls.Add(txtMiddleName, 1, 2);
            mainPanel.Controls.Add(lblEmail, 0, 3);
            mainPanel.Controls.Add(txtEmail, 1, 3);

            // Блок аватара (5-я строка)
            var lblAvatar = new Label { Text = "Аватар:", TextAlign = ContentAlignment.MiddleRight };
            avatarPictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                BackColor = Color.White
            };

            var btnChangeAvatar = new Button
            {
                Text = "Выбрать аватар",
                Height = 30,
                Dock = DockStyle.Top,
                Margin = new Padding(5, 5, 5, 2), // Уменьшили нижний отступ
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(0, 0, 0, 0) // Убрали внутренние отступы
            };

            var btnRemoveAvatar = new Button
            {
                Text = "Удалить аватар",
                Height = 30,
                Dock = DockStyle.Top,
                Margin = new Padding(5, 2, 5, 5), // Уменьшили верхний отступ
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(0, 0, 0, 0) // Убрали внутренние отступы
            };

            // Загружаем текущий аватар, если он есть
            if (!string.IsNullOrEmpty(_avatarPath) && System.IO.File.Exists(_avatarPath))
            {
                avatarPictureBox.Image = Image.FromFile(_avatarPath);
            }
            else
            {
                avatarPictureBox.Image = CreateDefaultAvatar(_userProfile);
            }

            var avatarPanel = new Panel { Dock = DockStyle.Fill };
            avatarPanel.Controls.Add(avatarPictureBox);

            var buttonsPanel = new Panel { Dock = DockStyle.Bottom, Height = 70 };
            buttonsPanel.Controls.Add(btnChangeAvatar);
            buttonsPanel.Controls.Add(btnRemoveAvatar);

            avatarPanel.Controls.Add(buttonsPanel);

            mainPanel.Controls.Add(lblAvatar, 0, 4);
            mainPanel.Controls.Add(avatarPanel, 1, 4);

            // Кнопки сохранения/отмены (7-я строка)
            var btnSave = new Button
            {
                Text = "Сохранить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(50, 205, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(2),
                Padding = new Padding(0)
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(2),
                Padding = new Padding(0)
            };

            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 10, 0, 0)
            };
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttonPanel.Controls.Add(btnSave, 0, 0);
            buttonPanel.Controls.Add(btnCancel, 1, 0);

            mainPanel.Controls.Add(buttonPanel, 0, 6);
            mainPanel.SetColumnSpan(buttonPanel, 2);

            // Заполняем данные, если профиль существует
            if (_userProfile != null)
            {
                txtLastName.Text = _userProfile.LastName;
                txtFirstName.Text = _userProfile.FirstName;
                txtMiddleName.Text = _userProfile.MiddleName ?? "";
                txtEmail.Text = _userProfile.Email ?? "";
            }

            // Обработчики событий
            btnChangeAvatar.Click += (s, e) => ChangeAvatar();
            btnRemoveAvatar.Click += (s, e) => RemoveAvatar();

            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text))
                {
                    MessageBox.Show("Фамилия и имя обязательны для заполнения!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var user = _userProfile ?? new UserProfile();
                user.LastName = txtLastName.Text;
                user.FirstName = txtFirstName.Text;
                user.MiddleName = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text;
                user.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text;
                user.AvatarPath = _avatarPath;

                if (_userProfile == null)
                {
                    _dbContext.UserProfiles.Add(user);
                }
                else
                {
                    _dbContext.UserProfiles.Update(user);
                }

                _dbContext.SaveChanges();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.Add(mainPanel);
        }

        private void ChangeAvatar()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";
                openFileDialog.Title = "Выберите аватар";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                        if (fileInfo.Length > 2 * 1024 * 1024)
                        {
                            MessageBox.Show("Размер файла не должен превышать 2MB", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        avatarPictureBox.Image = Image.FromFile(openFileDialog.FileName);
                        _avatarPath = openFileDialog.FileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void RemoveAvatar()
        {
            _avatarPath = null;
            avatarPictureBox.Image = CreateDefaultAvatar(_userProfile);
        }

        private Image CreateDefaultAvatar(UserProfile user)
        {
            var bmp = new Bitmap(100, 100);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(Brushes.LightGray, 0, 0, 99, 99);

                string initials = user != null ? $"{user.LastName?[0]}{user.FirstName?[0]}".ToUpper() : "?";
                using (var font = new Font("Segoe UI", 24, FontStyle.Bold))
                {
                    var size = g.MeasureString(initials, font);
                    g.DrawString(initials, font, Brushes.DarkGreen,
                        (bmp.Width - size.Width) / 2,
                        (bmp.Height - size.Height) / 2);
                }
            }
            return bmp;
        }
    }
}