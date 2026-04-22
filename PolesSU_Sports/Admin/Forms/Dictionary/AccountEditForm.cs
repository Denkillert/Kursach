using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;
using System.Security.Cryptography;
using System.Text;

namespace PolesSU_Sports.Admin.Forms.Dictionary
{
    public partial class AccountEditForm : Form
    {
        private int? accountID;
        private TextBox txtLogin;
        private TextBox txtPassword;
        private ComboBox cmbRole;
        private ComboBox cmbStudent;
        private ComboBox cmbTrainer;
        private CheckBox chkActive;

        public AccountEditForm()
        {
            accountID = null;
            SetupUI("➕ Добавить учётную запись");
            LoadComboBoxes();
        }

        public AccountEditForm(int id)
        {
            accountID = id;
            SetupUI("✏️ Редактировать учётную запись");
            LoadComboBoxes();
            LoadAccountData();
        }

        private void SetupUI(string title)
        {
            this.Text = title;
            this.Size = new Size(500, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 9);

            // Заголовок
            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(0, 61, 130)
            };

            // Логин
            Label lblLogin = new Label { Text = "Логин:", Location = new Point(20, 60), AutoSize = true };
            txtLogin = new TextBox
            {
                Location = new Point(20, 80),
                Size = new Size(440, 25),
                Font = new Font("Segoe UI", 10)
            };

            // Пароль
            Label lblPassword = new Label
            {
                Text = "Пароль:" + (accountID == null ? " (обязательно)" : " (оставьте пустым, чтобы не менять)"),
                Location = new Point(20, 120),
                AutoSize = true
            };
            txtPassword = new TextBox
            {
                Location = new Point(20, 140),
                Size = new Size(440, 25),
                PasswordChar = '•',
                Font = new Font("Segoe UI", 10)
            };

            // Роль
            Label lblRole = new Label { Text = "Роль:", Location = new Point(20, 180), AutoSize = true };
            cmbRole = new ComboBox
            {
                Location = new Point(20, 200),
                Size = new Size(440, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbRole.Items.AddRange(new object[] { "Administrator", "Manager", "Trainer", "Student" });
            cmbRole.SelectedIndex = 1;
            cmbRole.SelectedIndexChanged += CmbRole_SelectedIndexChanged;

            // Студент (для роли Student)
            Label lblStudent = new Label { Text = "Студент:", Location = new Point(20, 240), AutoSize = true, Visible = false };
            cmbStudent = new ComboBox
            {
                Location = new Point(20, 260),
                Size = new Size(440, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Visible = false
            };

            // Тренер (для роли Trainer)
            Label lblTrainer = new Label { Text = "Тренер:", Location = new Point(20, 240), AutoSize = true, Visible = false };
            cmbTrainer = new ComboBox
            {
                Location = new Point(20, 260),
                Size = new Size(440, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Visible = false
            };

            // Активен
            chkActive = new CheckBox
            {
                Text = "✅ Активен",
                Location = new Point(20, 300),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 10)
            };

            // Кнопки
            Panel btnPanel = new Panel { Location = new Point(20, 440), Size = new Size(440, 40) };

            Button btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(0, 0),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 86, 179),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(130, 0),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            btnPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            this.Controls.AddRange(new Control[] {
                lblTitle, lblLogin, txtLogin, lblPassword, txtPassword,
                lblRole, cmbRole, lblStudent, cmbStudent, lblTrainer, cmbTrainer,
                chkActive, btnPanel
            });
        }

        private void LoadComboBoxes()
        {
            try
            {
                // Студенты
                var students = DBConnection.Instance.ExecuteQuery(
                    "SELECT StudentCardNumber, LastName + ' ' + FirstName + ' ' + ISNULL(MiddleName, '') AS FullName FROM Students ORDER BY LastName");
                cmbStudent.DataSource = students;
                cmbStudent.DisplayMember = "FullName";
                cmbStudent.ValueMember = "StudentCardNumber";

                // Тренеры
                var trainers = DBConnection.Instance.ExecuteQuery(
                    "SELECT TrainerID, LastName + ' ' + FirstName + ' ' + ISNULL(MiddleName, '') AS FullName FROM Trainers ORDER BY LastName");
                cmbTrainer.DataSource = trainers;
                cmbTrainer.DisplayMember = "FullName";
                cmbTrainer.ValueMember = "TrainerID";
            }
            catch { }
        }

        private void CmbRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            string role = cmbRole.SelectedItem?.ToString();

            bool isStudent = (role == "Student");
            bool isTrainer = (role == "Trainer");

            cmbStudent.Visible = isStudent;
            cmbTrainer.Visible = isTrainer;

            if (!isStudent) cmbStudent.SelectedIndex = -1;
            if (!isTrainer) cmbTrainer.SelectedIndex = -1;
        }

