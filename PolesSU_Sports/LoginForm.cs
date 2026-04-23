using PolesSU_Sports.Admin;
using PolesSU_Sports.Management;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;
using System;
using System.Drawing;
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

        // ✅ ЦВЕТА ПОЛЕСГУ
        private readonly Color GreenMain = Color.FromArgb(5, 66, 38);
        private readonly Color BlueAccent = Color.FromArgb(0, 147, 197);

        public LoginForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // === ОСНОВНЫЕ НАСТРОЙКИ ===
            this.Text = "";
            this.Size = new Size(420, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            // === ШАПКА ===
            Panel headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(420, 160),
                BackColor = GreenMain
            };

            Label titleText = new Label
            {
                Text = "ПолесГУ Спорт",
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(380, 50),
                Location = new Point(20, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label subtitleText = new Label
            {
                Text = "Система управления спортивными секциями",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(190, 225, 210),
                AutoSize = false,
                Size = new Size(380, 30),
                Location = new Point(20, 90),
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerPanel.Controls.AddRange(new Control[] { titleText, subtitleText });
            this.Controls.Add(headerPanel);

            // === ЗАГОЛОВОК ФОРМЫ ===
            Label formTitle = new Label
            {
                Text = "Вход в систему",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(0, 180),
                Size = new Size(420, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(formTitle);

            // === ПОЛЕ ЛОГИНА ===
            Label lblLogin = new Label
            {
                Text = "Логин",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(50, 230),
                AutoSize = true
            };

            txtLogin = new TextBox
            {
                Location = new Point(50, 255),
                Size = new Size(320, 40),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Введите логин"
            };

            this.Controls.AddRange(new Control[] { lblLogin, txtLogin });

            // === ПОЛЕ ПАРОЛЯ ===
            Label lblPassword = new Label
            {
                Text = "Пароль",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(50, 310),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(50, 335),
                Size = new Size(320, 40),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●',
                PlaceholderText = "Введите пароль"
            };

            this.Controls.AddRange(new Control[] { lblPassword, txtPassword });

            // === КНОПКА ВОЙТИ ===
            btnLogin = new Button
            {
                Text = "Войти",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = BlueAccent,
                Location = new Point(50, 400),
                Size = new Size(320, 45),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 130, 175);
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            // === КНОПКА ОТМЕНА ===
            btnExit = new Button
            {
                Text = "Отмена",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.White,
                Location = new Point(50, 455),
                Size = new Size(320, 42),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExit.FlatAppearance.BorderSize = 1;
            btnExit.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnExit.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 245, 245);
            btnExit.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnExit);

            // === ОШИБКА ===
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 50, 50),
                Location = new Point(50, 510),
                Size = new Size(320, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblError);

            // === ФУТЕР ===
            Label footerText = new Label
            {
                Text = "© 2026 ПолесГУ",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(150, 150, 150),
                Location = new Point(0, 570),
                Size = new Size(420, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(footerText);
        }

        // === ФУНКЦИОНАЛ — БЕЗ ИЗМЕНЕНИЙ ===
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

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
                string passwordHash = HashPassword(password);
                Account account = DBConnection.Instance.Authenticate(login, passwordHash);

                if (account != null)
                {
                    DBConnection.Instance.UpdateLastLogin(account.AccountID);

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