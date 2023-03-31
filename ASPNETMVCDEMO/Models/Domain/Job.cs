namespace ASPNETMVCDEMO.Models.Domain
{
    public class Job 
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfLastStart { get; set; }
        public int Interval { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }
}
