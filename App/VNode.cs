using System;
using System.Collections.Generic;

public class VNode
{
    public string Type { get; set; }
    public Dictionary<string, object> Props { get; set; }
    public object Children { get; set; }
    public object El { get; set; }

    public VNode(string type = "", Dictionary<string, object> props = null, object children = null)
    {
        Type = type;
        Props = props ?? new Dictionary<string, object>();
        Children = children;
    }
}