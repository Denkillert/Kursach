using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using PolesSU_Sports.Shared.DB;

namespace PolesSU_Sports.Admin.Forms.Dictionary
{
    public partial class SportForm : Form
    {
        private DataGridView dgvSports;
        private TextBox txtSportName;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnDelete;
        private Button btnClear;
        private int currentSportID = 0;

        public SportForm()
        {
            InitializeComponent();
            LoadSports();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление видами спорта";
            this.Size = new System.Drawing.Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(10) };

            int y = 10;
            CreateLabel("Название вида спорта:", 10, y, 120, pnlTop);
            txtSportName = CreateTextBox(140, y, 300, pnlTop);
            y += 35;

            CreateLabel("Описание:", 10, y, 120, pnlTop);
            txtDescription = new TextBox { Location = new System.Drawing.Point(140, y), Size = new System.Drawing.Size(300, 60), Multiline = true };
            pnlTop.Controls.Add(txtDescription);

            Panel pnlButtons = new Panel { Location = new System.Drawing.Point(460, 10), Size = new System.Drawing.Size(120, 85), Height=120 };

            btnSave = new Button { Text = "💾 Сохранить", Dock = DockStyle.Top, Height = 35, BackColor = System.Drawing.Color.FromArgb(0, 122, 204), ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;

            btnDelete = new Button { Text = "🗑️ Удалить", Dock = DockStyle.Top, Height = 35, BackColor = System.Drawing.Color.FromArgb(220, 53, 69), ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "❌ Очистить", Dock = DockStyle.Top, Height = 35, FlatStyle = FlatStyle.Flat };
            btnClear.Click += (s, e) => ClearForm();

            pnlButtons.Controls.AddRange(new Control[] { btnSave, btnDelete, btnClear });
            pnlTop.Controls.Add(pnlButtons);

            dgvSports = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false };
            dgvSports.CellClick += DgvSports_CellClick;

            this.Controls.Add(dgvSports);
            this.Controls.Add(pnlTop);
        }

        private Label CreateLabel(string text, int x, int y, int width, Control parent)
        {
            Label lbl = new Label { Text = text, Location = new System.Drawing.Point(x, y), Size = new System.Drawing.Size(width, 23), TextAlign = System.Drawing.ContentAlignment.MiddleRight };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private TextBox CreateTextBox(int x, int y, int width, Control parent)
        {
            TextBox txt = new TextBox { Location = new System.Drawing.Point(x, y), Size = new System.Drawing.Size(width, 23) };
            parent.Controls.Add(txt);
            return txt;
        }

        private void LoadSports()
        {
            DataTable dt = DBConnection.Instance.ExecuteQuery(
                "SELECT SportID AS [ID], SportName AS [Название], Description AS [Описание] FROM Sports ORDER BY SportName");
            dgvSports.DataSource = dt;
        }

        private void DgvSports_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSports.Rows[e.RowIndex];
                currentSportID = Convert.ToInt32(row.Cells["ID"].Value);
                txtSportName.Text = row.Cells["Название"].Value.ToString();
                txtDescription.Text = row.Cells["Описание"].Value.ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSportName.Text))
            {
                MessageBox.Show("Введите название вида спорта", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (currentSportID > 0)
                {
                    DBConnection.Instance.ExecuteCommand(@"
                        UPDATE Sports SET SportName = @SportName, Description = @Description WHERE SportID = @SportID",
                        new[] {
                            new SqlParameter("@SportName", txtSportName.Text),
                            new SqlParameter("@Description", string.IsNullOrEmpty(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text),
                            new SqlParameter("@SportID", currentSportID)
                        });
                    MessageBox.Show("✅ Вид спорта обновлён", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DBConnection.Instance.ExecuteCommand(
                        "INSERT INTO Sports (SportName, Description) VALUES (@SportName, @Description)",
                        new[] {
                            new SqlParameter("@SportName", txtSportName.Text),
                            new SqlParameter("@Description", string.IsNullOrEmpty(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text)
                        });
                    MessageBox.Show("✅ Вид спорта добавлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                LoadSports();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (currentSportID == 0) { MessageBox.Show("Выберите вид спорта", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (MessageBox.Show("Удалить вид спорта?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DBConnection.Instance.ExecuteCommand("DELETE FROM Sports WHERE SportID = @SportID", new[] { new SqlParameter("@SportID", currentSportID) });
                    MessageBox.Show("✅ Удалено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadSports();
                    ClearForm();
                }
                catch (Exception ex) { MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void ClearForm() { currentSportID = 0; txtSportName.Clear(); txtDescription.Clear(); }
    }
}