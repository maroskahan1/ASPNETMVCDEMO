﻿using ASPNETMVCDEMO.Models.Domain;

namespace ASPNETMVCDEMO.Models
{
    public class JobExecutionHistoryViewModel
    {
        public DateTime ExecutionDate;
        public int StatusCode;
        public string Response { get; set; }
        public bool Success { get; set; }
    }
}
