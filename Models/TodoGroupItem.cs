using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Models
{
    public class TodoGroupItem
    {
        public TodoGroupItem(string name) { Name = name; }

        public string Name { get; set; }
    }
}
