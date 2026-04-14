using System;

namespace PolesSU_Sports.Lib.Model
{
    public enum AccountRole
    {
        Administrator,
        Trainer,
        Student,
        Manager
    }

    public class Account
    {
        public int AccountID { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public AccountRole Role { get; set; }
        public string StudentCardNumber { get; set; }
        public int? TrainerID { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }

        // Для отображения
        public string RoleText
        {
            get
            {
                switch (Role)
                {
                    case AccountRole.Administrator: return "Администратор";
                    case AccountRole.Trainer: return "Тренер";
                    case AccountRole.Student: return "Студент";
                    case AccountRole.Manager: return "Руководитель";
                    default: return Role.ToString();
                }
            }
        }

        public string StatusText => IsActive ? "✅ Активен" : "❌ Заблокирован";
    }
}