using Bogus;

namespace Ivy.Samples.Shared.Helpers;

public class User
{
    public required string Name { get; init; }
    public required int Age { get; init; }
    public required string Email { get; init; }
    public required double Salary { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateOnly BirthDate { get; init; }
}

public static class SampleData
{
    public static User[] GetUsers(int amount)
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Age, f => f.Random.Number(18, 65))
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Salary, f => f.Random.Double(30000, 150000))
            .RuleFor(u => u.IsActive, f => f.Random.Bool())
            .RuleFor(u => u.CreatedAt, f => f.Date.Between(DateTime.Now.AddYears(-2), DateTime.Now))
            .RuleFor(u => u.BirthDate, f => DateOnly.FromDateTime(f.Date.Between(DateTime.Now.AddYears(-65), DateTime.Now.AddYears(-18))));

        return userFaker.Generate(amount).ToArray();
    }
}