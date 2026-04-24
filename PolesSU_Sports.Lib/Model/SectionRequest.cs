namespace PolesSU_Sports.Lib.Model
{
    public class SectionRequest
    {
        public int RequestID { get; set; }
        public string StudentCardNumber { get; set; }
        public int SectionID { get; set; }
        public string RequestType { get; set; }  // Join или Leave
        public string Status { get; set; }  // Pending, Approved, Rejected
        public DateTime RequestDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public int? ManagerID { get; set; }
        public string RejectionReason { get; set; }

        // Дополнительные поля для отображения
        public string StudentName { get; set; }
        public string SectionName { get; set; }
        public string GroupName { get; set; }
    }
}