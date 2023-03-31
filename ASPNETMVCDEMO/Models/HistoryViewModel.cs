namespace ASPNETMVCDEMO.Models
{
    public class HistoryViewModel
    {
        public string JobName { get; set; }
        public Guid JobId { get; set; }
        public List<JobExecutionHistoryViewModel>  JobExecutionHistoryList { get; set; }
    }
}
