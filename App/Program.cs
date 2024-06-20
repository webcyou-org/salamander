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
        var address = new Address
        {
            Street = "123 Main St",
            City = "Anytown"
        };

        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            // Address = address
        };

        var reactivePerson = DeepReactive.Create(person);
        reactivePerson.PropertyChanged += (sender, args) =>
        {
            Console.WriteLine($"{args.PropertyName} has changed.");
        };

        reactivePerson.Name = "Jane Doe";
        reactivePerson.Age = 35;
        reactivePerson.Address.Street = "456 Elm St";

        Console.WriteLine($"Name: {person.Name}, Age: {person.Age}, Street: {person.Address.Street}"); // Name: Jane Doe, Age: 35, Street: 456 Elm St
    }
}