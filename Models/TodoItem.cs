using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Models
{
    public class TodoItem(int objectId, string title, DateTimeOffset deadLine, string description = "", bool isFinish = false)
    {
        public int ObjectId { get; } = objectId;

        public string Title { get; } = title;
        
        public DateTimeOffset DeadLine { get; } = deadLine;
        
        public string Description { get; } = description;
        
        public bool IsFinish { get; } = isFinish;
    }
}
