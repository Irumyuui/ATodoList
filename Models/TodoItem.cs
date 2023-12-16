using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Models
{
    public class TodoItem(int objectId, string title, DateTime? deadLine = null, string description = "", bool isFinish = false)
    {
        public int ObjectId { get; } = objectId;

        public string Title { get; set; } = title;
        
        public DateTime? DeadLine { get; set; } = deadLine;
        
        public string Description { get; set; } = description;
        
        public bool IsFinish { get; set; } = isFinish;
    }
}
