using System;

namespace PolesSU_Sports.Shared.Model
{
    public class Schedule
    {
        public int ScheduleID { get; set; }
        public int SectionID { get; set; }
        public string SectionName { get; set; }  // Для отображения
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Room { get; set; }

        // День недели текстом
        public string DayOfWeekText
        {
            get
            {
                string[] days = { "", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };
                return DayOfWeek >= 1 && DayOfWeek <= 7 ? days[DayOfWeek] : "";
            }
        }

        // Время в формате строки
        public string TimeText => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    }
}