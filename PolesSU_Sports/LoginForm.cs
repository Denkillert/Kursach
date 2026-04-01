using PolesSU_Sports.Admin;
using PolesSU_Sports.Management;
using PolesSU_Sports.Shared.DB;
using PolesSU_Sports.Shared.Model;
using System;
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

        public LoginForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Авторизация - ПолесГУ Спорт";
            this.Size = new System.Drawing.Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "Система управления спортивными секциями",
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(340, 40),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            // Логин
            Label lblLogin = new Label
            {
                Text = "Логин:",
                Location = new System.Drawing.Point(40, 80),
                Size = new System.Drawing.Size(80, 23)
            };
            txtLogin = new TextBox
            {
                Location = new System.Drawing.Point(140, 80),
                Size = new System.Drawing.Size(200, 23),
                PlaceholderText = "Введите логин"
            };

            // Пароль
            Label lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new System.Drawing.Point(40, 120),
                Size = new System.Drawing.Size(80, 23)
            };
            txtPassword = new TextBox
            {
                Location = new System.Drawing.Point(140, 120),
                Size = new System.Drawing.Size(200, 23),
                PasswordChar = '*',
                PlaceholderText = "Введите пароль"
            };

            // Войти
            btnLogin = new Button
            {
                Text = "Войти",
                Location = new System.Drawing.Point(140, 170),
                Size = new System.Drawing.Size(100, 35),
                BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.Click += BtnLogin_Click;

            // Выход
            btnExit = new Button
            {
                Text = "Выход",
                Location = new System.Drawing.Point(140, 220),
                Size = new System.Drawing.Size(100, 35),
                FlatStyle = FlatStyle.Flat
            };
            btnExit.Click += (s, e) => Application.Exit();

            // Ошибка
            lblError = new Label
            {
                Text = "",
                ForeColor = System.Drawing.Color.Red,
                Location = new System.Drawing.Point(40, 270),
                Size = new System.Drawing.Size(300, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblLogin, txtLogin, lblPassword, txtPassword, btnLogin, btnExit, lblError });
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
                // Аутентификация
                Account account = DBConnection.Instance.Authenticate(login, password);

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

                    // Скрываем логин
                    this.Hide();

                    
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
                        // При закрытии целевой формы — закрываем всё
                        targetForm.FormClosed += (s, args) =>
                        {
                            User.Logout();
                            this.Close();
                        };
                        targetForm.ShowDialog();
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