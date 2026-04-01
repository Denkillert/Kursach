using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Shared.DB;
using System.Security.Cryptography;
using System.Text;

namespace PolesSU_Sports.Management.SettingsForms
{
    public partial class UserManagementForm : Form
    {
        private DataGridView dgvUsers;

        public UserManagementForm()
        {
            SetupUI();
            LoadUsers();
        }

        private void SetupUI()
        {
            this.Text = "Управление пользователями";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "👥 Управление пользователями",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(45, 55, 75)
            };

            // Панель кнопок
            Panel btnPanel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(400, 40)
            };

            Button btnAdd = CreateButton("➕ Добавить", Color.FromArgb(0, 122, 204), 0);
            btnAdd.Click += BtnAdd_Click;

            Button btnEdit = CreateButton("✏️ Изменить", Color.FromArgb(40, 167, 69), 120);
            btnEdit.Click += BtnEdit_Click;

            Button btnDelete = CreateButton("🗑️ Удалить", Color.FromArgb(220, 53, 69), 240);
            btnDelete.Click += BtnDelete_Click;

            btnPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            // Таблица пользователей
            dgvUsers = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(850, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                }
            };

            Button btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(20, 520),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, btnPanel, dgvUsers, btnClose });
        }

        private Button CreateButton(string text, Color color, int x)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, 0),
                Size = new Size(110, 35),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
        }

        private void LoadUsers()
        {
            try
            {
                dgvUsers.DataSource = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        a.AccountID AS [ID],
                        a.Login AS [Логин],
                        CASE 
                            WHEN a.Role = 1 THEN 'Администратор'
                            WHEN a.Role = 2 THEN 'Менеджер'
                            WHEN a.Role = 3 THEN 'Тренер'
                            WHEN a.Role = 4 THEN 'Студент'
                            ELSE 'Неизвестно'
                        END AS [Роль],
                        CASE WHEN a.IsActive = 1 THEN '✅ Активен' ELSE '❌ Заблокирован' END AS [Статус],
                        a.CreatedDate AS [Создан],
                        a.LastLogin AS [Последний вход]
                    FROM Accounts a
                    ORDER BY a.Login");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new UserEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                    LoadUsers();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int accountID = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["ID"].Value);
            using (var form = new UserEditForm(accountID))
            {
                if (form.ShowDialog() == DialogResult.OK)
                    LoadUsers();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить пользователя?\nЭто действие нельзя отменить!", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                int accountID = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["ID"].Value);
                DBConnection.Instance.ExecuteCommand(
                    "DELETE FROM Accounts WHERE AccountID = @ID",
                    new[] { new SqlParameter("@ID", accountID) });

                MessageBox.Show("✅ Пользователь удалён", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ==================== ФОРМА РЕДАКТИРОВАНИЯ ====================
    public class UserEditForm : Form
    {
        private int? accountID;
        private TextBox txtLogin;
        private TextBox txtPassword;
        private ComboBox cmbRole;
        private CheckBox chkActive;

        public UserEditForm()
        {
            accountID = null;
            SetupUI("Добавить пользователя");
        }

        public UserEditForm(int id)
        {
            accountID = id;
            SetupUI("Редактировать пользователя");
            LoadUserData();
        }

        private void SetupUI(string title)
        {
            this.Text = title;
            this.Size = new Size(400, 380);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 9);

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(45, 55, 75)
            };

            Label lblLogin = new Label { Text = "Логин:", Location = new Point(20, 60), AutoSize = true };
            txtLogin = new TextBox { Location = new Point(20, 80), Size = new Size(340, 25), Font = new Font("Segoe UI", 10) };

            Label lblPassword = new Label { Text = "Пароль:" + (accountID == null ? " (обязательно)" : " (оставьте пустым, чтобы не менять)"), Location = new Point(20, 120), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(20, 140), Size = new Size(340, 25), PasswordChar = '•', Font = new Font("Segoe UI", 10) };

            Label lblRole = new Label { Text = "Роль:", Location = new Point(20, 180), AutoSize = true };
            cmbRole = new ComboBox { Location = new Point(20, 200), Size = new Size(340, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbRole.Items.AddRange(new object[] { "Администратор", "Менеджер", "Тренер", "Студент" });
            cmbRole.SelectedIndex = 1;

            chkActive = new CheckBox { Text = "✅ Активен", Location = new Point(20, 240), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 10) };

            Panel btnPanel = new Panel { Location = new Point(20, 280), Size = new Size(340, 40) };

            Button btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(0, 0),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 122, 204),
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

            this.Controls.AddRange(new Control[] { lblTitle, lblLogin, txtLogin, lblPassword, txtPassword, lblRole, cmbRole, chkActive, btnPanel });
        }

        private void LoadUserData()
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(
                    "SELECT Login, Role, IsActive FROM Accounts WHERE AccountID = @ID",
                    new[] { new SqlParameter("@ID", accountID.Value) });

                if (dt.Rows.Count > 0)
                {
                    txtLogin.Text = dt.Rows[0]["Login"].ToString();
                    cmbRole.SelectedIndex = Convert.ToInt32(dt.Rows[0]["Role"]) - 1;
                    chkActive.Checked = Convert.ToBoolean(dt.Rows[0]["IsActive"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (accountID == null && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Введите пароль для нового пользователя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string login = txtLogin.Text.Trim();
                int role = cmbRole.SelectedIndex + 1;
                bool isActive = chkActive.Checked;

                if (accountID == null)
                {
                    // Добавление
                    string passwordHash = HashPassword(txtPassword.Text);
                    DBConnection.Instance.ExecuteCommand(@"
                        INSERT INTO Accounts (Login, PasswordHash, Role, IsActive, CreatedDate)
                        VALUES (@Login, @Password, @Role, @Active, GETDATE())",
                        new[] {
                            new SqlParameter("@Login", login),
                            new SqlParameter("@Password", passwordHash),
                            new SqlParameter("@Role", role),
                            new SqlParameter("@Active", isActive)
                        });
                    MessageBox.Show("✅ Пользователь добавлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Обновление
                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        string passwordHash = HashPassword(txtPassword.Text);
                        DBConnection.Instance.ExecuteCommand(@"
                            UPDATE Accounts SET Login = @Login, PasswordHash = @Password, Role = @Role, IsActive = @Active
                            WHERE AccountID = @ID",
                            new[] {
                                new SqlParameter("@Login", login),
                                new SqlParameter("@Password", passwordHash),
                                new SqlParameter("@Role", role),
                                new SqlParameter("@Active", isActive),
                                new SqlParameter("@ID", accountID.Value)
                            });
                    }
                    else
                    {
                        DBConnection.Instance.ExecuteCommand(@"
                            UPDATE Accounts SET Login = @Login, Role = @Role, IsActive = @Active
                            WHERE AccountID = @ID",
                            new[] {
                                new SqlParameter("@Login", login),
                                new SqlParameter("@Role", role),
                                new SqlParameter("@Active", isActive),
                                new SqlParameter("@ID", accountID.Value)
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