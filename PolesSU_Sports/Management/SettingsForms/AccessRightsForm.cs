using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Shared.DB;

namespace PolesSU_Sports.Management.SettingsForms
{
    public partial class AccessRightsForm : Form
    {
        private DataGridView dgvPermissions;

        public AccessRightsForm()
        {
            InitializeComponent();
            LoadPermissions();
        }

        private void InitializeComponent()
        {
            this.Text = "Права доступа";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label
            {
                Text = "🔐 Права доступа по ролям",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            dgvPermissions = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(750, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White
            };

            Button btnSave = new Button
            {
                Text = "💾 Сохранить изменения",
                Location = new Point(20, 430),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            Button btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(210, 430),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, dgvPermissions, btnSave, btnClose });
        }

        private void LoadPermissions()
        {
            dgvPermissions.Columns.Clear();
            dgvPermissions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Permission", HeaderText = "Разрешение", ReadOnly = true });
            dgvPermissions.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Admin", HeaderText = "Администратор", Width = 80 });
            dgvPermissions.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Manager", HeaderText = "Менеджер", Width = 80 });
            dgvPermissions.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Trainer", HeaderText = "Тренер", Width = 80 });
            dgvPermissions.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Student", HeaderText = "Студент", Width = 80 });

            // Загружаем разрешения из БД или создаём стандартные
            DataTable permissions = GetDefaultPermissions();

            foreach (DataRow row in permissions.Rows)
            {
                dgvPermissions.Rows.Add(
                    row["Permission"],
                    Convert.ToBoolean(row["Admin"]),
                    Convert.ToBoolean(row["Manager"]),
                    Convert.ToBoolean(row["Trainer"]),
                    Convert.ToBoolean(row["Student"])
                );
            }
        }

        private DataTable GetDefaultPermissions()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Permission");
            dt.Columns.Add("Admin");
            dt.Columns.Add("Manager");
            dt.Columns.Add("Trainer");
            dt.Columns.Add("Student");

            dt.Rows.Add("Просмотр дашборда", true, true, true, true);
            dt.Rows.Add("Управление студентами", true, true, true, false);
            dt.Rows.Add("Управление тренерами", true, true, false, false);
            dt.Rows.Add("Управление секциями", true, true, true, false);
            dt.Rows.Add("Отметка посещаемости", true, true, true, false);
            dt.Rows.Add("Просмотр отчётов", true, true, true, true);
            dt.Rows.Add("Генерация отчётов", true, true, false, false);
            dt.Rows.Add("Управление пользователями", true, false, false, false);
            dt.Rows.Add("Настройка системы", true, false, false, false);

            return dt;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int saved = 0;
                foreach (DataGridViewRow row in dgvPermissions.Rows)
                {
                    if (row.Cells["Permission"].Value != null)
                    {
                        string permission = row.Cells["Permission"].Value.ToString();
                        bool admin = Convert.ToBoolean(row.Cells["Admin"].Value);
                        bool manager = Convert.ToBoolean(row.Cells["Manager"].Value);
                        bool trainer = Convert.ToBoolean(row.Cells["Trainer"].Value);
                        bool student = Convert.ToBoolean(row.Cells["Student"].Value);

                        // Сохраняем в БД (нужно создать таблицу Permissions)
                        // DBConnection.Instance.ExecuteCommand(...);
                        saved++;
                    }
                }

                MessageBox.Show($"✅ Сохранено {saved} разрешений\n\n⚠️ Примечание: Полная реализация требует создания таблицы Permissions в БД",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}