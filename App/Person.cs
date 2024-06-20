public class Person : ReactiveObject
{
    private string _name;
    private int _age;
    private Address _address;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value, nameof(Name));
    }

    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value, nameof(Age));
    }

    public Address Address
    {
        get => _address;
        set => SetProperty(ref _address, value, nameof(Address));
    }
}

public class Address : ReactiveObject
{
    private string _street;
    private string _city;

    public string Street
    {
        get => _street;
        set => SetProperty(ref _street, value, nameof(Street));
    }

    public string City
    {
        get => _city;
        set => SetProperty(ref _city, value, nameof(City));
    }
}