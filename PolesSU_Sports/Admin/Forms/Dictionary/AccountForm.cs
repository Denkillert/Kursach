using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Lib.DB;

namespace PolesSU_Sports.Admin.Forms.Dictionary
{
    public partial class AccountForm : Form
    {
        private DataGridView dgvAccounts;

        public AccountForm()
        {
            SetupUI();
            LoadAccounts();
        }

        private void SetupUI()
        {
            this.Text = "Управление учётными записями";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);
            this.MinimumSize = new Size(900, 600);

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "🔐 Управление учётными записями",
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

            // Таблица аккаунтов
            dgvAccounts = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(950, 450),
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
            dgvAccounts.CellDoubleClick += (s, e) => BtnEdit_Click(s, e);

            Button btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(20, 570),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, btnPanel, dgvAccounts, btnClose });
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

        private void LoadAccounts()
        {
            try
            {
                dgvAccounts.DataSource = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        a.AccountID AS [ID],
                        a.Login AS [Логин],
                        CASE 
                            WHEN a.Role = 'Administrator' THEN 'Администратор'
                            WHEN a.Role = 'Manager' THEN 'Менеджер'
                            WHEN a.Role = 'Trainer' THEN 'Тренер'
                            WHEN a.Role = 'Student' THEN 'Студент'
                            ELSE ISNULL(a.Role, 'Неизвестно')
                        END AS [Роль],
                        CASE WHEN a.StudentCardNumber IS NOT NULL THEN s.LastName + ' ' + s.FirstName ELSE '' END AS [Студент],
                        CASE WHEN a.TrainerID IS NOT NULL THEN t.LastName + ' ' + t.FirstName ELSE '' END AS [Тренер],
                        CASE WHEN a.IsActive = 1 THEN '✅ Активен' ELSE '❌ Заблокирован' END AS [Статус],
                        a.CreatedDate AS [Создан],
                        a.LastLogin AS [Последний вход]
                    FROM Accounts a
                    LEFT JOIN Students s ON a.StudentCardNumber = s.StudentCardNumber
                    LEFT JOIN Trainers t ON a.TrainerID = t.TrainerID
                    ORDER BY a.Login");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки аккаунтов: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new AccountEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                    LoadAccounts();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите учётную запись", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int accountID = Convert.ToInt32(dgvAccounts.SelectedRows[0].Cells["ID"].Value);
            using (var form = new AccountEditForm(accountID))
            {
                if (form.ShowDialog() == DialogResult.OK)
                    LoadAccounts();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите учётную запись для удаления", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить учётную запись?\nЭто действие нельзя отменить!", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                int accountID = Convert.ToInt32(dgvAccounts.SelectedRows[0].Cells["ID"].Value);
                DBConnection.Instance.DeleteAccount(accountID);

                MessageBox.Show("✅ Учётная запись удалена", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAccounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}