namespace Ums.Shell.Ddd.Test.Dtos
{
    public class AuditDto
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ComplexityLevel { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string TimeSpan { get; set; }
    }
}
