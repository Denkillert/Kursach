namespace PolesSU_Sports.Lib.Model
{
    public class Section
    {
        public int SectionID { get; set; }
        public string SectionName { get; set; }
        public int SportID { get; set; }
        public string SportName { get; set; }  
        public int TrainerID { get; set; }
        public string TrainerName { get; set; }  
        public int? MaxStudents { get; set; }
        public decimal PricePerMonth { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
    }
}