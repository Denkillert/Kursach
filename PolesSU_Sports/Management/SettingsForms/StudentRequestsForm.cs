using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;

namespace PolesSU_Sports.Management.SettingsForms
{
    public partial class StudentRequestsForm : Form
    {
        private DataGridView dgvRequests;
        private Label lblTitle;
        private Label lblInfo;
        private Button btnRefresh;
        private int currentManagerId;

        public StudentRequestsForm(int managerId)
        {
            currentManagerId = managerId;
            InitializeComponent();
            LoadRequests();
        }

        private void InitializeComponent()
        {
            this.Text = "🎓 Заявки студентов";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);
            this.BackColor = Color.FromArgb(245, 247, 250);

            // Заголовок
            lblTitle = new Label
            {
                Text = "🎓 Заявки студентов на запись в секции",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(5, 66, 38),
                Location = new Point(30, 20),
                AutoSize = true
            };

            lblInfo = new Label
            {
                Text = "Ожидают обработки:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(30, 55),
                AutoSize = true
            };

            // Кнопка обновления
            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(950, 20),
                Size = new Size(120, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 147, 197),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadRequests();

            // Таблица заявок
            dgvRequests = new DataGridView
            {
                Location = new Point(30, 90),
                Size = new Size(1040, 520),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(5, 66, 38),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                },
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvRequests.CellFormatting += DgvRequests_CellFormatting;

            // Кнопки действий (появляются при выборе строки)
            var actionPanel = new Panel
            {
                Location = new Point(30, 620),
                Size = new Size(1040, 50),
                BackColor = Color.Transparent
            };

            var btnApprove = new Button
            {
                Text = "✅ Одобрить",
                Location = new Point(0, 0),
                Size = new Size(130, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Click += BtnApprove_Click;

            var btnReject = new Button
            {
                Text = "❌ Отклонить",
                Location = new Point(140, 0),
                Size = new Size(130, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            btnReject.FlatAppearance.BorderSize = 0;
            btnReject.Click += BtnReject_Click;

            var btnClose = new Button
            {
                Text = "✖ Закрыть",
                Location = new Point(910, 0),
                Size = new Size(130, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            actionPanel.Controls.AddRange(new Control[] { btnApprove, btnReject, btnClose });

            // Включение кнопок при выборе строки
            dgvRequests.SelectionChanged += (s, e) =>
            {
                bool hasSelection = dgvRequests.SelectedRows.Count > 0;
                btnApprove.Enabled = hasSelection;
                btnReject.Enabled = hasSelection;
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblInfo, btnRefresh, dgvRequests, actionPanel });
        }

        private void LoadRequests()
        {
            try
            {
                var data = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        r.RequestID,
                        r.StudentCardNumber AS [Билет],
                        s.LastName + ' ' + s.FirstName + ' ' + ISNULL(s.MiddleName, '') AS [Студент],
                        s.GroupName AS [Группа],
                        f.FacultyName AS [Факультет],
                        sec.SectionName AS [Секция],
                        sp.SportName AS [Вид спорта],
                        CASE WHEN r.RequestType = 'Join' THEN '📝 Вступление' ELSE '🚪 Выход' END AS [Тип],
                        r.RequestDate AS [Дата заявки]
                    FROM StudentSectionRequests r
                    JOIN Students s ON r.StudentCardNumber = s.StudentCardNumber
                    JOIN Faculties f ON s.FacultyID = f.FacultyID
                    JOIN Sections sec ON r.SectionID = sec.SectionID
                    JOIN Sports sp ON sec.SportID = sp.SportID
                    WHERE r.Status = 'Pending'
                    ORDER BY r.RequestDate DESC");

                dgvRequests.DataSource = data;

                lblInfo.Text = $"Ожидают обработки: {data.Rows.Count} заявок";

                // Сброс выделения
                dgvRequests.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заявок: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvRequests_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Подсветка типа заявки
            if (dgvRequests.Columns[e.ColumnIndex].Name == "Тип" ||
                dgvRequests.Columns[e.ColumnIndex].HeaderText == "Тип")
            {
                if (e.Value?.ToString() == "📝 Вступление")
                {
                    e.CellStyle.BackColor = Color.FromArgb(220, 255, 220);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
                else if (e.Value?.ToString() == "🚪 Выход")
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 240, 220);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            }
        }

        private void BtnApprove_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) return;

            var requestId = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
            var studentName = dgvRequests.SelectedRows[0].Cells["Студент"].Value.ToString();
            var requestType = dgvRequests.SelectedRows[0].Cells["Тип"].Value.ToString();

            if (MessageBox.Show($"Одобрить заявку?\n\nСтудент: {studentName}\nДействие: {requestType}",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                DBConnection.Instance.ProcessRequest(requestId, true, currentManagerId);
                MessageBox.Show("✅ Заявка одобрена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReject_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) return;

            var requestId = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
            var studentName = dgvRequests.SelectedRows[0].Cells["Студент"].Value.ToString();

            using (var form = new RejectReasonForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        DBConnection.Instance.ProcessRequest(requestId, false, currentManagerId, form.Reason);
                        MessageBox.Show("❌ Заявка отклонена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadRequests();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }

    // Вспомогательная форма для ввода причины отклонения
    public class RejectReasonForm : Form
    {
        public string Reason { get; private set; }
        private TextBox txtReason;

        public RejectReasonForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "❌ Причина отклонения";
            this.Size = new Size(450, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 9);

            var lbl = new Label
            {
                Text = "Укажите причину отклонения заявки:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            txtReason = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(390, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10)
            };

            var btnOk = new Button
            {
                Text = "❌ Отклонить",
                Location = new Point(200, 170),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                Reason = txtReason.Text.Trim();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(310, 170),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lbl, txtReason, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}