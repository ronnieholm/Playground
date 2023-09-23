public class Program
{
    private record Input(string AId, string BId, string CId);
    private record C(string Id);
    private record B(string Id, List<C> Cs);
    private record A(string Id, List<B> Bs);
    
    public static void Main()
    {
        var input = new Input[]
        {
            new("A1","B1","C1"),
            new("A1","B1","C2"),
            new("A1","B2","C3"),
            new("A1","B2","C4"),
        };

        // We know a priori the structure of the response, i.e., how many levels to iterate.

        var lastA = "";
        var lastB = "";
        var lastC = "";
        var accA = new List<A>();
        var accB = new List<B>();
        var accC = new List<C>();
        
        for (var i = 0; i < input.Length; i++)
        {
            if (i == 0)
            {
                // TODO: Assumes B or C isn't missing for this A.
                lastA = input[0].AId;
                lastB = input[0].BId;
                lastC = input[0].CId;
                
                // Add leaf node
                accC.Add(new C(lastC));
            }
            else
            {
                if (lastA != input[i].AId)
                {
                    lastA = input[i].AId;
                    accA.Add(new A(lastA, accB));
                    accB = new List<B>();
                    accC = new List<C>();
                }
                if (lastB != input[i].BId)
                {
                    lastB = input[i].BId;
                    accB.Add(new B(lastB, accC));
                    accC = new List<C>();
                }
                if (lastC != input[i].CId)
                {
                    lastC = input[i].CId;
                    accC.Add(new C(lastC));
                }
            }

            Console.WriteLine($"{lastA} {lastB} {lastC}");
        }

        // No more results
        accB.Add(new B(lastB, accC));
        accA.Add(new A(lastA, accB));
    }
}