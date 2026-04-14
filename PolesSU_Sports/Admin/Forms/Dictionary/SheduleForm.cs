using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using PolesSU_Sports.Lib.DB;
using System.Linq;

namespace PolesSU_Sports.Admin.Forms.Dictionary
{
    public partial class ScheduleForm : Form
    {
        private DataGridView dgvSchedule;
        private ComboBox cmbSection;
        private ComboBox cmbDayOfWeek;
        private MaskedTextBox txtStartTime;
        private MaskedTextBox txtEndTime;
        private TextBox txtRoom;
        private Button btnSave;
        private Button btnDelete;
        private Button btnClear;
        private int currentScheduleID = 0;

        public ScheduleForm()
        {
            InitializeComponent();
            LoadSections();
            LoadSchedule();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление расписанием";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(800, 500);

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
            int inputWidth = 350;
            int inputX = 120;

            // --- Секция ---
            Label lblSection = new Label
            {
                Text = "Секция:",
                Location = new System.Drawing.Point(10, y),
                Size = new System.Drawing.Size(labelWidth, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            cmbSection = new ComboBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            pnlTop.Controls.AddRange(new Control[] { lblSection, cmbSection });
            y += 35;

            // --- День недели ---
            Label lblDay = new Label
            {
                Text = "День недели:",
                Location = new System.Drawing.Point(10, y),
                Size = new System.Drawing.Size(labelWidth, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            cmbDayOfWeek = new ComboBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDayOfWeek.Items.AddRange(new[] { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" });
            cmbDayOfWeek.SelectedIndex = 0;
            pnlTop.Controls.AddRange(new Control[] { lblDay, cmbDayOfWeek });
            y += 35;

            // --- Время ---
            Label lblTime = new Label
            {
                Text = "Время:",
                Location = new System.Drawing.Point(10, y),
                Size = new System.Drawing.Size(labelWidth, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            Panel pnlTime = new Panel
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(300, 25)
            };
            txtStartTime = new MaskedTextBox
            {
                Location = new System.Drawing.Point(0, 0),
                Size = new System.Drawing.Size(80, 23),
                Mask = "00:00",
                Text = "16:00"
            };
            Label lblTo = new Label
            {
                Text = "—",
                Location = new System.Drawing.Point(85, 2),
                Size = new System.Drawing.Size(20, 20),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            txtEndTime = new MaskedTextBox
            {
                Location = new System.Drawing.Point(110, 0),
                Size = new System.Drawing.Size(80, 23),
                Mask = "00:00",
                Text = "17:30"
            };
            pnlTime.Controls.AddRange(new Control[] { txtStartTime, lblTo, txtEndTime });
            pnlTop.Controls.AddRange(new Control[] { lblTime, pnlTime });
            y += 35;

            // --- Аудитория ---
            Label lblRoom = new Label
            {
                Text = "Аудитория/зал:",
                Location = new System.Drawing.Point(10, y),
                Size = new System.Drawing.Size(labelWidth, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            txtRoom = new TextBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23)
            };
            pnlTop.Controls.AddRange(new Control[] { lblRoom, txtRoom });

            // ПАНЕЛЬ С КНОПКАМИ
            FlowLayoutPanel pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,  
                Width = 150,
                Height = 120,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(5),
                BackColor = System.Drawing.Color.Transparent
            };

            btnSave = new Button
            {
                Text = "💾 Сохранить",
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
                Text = "🗑️ Удалить",
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
                Text = "❌ Очистить",
                Width = 140,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9),
                Margin = new Padding(3)
            };
            btnClear.Click += (s, e) => ClearForm();

            pnlButtons.Controls.AddRange(new Control[] { btnSave, btnDelete, btnClear });
            pnlTop.Controls.Add(pnlButtons);

            dgvSchedule = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 9, System.Drawing.FontStyle.Bold),
                    SelectionBackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                    SelectionForeColor = System.Drawing.Color.White
                },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
            dgvSchedule.CellClick += DgvSchedule_CellClick;

            this.Controls.Add(dgvSchedule);  
            this.Controls.Add(pnlTop);       
        }


        private void LoadSections()
        {
            DataTable dt = DBConnection.Instance.ExecuteQuery("SELECT SectionID, SectionName FROM Sections ORDER BY SectionName");
            cmbSection.DataSource = dt;
            cmbSection.DisplayMember = "SectionName";
            cmbSection.ValueMember = "SectionID";
        }

        private void LoadSchedule()
        {
            DataTable dt = DBConnection.Instance.ExecuteQuery(@"
                SELECT 
                    s.ScheduleID AS [ID],
                    sec.SectionName AS [Секция],
                    CASE s.DayOfWeek 
                        WHEN 1 THEN 'Понедельник' WHEN 2 THEN 'Вторник' WHEN 3 THEN 'Среда' 
                        WHEN 4 THEN 'Четверг' WHEN 5 THEN 'Пятница' WHEN 6 THEN 'Суббота' 
                        WHEN 7 THEN 'Воскресенье' 
                    END AS [День недели],
                    s.StartTime AS [Начало],
                    s.EndTime AS [Конец],
                    s.Room AS [Помещение]
                FROM Schedule s
                JOIN Sections sec ON s.SectionID = sec.SectionID
                ORDER BY s.DayOfWeek, s.StartTime");
            dgvSchedule.DataSource = dt;
        }

        private void DgvSchedule_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSchedule.Rows[e.RowIndex];
                currentScheduleID = Convert.ToInt32(row.Cells["ID"].Value);
                // Здесь нужно загрузить данные для редактирования (упрощено)
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Проверка секции
            if (cmbSection.SelectedValue == null)
            {
                MessageBox.Show("Выберите секцию", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка времени
            if (string.IsNullOrWhiteSpace(txtStartTime.Text) || string.IsNullOrWhiteSpace(txtEndTime.Text))
            {
                MessageBox.Show("Введите время начала и окончания", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Парсим время из MaskedTextBox
            TimeSpan startTime;
            TimeSpan endTime;

            if (!TimeSpan.TryParse(txtStartTime.Text, out startTime))
            {
                MessageBox.Show("Неверный формат времени начала (должно быть ЧЧ:ММ)", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStartTime.Focus();
                return;
            }

            if (!TimeSpan.TryParse(txtEndTime.Text, out endTime))
            {
                MessageBox.Show("Неверный формат времени окончания (должно быть ЧЧ:ММ)", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEndTime.Focus();
                return;
            }

            // Проверка: конец должен быть больше начала
            if (endTime <= startTime)
            {
                MessageBox.Show("Время окончания должно быть больше времени начала", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEndTime.Focus();
                return;
            }

            try
            {
                int dayOfWeek = cmbDayOfWeek.SelectedIndex + 1;

                SqlParameter[] parameters = {
            new SqlParameter("@SectionID", cmbSection.SelectedValue),
            new SqlParameter("@DayOfWeek", dayOfWeek),
            new SqlParameter("@StartTime", startTime),
            new SqlParameter("@EndTime", endTime),
            new SqlParameter("@Room", string.IsNullOrEmpty(txtRoom.Text) ? (object)DBNull.Value : txtRoom.Text)
        };

                if (currentScheduleID > 0)
                {
                    // Обновление
                    parameters = parameters.Concat(new[] {
                new SqlParameter("@ScheduleID", currentScheduleID)
            }).ToArray();

                    DBConnection.Instance.ExecuteCommand(@"
                UPDATE Schedule SET 
                    SectionID = @SectionID, 
                    DayOfWeek = @DayOfWeek, 
                    StartTime = @StartTime, 
                    EndTime = @EndTime, 
                    Room = @Room 
                WHERE ScheduleID = @ScheduleID",
                        parameters);

                    MessageBox.Show("✅ Расписание обновлено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Добавление
                    DBConnection.Instance.ExecuteCommand(@"
                INSERT INTO Schedule (SectionID, DayOfWeek, StartTime, EndTime, Room) 
                VALUES (@SectionID, @DayOfWeek, @StartTime, @EndTime, @Room)",
                        parameters);

                    MessageBox.Show("✅ Расписание добавлено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadSchedule();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (currentScheduleID == 0) { MessageBox.Show("Выберите запись", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (MessageBox.Show("Удалить запись расписания?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DBConnection.Instance.ExecuteCommand("DELETE FROM Schedule WHERE ScheduleID = @ScheduleID", new[] { new SqlParameter("@ScheduleID", currentScheduleID) });
                    MessageBox.Show("✅ Удалено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadSchedule();
                    ClearForm();
                }
                catch (Exception ex) { MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void ClearForm() { currentScheduleID = 0; txtRoom.Clear(); }
    }
}