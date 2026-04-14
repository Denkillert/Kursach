using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;

namespace PolesSU_Sports.Admin.Forms.Dictionary
{
    public partial class AccountForm : Form
    {
        private DataGridView dgvAccounts;
        private TextBox txtLogin;
        private TextBox txtPassword;
        private ComboBox cmbRole;
        private ComboBox cmbStudent;
        private ComboBox cmbTrainer;
        private CheckBox chkIsActive;
        private Button btnSave;
        private Button btnDelete;
        private Button btnClear;
        private int currentAccountID = 0;

        public AccountForm()
        {
            InitializeComponent();
            LoadAccounts();
            LoadStudents();
            LoadTrainers();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление учётными записями";
            this.Size = new System.Drawing.Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.BackColor = System.Drawing.Color.White;

            // ВЕРХНЯЯ ПАНЕЛЬ 
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                Padding = new Padding(15),
                BackColor = System.Drawing.Color.FromArgb(245, 245, 245)
            };

            int y = 10;
            int labelWidth = 100;
            int inputWidth = 200;
            int inputX = 120;

            // Логин
            Label lblLogin = new Label
            {
                Text = "Логин:",
                Location = new System.Drawing.Point(10, y),
                Size = new System.Drawing.Size(labelWidth, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            txtLogin = new TextBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            pnlTop.Controls.AddRange(new Control[] { lblLogin, txtLogin });
            y += 35;

            // Пароль
            Label lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new System.Drawing.Point(10, y),
                Size = new System.Drawing.Size(labelWidth, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            txtPassword = new TextBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                PasswordChar = '*',
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            pnlTop.Controls.AddRange(new Control[] { lblPassword, txtPassword });
            y += 35;

            // Роль
            Label lblRole = new Label
            {
                Text = "Роль:",
                Location = new System.Drawing.Point(10, y),
                Size = new System.Drawing.Size(labelWidth, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            cmbRole = new ComboBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            cmbRole.Items.AddRange(new[] { "Administrator", "Manager", "Trainer", "Student" });
            cmbRole.SelectedIndex = 0;
            cmbRole.SelectedIndexChanged += CmbRole_SelectedIndexChanged;
            pnlTop.Controls.AddRange(new Control[] { lblRole, cmbRole });

            // Студент (справа)
            Label lblStudent = new Label
            {
                Text = "Студент:",
                Location = new System.Drawing.Point(350, 10),
                Size = new System.Drawing.Size(labelWidth, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            cmbStudent = new ComboBox
            {
                Location = new System.Drawing.Point(470, 10),
                Size = new System.Drawing.Size(350, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            pnlTop.Controls.AddRange(new Control[] { lblStudent, cmbStudent });

            // Тренер (справа)
            Label lblTrainer = new Label
            {
                Text = "Тренер:",
                Location = new System.Drawing.Point(350, 45),
                Size = new System.Drawing.Size(labelWidth, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            cmbTrainer = new ComboBox
            {
                Location = new System.Drawing.Point(470, 45),
                Size = new System.Drawing.Size(350, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            pnlTop.Controls.AddRange(new Control[] { lblTrainer, cmbTrainer });

            // Активен (справа)
            chkIsActive = new CheckBox
            {
                Text = "Активен",
                Location = new System.Drawing.Point(470, 80),
                Size = new System.Drawing.Size(150, 25),
                Checked = true,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            pnlTop.Controls.Add(chkIsActive);

            // КНОПКИ 
            FlowLayoutPanel pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 150,
                Height = 140,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(5, 10, 5, 5),
                BackColor = System.Drawing.Color.Transparent
            };

            btnSave = new Button
            {
                Text = "Сохранить",
                Width = 140,
                Height = 35,
                BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9),
                Margin = new Padding(3, 0, 3, 5)
            };
            btnSave.Click += BtnSave_Click;

            btnDelete = new Button
            {
                Text = "Удалить",
                Width = 140,
                Height = 35,
                BackColor = System.Drawing.Color.FromArgb(220, 53, 69),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9),
                Margin = new Padding(3, 0, 3, 5)
            };
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button
            {
                Text = "Очистить",
                Width = 140,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9),
                Margin = new Padding(3)
            };
            btnClear.Click += (s, e) => ClearForm();

            pnlButtons.Controls.AddRange(new Control[] { btnSave, btnDelete, btnClear });
            pnlTop.Controls.Add(pnlButtons);

            dgvAccounts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 9, System.Drawing.FontStyle.Bold)
                }
            };
            dgvAccounts.CellClick += DgvAccounts_CellClick;

            this.Controls.Add(dgvAccounts);
            this.Controls.Add(pnlTop);
        }

        private void LoadAccounts()
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        a.AccountID AS [ID],
                        a.Login AS [Логин],
                        a.Role AS [Роль],
                        CASE WHEN a.StudentCardNumber IS NOT NULL THEN s.LastName + ' ' + s.FirstName ELSE '' END AS [Студент],
                        CASE WHEN a.TrainerID IS NOT NULL THEN t.LastName + ' ' + t.FirstName ELSE '' END AS [Тренер],
                        CASE WHEN a.IsActive = 1 THEN 'Да' ELSE 'Нет' END AS [Активен],
                        a.CreatedDate AS [Создан],
                        a.LastLogin AS [Последний вход]
                    FROM Accounts a
                    LEFT JOIN Students s ON a.StudentCardNumber = s.StudentCardNumber
                    LEFT JOIN Trainers t ON a.TrainerID = t.TrainerID
                    ORDER BY a.Login");
                dgvAccounts.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки аккаунтов: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStudents()
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(
                    "SELECT StudentCardNumber, LastName + ' ' + FirstName + ' ' + ISNULL(MiddleName, '') AS FullName FROM Students ORDER BY LastName");
                cmbStudent.DataSource = dt;
                cmbStudent.DisplayMember = "FullName";
                cmbStudent.ValueMember = "StudentCardNumber";
            }
            catch { }
        }

        private void LoadTrainers()
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(
                    "SELECT TrainerID, LastName + ' ' + FirstName + ' ' + ISNULL(MiddleName, '') AS FullName FROM Trainers ORDER BY LastName");
                cmbTrainer.DataSource = dt;
                cmbTrainer.DisplayMember = "FullName";
                cmbTrainer.ValueMember = "TrainerID";
            }
            catch { }
        }

        private void CmbRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            string role = cmbRole.SelectedItem?.ToString();
            cmbStudent.Enabled = (role == "Student");
            cmbTrainer.Enabled = (role == "Trainer");

            // ✅ ПРАВИЛЬНО: только SelectedIndex = -1
            if (role != "Student")
                cmbStudent.SelectedIndex = -1;

            if (role != "Trainer")
                cmbTrainer.SelectedIndex = -1;
        }

        private void DgvAccounts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvAccounts.Rows[e.RowIndex];
                currentAccountID = Convert.ToInt32(row.Cells["ID"].Value);
                txtLogin.Text = row.Cells["Логин"].Value.ToString();
                txtPassword.Text = "";
                cmbRole.Text = row.Cells["Роль"].Value.ToString();
                chkIsActive.Checked = row.Cells["Активен"].Value.ToString() == "Да";

                CmbRole_SelectedIndexChanged(null, null);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Введите логин", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLogin.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text) && currentAccountID == 0)
            {
                MessageBox.Show("Введите пароль", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                Account account = new Account
                {
                    AccountID = currentAccountID,
                    Login = txtLogin.Text,
                    PasswordHash = txtPassword.Text,
                    Role = (AccountRole)Enum.Parse(typeof(AccountRole), cmbRole.SelectedItem.ToString()),
                    StudentCardNumber = cmbStudent.Enabled ? cmbStudent.SelectedValue?.ToString() : null,
                    TrainerID = cmbTrainer.Enabled ? (int?)cmbTrainer.SelectedValue : null,
                    IsActive = chkIsActive.Checked
                };

                if (currentAccountID > 0)
                {
                    DBConnection.Instance.UpdateAccount(account);
                    MessageBox.Show("Аккаунт обновлён", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DBConnection.Instance.CreateAccount(account);
                    MessageBox.Show("Аккаунт создан", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadAccounts();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (currentAccountID == 0)
            {
                MessageBox.Show("Выберите аккаунт для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить учётную запись?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DBConnection.Instance.DeleteAccount(currentAccountID);
                    MessageBox.Show("Аккаунт удалён", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAccounts();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            currentAccountID = 0;
            txtLogin.Clear();
            txtPassword.Clear();
            cmbRole.SelectedIndex = 0;
            cmbStudent.SelectedIndex = -1;
            cmbTrainer.SelectedIndex = -1;

            chkIsActive.Checked = true;
            CmbRole_SelectedIndexChanged(null, null);
        }
    }
}