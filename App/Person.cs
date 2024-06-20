using System;
using System.ComponentModel;

public interface IPerson : INotifyPropertyChanged
{
    string Name { get; set; }
    int Age { get; set; }
}

public class Person : IPerson
{
    private string name;
    private int age;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name
    {
        get => name;
        set
        {
            if (name != value)
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public int Age
    {
        get => age;
        set
        {
            if (age != value)
            {
                age = value;
                OnPropertyChanged(nameof(Age));
            }
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
