using System;

namespace PolesSU_Sports.Lib.Model
{
    public enum UserRole
    {
        Administrator,
        Trainer,
        Student,
        Manager
    }

    public class User
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLogin { get; set; }

        // Текущий авторизованный пользователь
        private static User _currentUser;

        public static User CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        public static bool IsLoggedIn => _currentUser != null;

        public static void Logout()
        {
            _currentUser = null;
        }
    }
}