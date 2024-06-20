using System;
using System.Threading.Tasks;

// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

public class Program
{
    public static async Task Main(string[] args)
    {
        // var jobQueue = new JobQueue();
        //
        // jobQueue.QueueJob(() => Console.WriteLine("Job 1 executed"));
        // jobQueue.QueueJob(() => Console.WriteLine("Job 2 executed"));
        // await Task.Delay(100);
        
        var oldNode = VDom.CreateVNode("div", new Dictionary<string, object> { { "id", "old" } }, "Hello World");
        var newNode = VDom.CreateVNode("div", new Dictionary<string, object> { { "id", "new" } }, "Hello Universe");

        var container = new object();
        VDom.Patch(oldNode, newNode, container);
        
        Console.WriteLine("Patched VNode");
    }
}