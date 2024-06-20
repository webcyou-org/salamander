using System;
using System.Collections.Generic;

public class VDom
{
    private static bool isFlushing = false;
    private static bool isFlushPending = false;
    private static Queue<Action> queue = new Queue<Action>();

    private static void NextTick(Action job)
    {
        Task.Run(job);
    }

    public static VNode CreateVNode(string type = "", Dictionary<string, object> props = null, object children = null)
    {
        return new VNode(type, props, children);
    }

    public static bool IsVNode(object node)
    {
        return node is VNode;
    }

    public static void QueueJob(Action job)
    {
        if (!queue.Contains(job))
        {
            queue.Enqueue(job);
            QueueFlush();
        }
    }

    private static void QueueFlush()
    {
        if (!isFlushPending && !isFlushing)
        {
            isFlushPending = true;
            NextTick(FlushJobs);
        }
    }

    private static void FlushJobs()
    {
        isFlushing = true;
        isFlushPending = false;

        while (queue.Count > 0)
        {
            var job = queue.Dequeue();
            job();
        }

        isFlushing = false;
    }

    public static void Patch(VNode n1, VNode n2, object container)
    {
        object el;

        if (n1.Type != n2.Type)
        {
            el = n2.El = NodeOps.Create(n2.Type);
            NodeOps.Append(container, el);
        }
        else
        {
            el = n2.El = n1.El;
        }

        foreach (var key in n2.Props.Keys)
        {
            var prevProps = n1.Props.ContainsKey(key) ? n1.Props[key] : null;
            var nextProps = n2.Props[key];

            if (!Equals(prevProps, nextProps))
            {
                if (key.StartsWith("on"))
                {
                    if (prevProps == null || prevProps.ToString() != nextProps.ToString())
                    {
                        // NodeOps.On(el, key.Substring(2).ToLower(), () => nextProps);
                    }
                }
                else
                {
                    NodeOps.SetAttr(el, key, nextProps);
                }
            }
        }

        if (n2.Children is List<VNode> children)
        {
            for (int i = 0; i < children.Count; ++i)
            {
                if (i < (n1.Children as List<VNode>)?.Count)
                {
                    var oldChild = (n1.Children as List<VNode>)[i];
                    var newChild = children[i];

                    if (IsVNode(oldChild) && IsVNode(newChild))
                    {
                        Patch(oldChild, newChild, el);
                    }
                }
                else
                {
                    Patch(CreateVNode(), children[i], el);
                }
            }
        }
        else
        {
            if (!Equals(n1.Children, n2.Children))
            {
                NodeOps.Html(el, n2.Children);
            }
        }
    }
}
