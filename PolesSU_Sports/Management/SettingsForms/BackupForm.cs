using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Shared.DB;

namespace PolesSU_Sports.Management.SettingsForms
{
    public partial class BackupForm : Form
    {
        public BackupForm()
        {
            InitializeComponent();
            LoadBackupHistory();
        }

        private void InitializeComponent()
        {
            this.Text = "Резервное копирование";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label
            {
                Text = "💾 Резервное копирование БД",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            Panel backupPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(750, 120),
                BackColor = Color.FromArgb(245, 245, 245),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblBackup = new Label
            {
                Text = "📦 Создать резервную копию",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true
            };

            Label lblBackupInfo = new Label
            {
                Text = "Создайте полную копию базы данных для восстановления в случае сбоя",
                Location = new Point(15, 45),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            Button btnBackup = new Button
            {
                Text = "📦 Создать копию сейчас",
                Location = new Point(15, 75),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBackup.Click += BtnBackup_Click;

            backupPanel.Controls.AddRange(new Control[] { lblBackup, lblBackupInfo, btnBackup });

            Panel restorePanel = new Panel
            {
                Location = new Point(20, 200),
                Size = new Size(750, 120),
                BackColor = Color.FromArgb(255, 243, 205),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblRestore = new Label
            {
                Text = "🔄 Восстановить из копии",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(230, 126, 34)
            };

            Label lblRestoreInfo = new Label
            {
                Text = "⚠️ Внимание! Восстановление заменит текущие данные. Все изменения после даты копии будут потеряны!",
                Location = new Point(15, 45),
                Size = new Size(500, 40),
                ForeColor = Color.FromArgb(230, 126, 34)
            };

            Button btnRestore = new Button
            {
                Text = "🔄 Восстановить",
                Location = new Point(15, 75),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRestore.Click += BtnRestore_Click;

            restorePanel.Controls.AddRange(new Control[] { lblRestore, lblRestoreInfo, btnRestore });

            Label lblHistory = new Label
            {
                Text = "📋 История резервных копий",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 330),
                AutoSize = true
            };

            DataGridView dgvHistory = new DataGridView
            {
                Location = new Point(20, 360),
                Size = new Size(750, 120),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            Button btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(20, 490),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, backupPanel, restorePanel, lblHistory, dgvHistory, btnClose });
        }

        private void LoadBackupHistory()
        {
            // Загрузка истории из папки с бэкапами
            string backupFolder = Path.Combine(Application.StartupPath, "Backups");
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                string backupFolder = Path.Combine(Application.StartupPath, "Backups");
                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                string backupFile = Path.Combine(backupFolder, $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");

                // SQL Server Backup Command
                string backupQuery = $@"
                    BACKUP DATABASE [SportSectionsPolesSU] 
                    TO DISK = '{backupFile}' 
                    WITH FORMAT, INIT, NAME = 'Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                DBConnection.Instance.ExecuteCommand(backupQuery);

                MessageBox.Show($"✅ Резервная копия создана:\n{backupFile}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadBackupHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка создания копии: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "⚠️ ВНИМАНИЕ!\n\n" +
                "Восстановление заменит ВСЕ текущие данные!\n" +
                "Все изменения после даты резервной копии будут потеряны!\n\n" +
                "Вы уверены, что хотите продолжить?",
                "Подтверждение восстановления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                // Открываем диалог выбора файла
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Backup Files (*.bak)|*.bak|All Files|*.*";
                    ofd.Title = "Выберите файл резервной копии";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string backupFile = ofd.FileName;

                        // SQL Server Restore Command
                        string restoreQuery = $@"
                            USE master;
                            ALTER DATABASE [SportSectionsPolesSU] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            RESTORE DATABASE [SportSectionsPolesSU] 
                            FROM DISK = '{backupFile}' 
                            WITH REPLACE, RECOVERY;
                            ALTER DATABASE [SportSectionsPolesSU] SET MULTI_USER;";

                        DBConnection.Instance.ExecuteCommand(restoreQuery);

                        MessageBox.Show("✅ База данных восстановлена!\nПриложение будет перезапущено.", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Application.Restart();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка восстановления: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}