        private void LoadAccountData()
        {
            try
            {
                var dt = DBConnection.Instance.ExecuteQuery(
                    "SELECT * FROM Accounts WHERE AccountID = @ID",
                    new[] { new SqlParameter("@ID", accountID.Value) });

                if (dt.Rows.Count == 0) return;

                var row = dt.Rows[0];
                txtLogin.Text = row["Login"].ToString();
                txtPassword.Text = ""; // Не показываем хеш

                // Роль
                string role = row["Role"].ToString();
                int roleIndex = Array.IndexOf(new[] { "Administrator", "Manager", "Trainer", "Student" }, role);
                cmbRole.SelectedIndex = roleIndex >= 0 ? roleIndex : 1;

                // Студент/Тренер
                if (role == "Student" && row["StudentCardNumber"] != DBNull.Value)
                {
                    cmbStudent.SelectedValue = row["StudentCardNumber"].ToString();
                }
                if (role == "Trainer" && row["TrainerID"] != DBNull.Value)
                {
                    cmbTrainer.SelectedValue = row["TrainerID"];
                }

                // Активен
                chkActive.Checked = Convert.ToBoolean(row["IsActive"]);

                CmbRole_SelectedIndexChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ ХЕШИРОВАНИЕ ПАРОЛЯ (SHA256 — как в веб-приложении!)
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLogin.Focus();
                return;
            }

            if (accountID == null && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Введите пароль для нового пользователя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                string login = txtLogin.Text.Trim();
                string role = cmbRole.SelectedItem.ToString();
                bool isActive = chkActive.Checked;

                // ✅ Хешируем пароль ТОЛЬКО если он введён (новый или смена)
                string passwordHash = null;
                if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    passwordHash = HashPassword(txtPassword.Text);
                }

                if (accountID == null)
                {
                    // === ДОБАВЛЕНИЕ ===
                    if (string.IsNullOrWhiteSpace(passwordHash))
                    {
                        MessageBox.Show("Пароль обязателен при создании аккаунта", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    Account newAccount = new Account
                    {
                        Login = login,
                        PasswordHash = passwordHash,  // ✅ Сохраняем ХЕШ!
                        Role = (AccountRole)Enum.Parse(typeof(AccountRole), role),
                        StudentCardNumber = (role == "Student") ? cmbStudent.SelectedValue?.ToString() : null,
                        TrainerID = (role == "Trainer") ? (int?)cmbTrainer.SelectedValue : null,
                        IsActive = isActive
                    };

                    DBConnection.Instance.CreateAccount(newAccount);
                    MessageBox.Show("✅ Учётная запись создана", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // === ОБНОВЛЕНИЕ ===
                    Account account = new Account
                    {
                        AccountID = accountID.Value,
                        Login = login,
                        Role = (AccountRole)Enum.Parse(typeof(AccountRole), role),
                        StudentCardNumber = (role == "Student") ? cmbStudent.SelectedValue?.ToString() : null,
                        TrainerID = (role == "Trainer") ? (int?)cmbTrainer.SelectedValue : null,
                        IsActive = isActive
                    };

                    if (!string.IsNullOrWhiteSpace(passwordHash))
                    {
                        // Меняем пароль
                        account.PasswordHash = passwordHash;  // ✅ Сохраняем ХЕШ!
                        DBConnection.Instance.UpdateAccount(account);
                    }
                    else
                    {
                        // Не меняем пароль — обновляем только остальные поля
                        DBConnection.Instance.ExecuteCommand(@"
                            UPDATE Accounts SET Login = @Login, Role = @Role, 
                                StudentCardNumber = @StudentCard, TrainerID = @TrainerID, IsActive = @Active
                            WHERE AccountID = @AccountID",
                            new[] {
                                new SqlParameter("@Login", account.Login),
                                new SqlParameter("@Role", account.Role.ToString()),
                                new SqlParameter("@StudentCard", (object)account.StudentCardNumber ?? DBNull.Value),
                                new SqlParameter("@TrainerID", (object)account.TrainerID ?? DBNull.Value),
                                new SqlParameter("@Active", account.IsActive),
                                new SqlParameter("@AccountID", account.AccountID)
                            });
                    }

                    MessageBox.Show("✅ Данные обновлены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}