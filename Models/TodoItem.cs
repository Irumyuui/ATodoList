using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Models
{
    public class TodoItem(int objectId, string title, DateTime deadLine, string description = "", bool isFinish = false)
    {
        public int ObjectId { get; } = objectId;

        public string Title { get; } = title;
        
        public DateTime DeadLine { get; } = deadLine;
        
        public string Description { get; } = description;
        
        public bool IsFinish { get; } = isFinish;
    }
}
