// Form1.Designer.cs

using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    partial class Form1
    {
        private Button btnVisitor;
        private Button btnStaff;
        private Label lblTitle;
        private Panel panel1;
        private Label lblSubtitle;

        private void InitializeComponent()
        {
            this.btnVisitor = new Button();
            this.btnStaff = new Button();
            this.lblTitle = new Label();
            this.panel1 = new Panel();
            this.lblSubtitle = new Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();

            // btnVisitor
            this.btnVisitor.BackColor = Color.FromArgb(0, 153, 255);
            this.btnVisitor.FlatAppearance.BorderSize = 0;
            this.btnVisitor.FlatStyle = FlatStyle.Flat;
            this.btnVisitor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnVisitor.ForeColor = Color.White;
            this.btnVisitor.Location = new Point(250, 200);
            this.btnVisitor.Name = "btnVisitor";
            this.btnVisitor.Size = new Size(400, 70);
            this.btnVisitor.TabIndex = 0;
            this.btnVisitor.Text = "👤 ПОСЕТИТЕЛЬ";
            this.btnVisitor.UseVisualStyleBackColor = false;
            this.btnVisitor.Click += new EventHandler(this.btnVisitor_Click);
            this.btnVisitor.MouseEnter += new EventHandler(this.btnVisitor_MouseEnter);
            this.btnVisitor.MouseLeave += new EventHandler(this.btnVisitor_MouseLeave);

            // btnStaff
            this.btnStaff.BackColor = Color.FromArgb(76, 175, 80);
            this.btnStaff.FlatAppearance.BorderSize = 0;
            this.btnStaff.FlatStyle = FlatStyle.Flat;
            this.btnStaff.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnStaff.ForeColor = Color.White;
            this.btnStaff.Location = new Point(250, 300);
            this.btnStaff.Name = "btnStaff";
            this.btnStaff.Size = new Size(400, 70);
            this.btnStaff.TabIndex = 1;
            this.btnStaff.Text = "👥 ПЕРСОНАЛ";
            this.btnStaff.UseVisualStyleBackColor = false;
            this.btnStaff.Click += new EventHandler(this.btnStaff_Click);
            this.btnStaff.MouseEnter += new EventHandler(this.btnStaff_MouseEnter);
            this.btnStaff.MouseLeave += new EventHandler(this.btnStaff_MouseLeave);

            // lblTitle
            this.lblTitle.Dock = DockStyle.Top;
            this.lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.Location = new Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(900, 80);
            this.lblTitle.TabIndex = 3;
            this.lblTitle.Text = "СИСТЕМА УЧЕТА ГРЕБНЫХ БАЗ";
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // lblSubtitle
            this.lblSubtitle.Dock = DockStyle.Top;
            this.lblSubtitle.Font = new Font("Segoe UI", 12F);
            this.lblSubtitle.ForeColor = Color.White;
            this.lblSubtitle.Location = new Point(0, 80);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new Size(900, 40);
            this.lblSubtitle.TabIndex = 4;
            this.lblSubtitle.Text = "Выберите тип входа в систему";
            this.lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;

            // panel1
            this.panel1.BackColor = Color.FromArgb(0, 102, 204);
            this.panel1.Controls.Add(this.lblSubtitle);
            this.panel1.Controls.Add(this.lblTitle);
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(900, 120);
            this.panel1.TabIndex = 5;

            // Form1
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.ClientSize = new Size(900, 600);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnStaff);
            this.Controls.Add(this.btnVisitor);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Система учета гребных баз";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

    }
}
