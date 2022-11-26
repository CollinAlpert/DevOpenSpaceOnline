namespace Test;

public class Program
{
	public static void Main(string[] args)
	{
		var person = new Person("Steve", 23, new Person.Address());
		Console.WriteLine(person.Name);
	}
}

public partial class Person
{
	private readonly string _name;
	private readonly int _age;
	private readonly Address _homeAddress;
	
	internal class Address
	{

	}
}

