using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Lib.DB;

namespace PolesSU_Sports.Admin.Attendance
{
    public partial class AttendanceEditForm : Form
    {
        private int attendanceID;
        private string studentCard;
        private DateTime visitDate;

        public AttendanceEditForm(int id, string card, DateTime date)
        {
            attendanceID = id;
            studentCard = card;
            visitDate = date;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Редактирование посещения";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label
            {
                Text = "Статус посещения",
                Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            RadioButton rbPresent = new RadioButton
            {
                Text = "✅ Присутствовал",
                Location = new Point(20, 60),
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 10),
                Checked = true
            };

            RadioButton rbAbsent = new RadioButton
            {
                Text = "❌ Отсутствовал",
                Location = new Point(20, 90),
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            Label lblNotes = new Label
            {
                Text = "Примечание:",
                Location = new Point(20, 125),
                AutoSize = true
            };

            TextBox txtNotes = new TextBox
            {
                Location = new Point(20, 145),
                Size = new Size(340, 23),
                PlaceholderText = "Необязательно"
            };

            Button btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(20, 180),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 86, 179),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += (s, e) =>
            {
                int status = rbPresent.Checked ? 1 : 0;
                DBConnection.Instance.ExecuteCommand(
                    "UPDATE Attendance SET Status = @Status, Notes = @Notes WHERE AttendanceID = @ID",
                    new[] {
                        new SqlParameter("@Status", status),
                        new SqlParameter("@Notes", txtNotes.Text),
                        new SqlParameter("@ID", attendanceID)
                    });
                this.DialogResult = DialogResult.OK;
            };

            Button btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(150, 180),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lblTitle, rbPresent, rbAbsent, lblNotes, txtNotes, btnSave, btnCancel });
        }
    }
}