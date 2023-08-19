using System.Diagnostics;

public class Program
{
    public static void Main(string[] argv)
    {
        var r = new Random();
        var d = new Dictionary<int, int>();

        var sw1 = new Stopwatch();
        sw1.Start();
        for (var i = 0; i < 10_000_000; i ++)
            d.Add(i, 0);
        sw1.Stop();

        var c = 0;
        var sw2 = new Stopwatch();
        sw2.Start();
        for (var i = 0; i < 10_000_000; i++)
        {
            var ok = d.TryGetValue(r.Next(100_000_000), out _);
            if (ok)
                c++;
        }
        sw2.Stop();
        
        Console.WriteLine($"{sw1.ElapsedMilliseconds} {sw2.ElapsedMilliseconds} {c}");
    }
}