using System.Collections.Generic;

namespace VismaBookLibrary.Models
{
    public class DataTransferHelper
    {
        public string Message { get; set; }

        public bool NeedsAnswerCollecting { get; set; }
        
        public List<string> Requests { get; set; }
    }
}