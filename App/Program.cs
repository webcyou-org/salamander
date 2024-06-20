using System;
using System.Threading.Tasks;

// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

using System.ComponentModel;

public class Program
{
    // public static async Task Main(string[] args)
    // {
    //     // var jobQueue = new JobQueue();
    //     //
    //     // jobQueue.QueueJob(() => Console.WriteLine("Job 1 executed"));
    //     // jobQueue.QueueJob(() => Console.WriteLine("Job 2 executed"));
    //     // await Task.Delay(100);
    //     
    //     var oldNode = VDom.CreateVNode("div", new Dictionary<string, object> { { "id", "old" } }, "Hello World");
    //     var newNode = VDom.CreateVNode("div", new Dictionary<string, object> { { "id", "new" } }, "Hello Universe");
    //
    //     var container = new object();
    //     VDom.Patch(oldNode, newNode, container);
    //     
    //     Console.WriteLine("Patched VNode");
    // }
    
    public static void Main()
    {
        IPerson person = new Person
        {
            Name = "John Doe",
            Age = 30
        };

        var reactivePerson = DeepReactive<IPerson>.Create(person);
        ((DeepReactive<IPerson>)(object)reactivePerson).AddPropertyChangedHandler((sender, args) =>
        {
            Console.WriteLine($"{args.PropertyName} has changed.");
        });

        reactivePerson.Name = "Jane Doe";
        reactivePerson.Age = 35;

        Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
    }
}