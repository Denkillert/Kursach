using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Lib.DB;

namespace PolesSU_Sports.Admin.Attendance
{
    public partial class AttendanceMarkForm : Form
    {
        private DateTime visitDate;
        private DataGridView dgvStudents;
        private ComboBox cmbSections;

        public AttendanceMarkForm(DateTime date)
        {
            visitDate = date;
            InitializeComponent();
            LoadSections();
        }

        private void InitializeComponent()
        {
            this.Text = "Отметка посещения — " + visitDate.ToShortDateString();
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label
            {
                Text = "Отметьте ОТСУТСТВУЮЩИХ студентов",
                Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69),
                Location = new Point(20, 15),
                AutoSize = true
            };

            Label lblInfo = new Label
            {
                Text = "По умолчанию все студенты присутствовали. Отметьте тех, кто отсутствовал.",
                Font = new Font("Microsoft Sans Serif", 9),
                ForeColor = Color.Gray,
                Location = new Point(20, 40),
                AutoSize = true
            };

            Label lblSection = new Label
            {
                Text = "Секция:",
                Location = new Point(20, 70),
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            cmbSections = new ComboBox
            {
                Location = new Point(80, 67),
                Size = new Size(300, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            cmbSections.SelectedIndexChanged += (s, e) => LoadStudents();

            dgvStudents = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(850, 380),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false
            };

            // Колонки:
            dgvStudents.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Absent",
                HeaderText = "❌ Отсутствует",
                Width = 90
            });
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StudentCard",
                HeaderText = "Билет",
                ReadOnly = true
            });
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                HeaderText = "ФИО",
                ReadOnly = true
            });
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                HeaderText = "Примечание (причина отсутствия)",
                Width = 250
            });

            Button btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(20, 490),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(170, 490),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] {
                lblTitle, lblInfo, lblSection, cmbSections, dgvStudents, btnSave, btnCancel
            });
        }

        private void LoadSections()
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(@"
                    SELECT SectionID, SectionName 
                    FROM Sections 
                    ORDER BY SectionName");

                cmbSections.DataSource = dt;
                cmbSections.DisplayMember = "SectionName";
                cmbSections.ValueMember = "SectionID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки секций: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStudents()
        {
            dgvStudents.Rows.Clear();

            if (cmbSections.SelectedValue == null || cmbSections.SelectedValue == DBNull.Value)
            {
                return;
            }

            int sectionID = 0;

            try
            {
                if (cmbSections.SelectedValue is DataRowView rowView)
                {
                    var id = rowView["SectionID"];
                    if (id != null && id != DBNull.Value)
                    {
                        sectionID = Convert.ToInt32(id);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    sectionID = Convert.ToInt32(cmbSections.SelectedValue);
                }

                // Загружаем только студентов, записанных в эту секцию
                DataTable dt = DBConnection.Instance.ExecuteQuery(@"
                    SELECT s.StudentCardNumber, 
                        s.LastName + ' ' + s.FirstName AS FullName
                    FROM Students s
                    INNER JOIN StudentSections ss ON s.StudentCardNumber = ss.StudentCardNumber
                    WHERE ss.SectionID = @SectionID AND ss.IsActive = 1
                    ORDER BY s.LastName",
                    new[] { new SqlParameter("@SectionID", sectionID) });

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("В выбранной секции нет записанных студентов", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (DataRow row in dt.Rows)
                {
                    string cardNumber = row["StudentCardNumber"]?.ToString();
                    string fullName = row["FullName"]?.ToString();

                    if (!string.IsNullOrEmpty(cardNumber))
                    {
                        int rowIndex = dgvStudents.Rows.Add(false, cardNumber, fullName, "");

                        // Проверяем, был ли студент отмечен как отсутствующий на эту дату
                        try
                        {
                            var attendanceData = DBConnection.Instance.ExecuteQuery(
                                @"SELECT Status, Notes 
                                  FROM Attendance a 
                                  JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID 
                                  WHERE a.StudentCardNumber = @Card 
                                  AND CAST(a.VisitDate AS DATE) = @Date 
                                  AND sc.SectionID = @SectionID",
                                new[] {
                                    new SqlParameter("@Card", cardNumber),
                                    new SqlParameter("@Date", visitDate.Date),
                                    new SqlParameter("@SectionID", sectionID)
                                });

                            if (attendanceData != null && attendanceData.Rows.Count > 0)
                            {
                                DataRow attendanceRow = attendanceData.Rows[0];
                                int status = Convert.ToInt32(attendanceRow["Status"]);

                                // Если отсутствовал (Status = 0)
                                if (status == 0)
                                {
                                    dgvStudents.Rows[rowIndex].Cells["Absent"].Value = true;

                                    // Загружаем примечание
                                    if (attendanceRow["Notes"] != DBNull.Value)
                                    {
                                        dgvStudents.Rows[rowIndex].Cells["Notes"].Value = attendanceRow["Notes"].ToString();
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки студентов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbSections.SelectedValue == null || cmbSections.SelectedValue == DBNull.Value)
            {
                MessageBox.Show("Выберите секцию", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sectionID = 0;

            try
            {
                if (cmbSections.SelectedValue is DataRowView rowView)
                {
                    var id = rowView["SectionID"];
                    if (id != null && id != DBNull.Value)
                    {
                        sectionID = Convert.ToInt32(id);
                    }
                }
                else
                {
                    sectionID = Convert.ToInt32(cmbSections.SelectedValue);
                }

                int saved = 0;
                int updated = 0;
                int dayOfWeek = (int)visitDate.DayOfWeek;

                // Находим или создаём Schedule
                int scheduleID = GetOrCreateSchedule(sectionID, dayOfWeek);

                foreach (DataGridViewRow row in dgvStudents.Rows)
                {
                    if (row.Cells["StudentCard"].Value != null)
                    {
                        string card = row.Cells["StudentCard"].Value.ToString();

                        if (string.IsNullOrEmpty(card)) continue;

                        try
                        {
                            // Получаем значение чекбокса и примечание
                            bool isAbsent = row.Cells["Absent"].Value != null &&
                                           Convert.ToBoolean(row.Cells["Absent"].Value);
                            string notes = row.Cells["Notes"].Value?.ToString() ?? "";

                            // Проверяем, есть ли уже запись
                            var existingAttendanceID = DBConnection.Instance.ExecuteScalar(
                                @"SELECT a.AttendanceID 
                                  FROM Attendance a 
                                  WHERE a.StudentCardNumber = @Card 
                                  AND a.ScheduleID = @ScheduleID 
                                  AND CAST(a.VisitDate AS DATE) = @Date",
                                new[] {
                                    new SqlParameter("@Card", card),
                                    new SqlParameter("@ScheduleID", scheduleID),
                                    new SqlParameter("@Date", visitDate.Date)
                                });

                            if (existingAttendanceID == null || existingAttendanceID == DBNull.Value)
                            {
                                // Создаём новую запись
                                int status = isAbsent ? 0 : 1;

                                DBConnection.Instance.ExecuteCommand(
                                    @"INSERT INTO Attendance (StudentCardNumber, ScheduleID, VisitDate, Status, Notes) 
                                      VALUES (@Card, @ScheduleID, @Date, @Status, @Notes)",
                                    new[] {
                                        new SqlParameter("@Card", card),
                                        new SqlParameter("@ScheduleID", scheduleID),
                                        new SqlParameter("@Date", visitDate.Date),
                                        new SqlParameter("@Status", status),
                                        new SqlParameter("@Notes", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes)
                                    });
                                saved++;
                            }
                            else
                            {
                                // Обновляем существующую запись
                                int attendanceID = Convert.ToInt32(existingAttendanceID);
                                int status = isAbsent ? 0 : 1;

                                DBConnection.Instance.ExecuteCommand(
                                    @"UPDATE Attendance 
                                      SET Status = @Status, Notes = @Notes 
                                      WHERE AttendanceID = @ID",
                                    new[] {
                                        new SqlParameter("@Status", status),
                                        new SqlParameter("@Notes", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes),
                                        new SqlParameter("@ID", attendanceID)
                                    });
                                updated++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Ошибка для студента {card}: {ex.Message}");
                        }
                    }
                }

                MessageBox.Show($"✅ Сохранено: {saved} записей создано, {updated} обновлено",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка сохранения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetOrCreateSchedule(int sectionID, int dayOfWeek)
        {
            try
            {
                // Ищем существующий Schedule
                var existingScheduleID = DBConnection.Instance.ExecuteScalar(
                    "SELECT ScheduleID FROM Schedule WHERE SectionID = @SectionID AND DayOfWeek = @DayOfWeek",
                    new[] {
                new SqlParameter("@SectionID", sectionID),
                new SqlParameter("@DayOfWeek", dayOfWeek)
                    });

                if (existingScheduleID != null && existingScheduleID != DBNull.Value)
                {
                    return Convert.ToInt32(existingScheduleID);
                }

                // Создаём новый Schedule
                DBConnection.Instance.ExecuteCommand(
                    @"INSERT INTO Schedule (SectionID, DayOfWeek, StartTime, EndTime) 
              VALUES (@SectionID, @DayOfWeek, '09:00', '10:30')",
                    new[] {
                new SqlParameter("@SectionID", sectionID),
                new SqlParameter("@DayOfWeek", dayOfWeek)
                    });

                
                var newScheduleID = DBConnection.Instance.ExecuteScalar(
                    "SELECT SCOPE_IDENTITY()");

                if (newScheduleID == null || newScheduleID == DBNull.Value)
                {
                    
                    newScheduleID = DBConnection.Instance.ExecuteScalar(
                        "SELECT TOP 1 ScheduleID FROM Schedule WHERE SectionID = @SectionID AND DayOfWeek = @DayOfWeek ORDER BY ScheduleID DESC",
                        new[] {
                    new SqlParameter("@SectionID", sectionID),
                    new SqlParameter("@DayOfWeek", dayOfWeek)
                        });
                }

                if (newScheduleID != null && newScheduleID != DBNull.Value)
                {
                    return Convert.ToInt32(newScheduleID);
                }
                else
                {
                    throw new Exception("Не удалось получить ID созданного расписания");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка создания/получения расписания: " + ex.Message);
            }
        }
    }
}