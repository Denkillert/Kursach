using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using PolesSU_Sports.Lib.DB;

namespace PolesSU_Sports.Admin.Forms.Dictionary
{
    public partial class FacultyForm : Form
    {
        private DataGridView dgvFaculties;
        private TextBox txtFacultyName;
        private TextBox txtDeanName;
        private TextBox txtPhone;
        private Button btnSave;
        private Button btnDelete;
        private Button btnClear;
        private int currentFacultyID = 0;

        public FacultyForm()
        {
            InitializeComponent();
            LoadFaculties();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление факультетами";
            this.Size = new System.Drawing.Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Панель управления
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(10)
            };

            int y = 10;
            CreateLabel("Название факультета:", 10, y, 120, pnlTop);
            txtFacultyName = CreateTextBox(140, y, 300, pnlTop);
            y += 35;

            CreateLabel("Декан:", 10, y, 120, pnlTop);
            txtDeanName = CreateTextBox(140, y, 300, pnlTop);
            y += 35;

            CreateLabel("Телефон:", 10, y, 120, pnlTop);
            txtPhone = CreateTextBox(140, y, 300, pnlTop);

            // Кнопки
            Panel pnlButtons = new Panel
            {
                Location = new System.Drawing.Point(460, 10),
                Size = new System.Drawing.Size(120, 85),
                Height = 130
            };

            btnSave = new Button
            {
                Text = "💾 Сохранить",
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            btnDelete = new Button
            {
                Text = "🗑️ Удалить",
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = System.Drawing.Color.FromArgb(220, 53, 69),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button
            {
                Text = "❌ Очистить",
                Dock = DockStyle.Top,
                Height = 35,
                FlatStyle = FlatStyle.Flat
            };
            btnClear.Click += (s, e) => ClearForm();

            pnlButtons.Controls.AddRange(new Control[] { btnSave, btnDelete, btnClear });
            pnlTop.Controls.Add(pnlButtons);

            // DataGridView
            dgvFaculties = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            dgvFaculties.CellClick += DgvFaculties_CellClick;

            this.Controls.Add(dgvFaculties);
            this.Controls.Add(pnlTop);
        }

        private Label CreateLabel(string text, int x, int y, int width, Control parent)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private TextBox CreateTextBox(int x, int y, int width, Control parent)
        {
            TextBox txt = new TextBox
            {
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, 23)
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private void LoadFaculties()
        {
            DataTable dt = DBConnection.Instance.ExecuteQuery(
                "SELECT FacultyID AS [ID], FacultyName AS [Название], DeanName AS [Декан], Phone AS [Телефон] FROM Faculties ORDER BY FacultyName");
            dgvFaculties.DataSource = dt;
        }

        private void DgvFaculties_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvFaculties.Rows[e.RowIndex];
                currentFacultyID = Convert.ToInt32(row.Cells["ID"].Value);
                txtFacultyName.Text = row.Cells["Название"].Value.ToString();
                txtDeanName.Text = row.Cells["Декан"].Value.ToString();
                txtPhone.Text = row.Cells["Телефон"].Value.ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFacultyName.Text))
            {
                MessageBox.Show("Введите название факультета", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (currentFacultyID > 0)
                {
                    // Обновление
                    DBConnection.Instance.ExecuteCommand(@"
                        UPDATE Faculties SET 
                            FacultyName = @FacultyName,
                            DeanName = @DeanName,
                            Phone = @Phone
                        WHERE FacultyID = @FacultyID",
                        new[] {
                            new SqlParameter("@FacultyName", txtFacultyName.Text),
                            new SqlParameter("@DeanName", string.IsNullOrEmpty(txtDeanName.Text) ? (object)DBNull.Value : txtDeanName.Text),
                            new SqlParameter("@Phone", string.IsNullOrEmpty(txtPhone.Text) ? (object)DBNull.Value : txtPhone.Text),
                            new SqlParameter("@FacultyID", currentFacultyID)
                        });
                    MessageBox.Show("✅ Факультет обновлён", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Добавление
                    DBConnection.Instance.ExecuteCommand(@"
                        INSERT INTO Faculties (FacultyName, DeanName, Phone) 
                        VALUES (@FacultyName, @DeanName, @Phone)",
                        new[] {
                            new SqlParameter("@FacultyName", txtFacultyName.Text),
                            new SqlParameter("@DeanName", string.IsNullOrEmpty(txtDeanName.Text) ? (object)DBNull.Value : txtDeanName.Text),
                            new SqlParameter("@Phone", string.IsNullOrEmpty(txtPhone.Text) ? (object)DBNull.Value : txtPhone.Text)
                        });
                    MessageBox.Show("✅ Факультет добавлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadFaculties();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (currentFacultyID == 0)
            {
                MessageBox.Show("Выберите факультет для удаления", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить факультет?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DBConnection.Instance.ExecuteCommand(
                        "DELETE FROM Faculties WHERE FacultyID = @FacultyID",
                        new[] { new SqlParameter("@FacultyID", currentFacultyID) });

                    MessageBox.Show("✅ Факультет удалён", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadFaculties();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("❌ Ошибка удаления: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            currentFacultyID = 0;
            txtFacultyName.Clear();
            txtDeanName.Clear();
            txtPhone.Clear();
        }
    }
}