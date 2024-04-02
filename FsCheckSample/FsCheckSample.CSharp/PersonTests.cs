using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Xunit.Abstractions;

// Setting MaxTest = 1, we'd get AutoFixture-like behavior, except AutoFixture
// may also integrates mocking. With the AutoData attribute, AutoFixture test
// methods may take as parameters frozen mock instances that we may define
// expectations on. The frozen mock instances are the same instances returned
// inside the system under test.
//
// AutoFixture may also integrate with FsCheck for test data generation. In
// comparison to FsCheck, AutoFixture generators are primitive, and without
// shrinking support.
//
// In an OO codebase, AutoFixture with mock and FsCheck support is likely
// preferred over only FsCheck.
[assembly:Properties(Arbitrary = [typeof(FsCheckSample.CSharp.Arbitraries)])]

namespace FsCheckSample.CSharp;

using Xunit;

public class Name
{
    public const int MinLength = 1;
    public const int MaxLength = 30;

    public string Value { get; }

    public Name(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Must not be null or whitespace.");
        Value = value.Length switch
        {
            < MinLength => throw new ArgumentException($"Must be at least length {MinLength}."),
            > MaxLength => throw new ArgumentException($"Must be less than or equal length {MaxLength}."),
            _ => value
        };
    }

    public override string ToString() => Value;
}

public class Age(uint value)
{
    public const uint Min = 18;
    public const uint Max = 120;
    
    public uint Value { get; } = value switch
    {
        < Min => throw new ArgumentException($"Must be at least age {Min}."),
        > Max => throw new ArgumentException($"Must be less than or below age {Max}"),
        _ => value
    };

    // Downside of C# code is less specific error reporting, printing the type:
    //
    //     Falsifiable, after 1 test (0 shrinks) (StdGen (1869442622,297310331)):
    //     Original:
    //     FsCheckSample.CSharp.Age
    // 
    // Without a ToString() override, or calling the Label method inside a
    // property, the error message include the default ToString() return. 
    public override string ToString() => Value.ToString();
}

// Could be turned into a record with custom equality implementation. This is
// an entity, not a value object.
public class Person(Name name, Age age)
{
    public Name Name { get; } = name;
    public Age Age { get; } = age;

    public override string ToString() => $"Name: {Name}, Age: {Age}";
}

// [Properties(Arbitrary = [typeof(Arbitraries)])]
static class Arbitraries
{
    // In an actual app, the .NET property would've been named Name. We can't
    // do that here without the domain and the property name conflicting.
    //
    // This would define generators, but they don't come with build-in
    // shrinkers like arbitraries do.
    public static Gen<Name> NameGen =>
        from s in ArbMap.Default.GeneratorFor<NonWhiteSpaceString>()
        where s.Get.Length is >= Name.MinLength and <= Name.MaxLength
        select new Name(s.Get);
    
    public static Gen<Age> AgeGen =>
        from a in ArbMap.Default.GeneratorFor<uint>()
        where a is >= Age.Min and <= Age.Max
        select new Age(a);

    public static Gen<Person> PersonGen =>
        from name in NameGen
        from age in AgeGen
        select new Person(name, age);
    
    // TODO: When to create Arbs vs Gens?
    //       With Gens alone, the PersonGen is required.
    //       With Arbs, Person can be inferred.
    
    public static Arbitrary<Name> Names() =>
        // TODO: How to define Arb with shrinker?
        // TODO: How to make it similar to the F# version? Should it be?
        Arb.From(ArbMap.Default.GeneratorFor<NonWhiteSpaceString>()
            .Where(s => s.Get.Length is >= Name.MinLength and <= Name.MaxLength)
            .Select(s => new Name(s.Get)));

    public static Arbitrary<Age> Ages() =>
        Arb.From(ArbMap.Default.GeneratorFor<uint>()
            .Where(a => a is >= Age.Min and <= Age.Max)
            .Select(a => new Age(a)));

    // Generation of Person instances is inferred from default and custom Name
    // and Age arbitraries. Only if Person specific constraints apply would we
    // need an explicit Arb<Person>.
    // public static Arbitrary<Person> People() =>
    //     from firstname in Arb.Generate<Firstname>()
    //     from age in Arb.Generate<Age>()
    //     select new Person(firstname, age);
}

public class PersonTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Must_be_valid_example_1()
    {
        // In a real codebase, this test would be redundant as Name creation is
        // exercised by the person test.
        //
        // Without registering Arbitraries, FsCheck will recursively construct
        // a Name instance using ArbMap.Default. Generation will use the
        // default string generator with Name's string constructor. It
        // generates null or whitespace so Name constructor throws an
        // exception.
        Prop.ForAll((Name name, Age age) => true)
            .Check(Config.Default.WithArbitrary([typeof(Arbitraries)]));
        
        // Alternatively arbitraries arguments without custom config.
        Prop.ForAll(
                Arbitraries.Names(),
                Arbitraries.Ages(),
                (name, age) => true)
            .QuickCheckThrowOnFailure();
        
        // Alternatively arbitraries from generators arguments without custom
        // config.
        Prop.ForAll(
                Arbitraries.NameGen.ToArbitrary(),
                Arbitraries.AgeGen.ToArbitrary(),
                Arbitraries.PersonGen.ToArbitrary(),
                (name, age, person) => true)
            .QuickCheckThrowOnFailure();
    }
    
    [Property]
    public void Must_be_valid_example_2()
    {
        // Using the property, custom arbitraries may be included at type or
        // assembly level.
        //
        // Even though the test passes, it's wrong in other ways. Because of
        // the property attribute, it's called upto MaxTest times as each run
        // of Prop.ForAll also generating upto MaxTest tests.
        //
        // Only use Prop.ForAll inside Fact attribute tests.
        //
        // The below property is expected to fail.
        Prop.ForAll((Name _) => true)
            .QuickCheckThrowOnFailure();
    }
    
    [Property(MaxTest = 3)]
    public void Must_be_valid_example_X()
    {
        // Don't use Property attribute together with Prop.ForAll.
        testOutputHelper.WriteLine("Outside");
        Prop.ForAll(
                (int _) =>
                {
                    testOutputHelper.WriteLine("inline");
                    return true;
                })
            .Check(Config.Default.WithMaxTest(2));
    }    

    [Property]
    public void Must_be_valid_example_3(Name name, Age age, Person person)
    {
        // Use Person instance in a larger context, such as writing and reading
        // it to and from a database.
        Assert.True(name.Value.Length >= 1);
        Assert.True(age.Value >= 18);
        testOutputHelper.WriteLine(person.ToString());
    }
}