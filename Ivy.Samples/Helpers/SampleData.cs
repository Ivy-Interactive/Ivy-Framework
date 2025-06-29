using Bogus;

namespace Ivy.Samples.Helpers;

public class User
{
    public required string Name { get; init; }
    public required int Age { get; init; }
    public required string Email { get; init; }
}

public static class SampleData
{
    public static User[] GetUsers(int amount)
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Age, f => f.Random.Number(18, 65))
            .RuleFor(u => u.Email, f => f.Internet.Email());

        return userFaker.Generate(amount).ToArray();
    }
}