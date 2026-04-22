using PolesSU_Sports.Admin;
using PolesSU_Sports.Management;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace PolesSU_Sports
{
    public partial class LoginForm : Form
    {
        private TextBox txtLogin;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnExit;
        private Label lblError;
        private Panel headerPanel;
        private Panel logoPanel;
        private Timer animationTimer;
        private int logoPosition = -100;
        private int headerHeight = 0;
        private Panel mainCard;
        private int cardPosition = 500;

        public LoginForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            StartAnimations();
        }

        private void StartAnimations()
        {
            animationTimer = new Timer();
            animationTimer.Interval = 16; // ~60 FPS
            animationTimer.Tick += AnimationLoop;
            animationTimer.Start();
        }

        private void AnimationLoop(object sender, EventArgs e)
        {
            // Анимация логотипа
            if (logoPosition < 120)
            {
                logoPosition += 8;
                logoPanel.Location = new Point((this.Width - logoPanel.Width) / 2, logoPosition);
            }

            // Анимация шапки
            if (headerHeight < 140)
            {
                headerHeight += 10;
                headerPanel.Height = headerHeight;
                headerPanel.Location = new Point(0, 0);
            }

            // Анимация карточки
            if (cardPosition > 160)
            {
                cardPosition -= 15;
                mainCard.Location = new Point((this.Width - mainCard.Width) / 2, cardPosition);
            }
            else if (logoPosition >= 120 && headerHeight >= 140 && cardPosition <= 160)
            {
                animationTimer.Stop();
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Авторизация - ПолесГУ Спорт";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);

            // ===== ГРАДИЕНТНЫЙ ФОН =====
            this.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(245, 247, 250),
                    Color.FromArgb(230, 235, 245),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            };

            // ===== ШАПКА С ЛОГОТИПОМ =====
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = Color.Transparent
            };

            logoPanel = new Panel
            {
                Size = new Size(200, 120),
                Location = new Point((this.Width - 200) / 2, -100),
                BackColor = Color.Transparent
            };

            // Логотип ПолесГУ (стилизованная буква "П")
            var logoLabel = new Label
            {
                Text = "🎓",
                Font = new Font("Segoe UI Emoji", 60, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 86, 179),
                AutoSize = false,
                Size = new Size(80, 80),
                Location = new Point(60, 10),
                BackColor = Color.Transparent
            };

            var universityName = new Label
            {
                Text = "ПолесГУ",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 86, 179),
                AutoSize = false,
                Size = new Size(200, 40),
                Location = new Point(0, 75),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            var sportsText = new Label
            {
                Text = "Спорт",
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.FromArgb(0, 61, 130),
                AutoSize = false,
                Size = new Size(200, 25),
                Location = new Point(0, 105),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            logoPanel.Controls.AddRange(new Control[] { logoLabel, universityName, sportsText });
            headerPanel.Controls.Add(logoPanel);
            this.Controls.Add(headerPanel);

            // ===== ОСНОВНАЯ КАРТОЧКА АВТОРИЗАЦИИ =====
            mainCard = new Panel
            {
                Size = new Size(450, 380),
                Location = new Point((this.Width - 450) / 2, 500),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            // Заголовок карточки
            Label lblTitle = new Label
            {
                Text = "Система управления\nспортивными секциями",
                Font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold),
                Location = new Point(40, 30),
                Size = new Size(370, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(0, 61, 130),
                BackColor = Color.Transparent
            };

            // Разделительная линия
            var separator = new Panel
            {
                Location = new Point(40, 90),
                Size = new Size(370, 3),
                BackColor = Color.FromArgb(0, 86, 179)
            };

            // Логин
            Label lblLogin = new Label
            {
                Text = "Логин:",
                Location = new Point(60, 115),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 10, FontStyle.SemiBold),
                ForeColor = Color.FromArgb(0, 61, 130),
                BackColor = Color.Transparent
            };
            txtLogin = new TextBox
            {
                Location = new Point(140, 113),
                Size = new Size(250, 30),
                PlaceholderText = "Введите логин",
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Пароль
            Label lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(60, 160),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 10, FontStyle.SemiBold),
                ForeColor = Color.FromArgb(0, 61, 130),
                BackColor = Color.Transparent
            };
            txtPassword = new TextBox
            {
                Location = new Point(140, 158),
                Size = new Size(250, 30),
                PasswordChar = '●',
                PlaceholderText = "Введите пароль",
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Кнопка "Войти"
            btnLogin = new Button
            {
                Text = "Войти в систему",
                Location = new Point(140, 210),
                Size = new Size(250, 45),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(0, 86, 179),
                ForeColor = Color.White
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 61, 130);
            btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 102);
            btnLogin.Click += BtnLogin_Click;

            // Анимация кнопки при наведении
            btnLogin.MouseEnter += (s, e) =>
            {
                btnLogin.BackColor = Color.FromArgb(0, 76, 159);
                btnLogin.Location = new Point(140, 208);
                btnLogin.Size = new Size(250, 49);
            };
            btnLogin.MouseLeave += (s, e) =>
            {
                btnLogin.BackColor = Color.FromArgb(0, 86, 179);
                btnLogin.Location = new Point(140, 210);
                btnLogin.Size = new Size(250, 45);
            };

            // Кнопка "Выход"
            btnExit = new Button
            {
                Text = "Выход",
                Location = new Point(140, 270),
                Size = new Size(250, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(0, 86, 179)
            };
            btnExit.FlatAppearance.BorderSize = 1;
            btnExit.FlatAppearance.BorderColor = Color.FromArgb(0, 86, 179);
            btnExit.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 245, 255);
            btnExit.Click += (s, e) => Application.Exit();

            // Ошибка
            lblError = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(220, 53, 69),
                Location = new Point(60, 320),
                Size = new Size(330, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            mainCard.Controls.AddRange(new Control[] { lblTitle, separator, lblLogin, txtLogin, lblPassword, txtPassword, btnLogin, btnExit, lblError });
            this.Controls.Add(mainCard);

            // Футер
            var footerLabel = new Label
            {
                Text = "© 2025 ПолесГУ. Все права защищены.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true,
                Location = new Point((this.Width - 250) / 2, this.Height - 40),
                BackColor = Color.Transparent
            };
            this.Controls.Add(footerLabel);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Проверка ввода
            if (string.IsNullOrEmpty(login))
            {
                lblError.Text = "⚠️ Введите логин";
                txtLogin.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                lblError.Text = "⚠️ Введите пароль";
                txtPassword.Focus();
                return;
            }

            try
            {
                // ✅ ХЕШИРУЕМ ПАРОЛЬ перед аутентификацией
                string passwordHash = HashPassword(password);

                // Аутентификация с хешированным паролем
                Account account = DBConnection.Instance.Authenticate(login, passwordHash);

                if (account != null)
                {
                    // Обновляем время входа
                    DBConnection.Instance.UpdateLastLogin(account.AccountID);

                    // Создаём сессию
                    User.CurrentUser = new User
                    {
                        UserId = account.AccountID,
                        Login = account.Login,
                        FullName = login,
                        Email = "",
                        Phone = "",
                        Role = (UserRole)account.Role,
                        IsActive = account.IsActive,
                        LastLogin = DateTime.Now
                    };

                    Form targetForm = null;

                    switch (User.CurrentUser.Role)
                    {
                        case UserRole.Administrator:
                            targetForm = new AdminForm();
                            break;

                        case UserRole.Manager:
                            targetForm = new ManagerForm();
                            break;

                        case UserRole.Trainer:
                        case UserRole.Student:
                            MessageBox.Show("Для вашей роли используйте веб-интерфейс", "Информация",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            User.Logout();
                            this.Close();
                            return;

                        default:
                            MessageBox.Show("Неизвестная роль", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            User.Logout();
                            this.Close();
                            return;
                    }

                    if (targetForm != null)
                    {
                        this.Hide();

                        targetForm.FormClosed += (s, args) =>
                        {
                            if (targetForm.DialogResult == DialogResult.Retry)
                            {
                                this.txtPassword.Clear();
                                this.lblError.Text = "";
                                this.Show();
                            }
                            else
                            {
                                this.Close();
                            }
                        };
                        targetForm.Show();
                    }
                }
                else
                {
                    lblError.Text = "❌ Неверный логин или пароль";
                    txtPassword.Clear();
                    txtLogin.Focus();
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "❌ Ошибка: " + ex.Message;
            }
        }

        private string HashPassword(string password)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                BtnLogin_Click(null, null);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}