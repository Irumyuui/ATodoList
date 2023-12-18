using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.Models;

/// <summary>
/// 待办事项分组
/// </summary>
/// <param name="name">组名</param>
public class TodoGroupItem(string name)
{
    public string Name { get; set; } = name;
}
