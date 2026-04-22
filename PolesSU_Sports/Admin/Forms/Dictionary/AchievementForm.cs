using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Lib.DB;

namespace PolesSU_Sports.Admin.Forms.Dictionary.Achievements
{
    public partial class AchievementForm : Form
    {
        private string studentCardNumber;
        private int? achievementId;

        public AchievementForm()
        {
            // Добавление нового достижения
            InitializeComponent();
            InitializeForm("Добавить достижение");
            LoadStudents();
            LoadSections();
        }

        public AchievementForm(string studentCard)
        {
            // Добавление достижения для конкретного студента
            InitializeComponent();
            studentCardNumber = studentCard;
            InitializeForm("Добавить достижение");
            LoadStudents();
            LoadSections();

            if (cmbStudents.SelectedValue != null)
            {
                cmbStudents.SelectedValue = studentCard;
            }
        }

        public AchievementForm(int achievementID)
        {
            InitializeComponent();
            achievementId = achievementID;
            InitializeForm("Редактировать достижение");
            LoadStudents();
            LoadSections();
            LoadAchievementData();
        }

        private void InitializeForm(string title)
        {
            this.Text = title;
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            // Студент
            Label lblStudent = new Label
            {
                Text = "Студент:",
                Location = new Point(20, 70),
                AutoSize = true
            };

            cmbStudents = new ComboBox
            {
                Location = new Point(170, 67),
                Size = new Size(380, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Секция (необязательно)
            Label lblSection = new Label
            {
                Text = "Секция (необязательно):",
                Location = new Point(20, 110),
                AutoSize = true
            };

            cmbSections = new ComboBox
            {
                Location = new Point(170, 107),
                Size = new Size(380, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Название соревнования
            Label lblCompetition = new Label
            {
                Text = "Название соревнования:",
                Location = new Point(20, 150),
                AutoSize = true
            };

            txtCompetition = new TextBox
            {
                Location = new Point(170, 147),
                Size = new Size(380, 23)
            };

            // Дата соревнования
            Label lblDate = new Label
            {
                Text = "Дата:",
                Location = new Point(20, 190),
                AutoSize = true
            };

            dtpCompetitionDate = new DateTimePicker
            {
                Location = new Point(170, 187),
                Size = new Size(200, 23),
                Format = DateTimePickerFormat.Short
            };

            // Место
            Label lblPlace = new Label
            {
                Text = "Место:",
                Location = new Point(20, 230),
                AutoSize = true
            };

            numPlace = new NumericUpDown
            {
                Location = new Point(170, 227),
                Size = new Size(100, 23),
                Minimum = 1,
                Maximum = 100
            };

            // Тип награды
            Label lblAwardType = new Label
            {
                Text = "Тип награды:",
                Location = new Point(20, 270),
                AutoSize = true
            };

            cmbAwardType = new ComboBox
            {
                Location = new Point(170, 267),
                Size = new Size(380, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbAwardType.Items.AddRange(new object[] {
                "Грамота",
                "Диплом",
                "Медаль",
                "Кубок",
                "Сертификат",
                "Другое"
            });

            // Описание
            Label lblDescription = new Label
            {
                Text = "Описание:",
                Location = new Point(20, 310),
                AutoSize = true
            };

            txtDescription = new TextBox
            {
                Location = new Point(170, 307),
                Size = new Size(380, 80),
                Multiline = true,
                AcceptsReturn = true
            };

            // Кнопки
            Button btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(150, 410),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 86, 179),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(290, 410),
                Size = new Size(120, 35),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] {
                lblTitle, lblStudent, cmbStudents, lblSection, cmbSections,
                lblCompetition, txtCompetition, lblDate, dtpCompetitionDate,
                lblPlace, numPlace, lblAwardType, cmbAwardType,
                lblDescription, txtDescription, btnSave, btnCancel
            });
        }

        private ComboBox cmbStudents;
        private ComboBox cmbSections;
        private TextBox txtCompetition;
        private DateTimePicker dtpCompetitionDate;
        private NumericUpDown numPlace;
        private ComboBox cmbAwardType;
        private TextBox txtDescription;

        private void LoadStudents()
        {
            DataTable dt = DBConnection.Instance.ExecuteQuery(@"
                SELECT StudentCardNumber, 
                    LastName + ' ' + FirstName + ' ' + ISNULL(MiddleName, '') AS FullName
                FROM Students
                ORDER BY LastName");

            cmbStudents.DataSource = dt;
            cmbStudents.DisplayMember = "FullName";
            cmbStudents.ValueMember = "StudentCardNumber";
        }

        private void LoadSections()
        {
            DataTable dt = DBConnection.Instance.ExecuteQuery(@"
                SELECT SectionID, SectionName
                FROM Sections
                ORDER BY SectionName");

            cmbSections.DataSource = dt;
            cmbSections.DisplayMember = "SectionName";
            cmbSections.ValueMember = "SectionID";

            // Добавляем пустой вариант
            DataRow row = dt.NewRow();
            row["SectionID"] = DBNull.Value;
            row["SectionName"] = "-- Не выбрано --";
            dt.Rows.InsertAt(row, 0);

            cmbSections.SelectedValue = 0;
        }

        private void LoadAchievementData()
        {
            if (achievementId == null) return;

            DataTable dt = DBConnection.Instance.ExecuteQuery(@"
                SELECT StudentCardNumber, SectionID, CompetitionName, CompetitionDate, 
                    Place, AwardType, AwardDescription
                FROM Achievements
                WHERE AchievementID = @ID",
                new[] { new SqlParameter("@ID", achievementId.Value) });

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                if (cmbStudents.SelectedValue != null)
                {
                    cmbStudents.SelectedValue = row["StudentCardNumber"].ToString();
                }

                if (row["SectionID"] != DBNull.Value)
                {
                    cmbSections.SelectedValue = row["SectionID"];
                }

                txtCompetition.Text = row["CompetitionName"].ToString();
                dtpCompetitionDate.Value = Convert.ToDateTime(row["CompetitionDate"]);

                if (row["Place"] != DBNull.Value)
                {
                    numPlace.Value = Convert.ToInt32(row["Place"]);
                }

                if (row["AwardType"] != DBNull.Value)
                {
                    cmbAwardType.Text = row["AwardType"].ToString();
                }

                if (row["AwardDescription"] != DBNull.Value)
                {
                    txtDescription.Text = row["AwardDescription"].ToString();
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbStudents.SelectedValue == null)
            {
                MessageBox.Show("Выберите студента", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCompetition.Text))
            {
                MessageBox.Show("Введите название соревнования", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string studentCard = cmbStudents.SelectedValue.ToString();
                object sectionID = cmbSections.SelectedValue ?? DBNull.Value;
                string competitionName = txtCompetition.Text.Trim();
                DateTime competitionDate = dtpCompetitionDate.Value.Date;
                int place = Convert.ToInt32(numPlace.Value);
                object awardType = string.IsNullOrWhiteSpace(cmbAwardType.Text) ? (object)DBNull.Value : cmbAwardType.Text.Trim();
                object description = string.IsNullOrWhiteSpace(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text.Trim();

                if (achievementId == null)
                {
                    // Добавление
                    DBConnection.Instance.ExecuteCommand(@"
                        INSERT INTO Achievements 
                        (StudentCardNumber, SectionID, CompetitionName, CompetitionDate, Place, AwardType, AwardDescription)
                        VALUES 
                        (@StudentCard, @SectionID, @Competition, @Date, @Place, @AwardType, @Description)",
                        new[] {
                            new SqlParameter("@StudentCard", studentCard),
                            new SqlParameter("@SectionID", sectionID),
                            new SqlParameter("@Competition", competitionName),
                            new SqlParameter("@Date", competitionDate),
                            new SqlParameter("@Place", place),
                            new SqlParameter("@AwardType", awardType),
                            new SqlParameter("@Description", description)
                        });

                    MessageBox.Show("✅ Достижение добавлено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Обновление
                    DBConnection.Instance.ExecuteCommand(@"
                        UPDATE Achievements SET
                            StudentCardNumber = @StudentCard,
                            SectionID = @SectionID,
                            CompetitionName = @Competition,
                            CompetitionDate = @Date,
                            Place = @Place,
                            AwardType = @AwardType,
                            AwardDescription = @Description
                        WHERE AchievementID = @ID",
                        new[] {
                            new SqlParameter("@StudentCard", studentCard),
                            new SqlParameter("@SectionID", sectionID),
                            new SqlParameter("@Competition", competitionName),
                            new SqlParameter("@Date", competitionDate),
                            new SqlParameter("@Place", place),
                            new SqlParameter("@AwardType", awardType),
                            new SqlParameter("@Description", description),
                            new SqlParameter("@ID", achievementId.Value)
                        });

                    MessageBox.Show("✅ Достижение обновлено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}