using System;

namespace PolesSU_Sports.Lib.Model
{
    public class Achievement
    {
        public int AchievementID { get; set; }
        public string StudentCardNumber { get; set; }
        public string StudentName { get; set; }  // Для отображения
        public int? SectionID { get; set; }
        public string SectionName { get; set; }  // Для отображения
        public string CompetitionName { get; set; }
        public DateTime CompetitionDate { get; set; }
        public int? Place { get; set; }
        public string AwardType { get; set; }
        public string AwardDescription { get; set; }

        // Место текстом
        public string PlaceText
        {
            get
            {
                if (!Place.HasValue) return "";
                return Place switch
                {
                    1 => "🥇 1 место",
                    2 => "🥈 2 место",
                    3 => "🥉 3 место",
                    _ => $"{Place} место"
                };
            }
        }
    }
}