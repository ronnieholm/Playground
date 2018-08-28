using System;
using System.Diagnostics;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

// TODO: Implement non table solution with

namespace Base64EncoderDecoder.Cli
{
    class Base64
    {
        static readonly char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=".ToCharArray();

        public static string EncodeWithNaive(byte[] source)
        {
            // One-to-one implementation of the algorithm as described on Wikipedia: https://en.wikipedia.org/wiki/Base64. 
            // While easy to follow, it's inefficient compared to the .NET Framework implementation. Not only does that one
            // not create the pattern, but it works directly on the input bytes. It also pre-computes and pre-allocates the 
            // size of the output buffer and uses unsafe code (pointers to arrays) to access the input and output buffers 
            // and eliminate the overhead of range checking. Compared to the implementation below, using a StringBuilder, 
            // that's why the framework implementation allocates less space in the speed and space tests.
            var encoded = new StringBuilder();
            var rest = source.Length % 3;
            var completeTriplets = source.Length - rest;

            var i = 0;
            for (; i < completeTriplets; i += 3)
            {
                int pattern = source[i] << 16 | source[i + 1] << 8 | source[i + 2];
                int a = (pattern & (63 << 18)) >> 18;
                int b = (pattern & (63 << 12)) >> 12;
                int c = (pattern & (63 << 6)) >> 6;
                int d = pattern & 63;
                encoded.Append(Alphabet[a]);
                encoded.Append(Alphabet[b]);
                encoded.Append(Alphabet[c]);
                encoded.Append(Alphabet[d]);
            }

            // Third byte not present in last batch
            if (rest == 2)
            {
                int pattern = source[i] << 16 | source[i + 1] << 8;
                int a = (pattern & (63 << 18)) >> 18;
                int b = (pattern & (63 << 12)) >> 12;
                int c = (pattern & (63 << 6)) >> 6;
                encoded.Append(Alphabet[a]);
                encoded.Append(Alphabet[b]);
                encoded.Append(Alphabet[c]);
                encoded.Append(Alphabet[64]);
            }
            // Second and third bytes not present in last batch
            else if (rest == 1)
            {
                int pattern = source[i] << 16;
                int a = (pattern & (63 << 18)) >> 18;
                int b = (pattern & (63 << 12)) >> 12;
                encoded.Append(Alphabet[a]);
                encoded.Append(Alphabet[b]);
                encoded.Append(Alphabet[64]);
                encoded.Append(Alphabet[64]);
            }

            return encoded.ToString();
        }

        public static char[] EncodeWithPreAllocatedOutputBuffer(byte[] source)
        {
            var rest = source.Length % 3;
            var completeTriplets = source.Length - rest;
            var encoded = new char[completeTriplets / 3 * 4 + (rest != 0 ? 4 : 0)];
            var j = 0;

            var i = 0;
            for (; i < completeTriplets; i += 3)
            {
                int pattern = source[i] << 16 | source[i + 1] << 8 | source[i + 2];
                int a = (pattern & (63 << 18)) >> 18;
                int b = (pattern & (63 << 12)) >> 12;
                int c = (pattern & (63 << 6)) >> 6;
                int d = pattern & 63;
                encoded[j] = Alphabet[a];
                encoded[j + 1] = Alphabet[b];
                encoded[j + 2] = Alphabet[c];
                encoded[j + 3] = Alphabet[d];
                j += 4;
            }

            // Third byte not present in last batch
            if (rest == 2)
            {
                int pattern = source[i] << 16 | source[i + 1] << 8;
                int a = (pattern & (63 << 18)) >> 18;
                int b = (pattern & (63 << 12)) >> 12;
                int c = (pattern & (63 << 6)) >> 6;
                encoded[j] = Alphabet[a];
                encoded[j + 1] = Alphabet[b];
                encoded[j + 2] = Alphabet[c];
                encoded[j + 3] = Alphabet[64];
                j += 4;
            }
            // Second and third bytes not present in last batch
            else if (rest == 1)
            {
                int pattern = source[i] << 16;
                int a = (pattern & (63 << 18)) >> 18;
                int b = (pattern & (63 << 12)) >> 12;
                encoded[j] = Alphabet[a];
                encoded[j + 1] = Alphabet[b];
                encoded[j + 2] = Alphabet[64];
                encoded[j + 3] = Alphabet[64];
                j += 4;
            }

            return encoded;
        }

        public static char[] EncodeWithOptimizedBitOperations(byte[] source)
        {
            var rest = source.Length % 3;
            var completeTriplets = source.Length - rest;
            var encoded = new char[completeTriplets / 3 * 4 + (rest != 0 ? 4 : 0)];
            var j = 0;

            var i = 0;
            for (; i < completeTriplets; i += 3)
            {
                // Rather than
                // int pattern = source[i] << 16 | source[i + 1] << 8 | source[i + 2];
                // int a = (pattern & (63 << 18)) >> 18;
                // int b = (pattern & (63 << 12)) >> 12;
                // int c = (pattern & (63 << 6)) >> 6;
                // int d = pattern & 63;
                // we can do the computation inline without the intermediate results.
                encoded[j] = Alphabet[(source[i] & 0b1111_1100) >> 2];
                encoded[j + 1] = Alphabet[((source[i] & 0b0000_0011) << 4) | (source[i + 1] & 0b1111_0000) >> 4];
                encoded[j + 2] = Alphabet[((source[i + 1] & 0b0000_1111) << 2) | (source[i + 2] & 0b1100_0000) >> 6];
                encoded[j + 3] = Alphabet[source[i + 2] & 0b0011_1111];
                j += 4;
            }

            // Third byte not present in last batch
            if (rest == 2)
            {
                encoded[j] = Alphabet[(source[i] & 0b1111_1100) >> 2];
                encoded[j + 1] = Alphabet[((source[i] & 0b0000_0011) << 4) | (source[i + 1] & 0b1111_0000) >> 4];
                encoded[j + 2] = Alphabet[(source[i + 1] & 0b0000_1111) << 2];
                encoded[j + 3] = Alphabet[64];
                j += 4;
            }
            // Second and third bytes not present in last batch
            else if (rest == 1)
            {
                encoded[j] = Alphabet[(source[i] & 0b1111_1100) >> 2];
                encoded[j + 1] = Alphabet[(source[i] & 0b0000_0011) << 4];
                encoded[j + 2] = Alphabet[64];
                encoded[j + 3] = Alphabet[64];
                j += 4;
            }

            return encoded;
        }

        public static unsafe char[] EncodeWithPointersToAvoidBoundsChecking(byte[] source)
        {          
            var rest = source.Length % 3;
            var completeTriplets = source.Length - rest;
            var encoded = new char[completeTriplets / 3 * 4 + (rest != 0 ? 4 : 0)];
            var j = 0;

            fixed (char* pAlphabet = &Alphabet[0])
            fixed (byte* pSource = &source[0])
            fixed (char* pEncoded = &encoded[0])
            {
                var i = 0;
                for (; i < completeTriplets; i += 3)
                {
                    pEncoded[j] = pAlphabet[(pSource[i] & 0b1111_1100) >> 2];
                    pEncoded[j + 1] = pAlphabet[((pSource[i] & 0b0000_0011) << 4) | (pSource[i + 1] & 0b1111_0000) >> 4];
                    pEncoded[j + 2] = pAlphabet[((pSource[i + 1] & 0b0000_1111) << 2) | (pSource[i + 2] & 0b1100_0000) >> 6];
                    pEncoded[j + 3] = pAlphabet[pSource[i + 2] & 0b0011_1111];
                    j += 4;
                }

                // Third byte not present in last batch
                if (rest == 2)
                {
                    pEncoded[j] = pAlphabet[(pSource[i] & 0b1111_1100) >> 2];
                    pEncoded[j + 1] = pAlphabet[((pSource[i] & 0b0000_0011) << 4) | (pSource[i + 1] & 0b1111_0000) >> 4];
                    pEncoded[j + 2] = pAlphabet[(pSource[i + 1] & 0b0000_1111) << 2];
                    pEncoded[j + 3] = pAlphabet[64];
                    j += 41;
                }
                // Second and third bytes not present in last batch
                else if (rest == 1)
                {
                    pEncoded[j] = Alphabet[(pSource[i] & 0b1111_1100) >> 2];
                    pEncoded[j + 1] = pAlphabet[(pSource[i] & 0b0000_0011) << 4];
                    pEncoded[j + 2] = pAlphabet[64];
                    pEncoded[j + 3] = pAlphabet[64];
                    j += 4;
                }
            }
            return encoded;
        }

        public static byte[] Decode(string encoded)
        {
            // Assume padding is used, meaning that we're dealing with an encoded string whose length is a multiple of 4.
            var decoded = new char[(encoded.Length / 4) * 3];

            // TODO

            return null;
        }
    }
    
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(printAsm: true, printSource: true, printIL: true)]
    [RPlotExporter]
    public class Benchmark
    {
        [Params(10000, 100000, 1000000)]
        public int InputSize { get; set; }

        byte[] data;

        [GlobalSetup]
        public void GlobalSetup()
        {
            data = new byte[InputSize];
            new Random(42).NextBytes(data);
        }

        [Benchmark]
        public string EncodeNaive() => Base64.EncodeWithNaive(data);

        [Benchmark]
        public char[] EncodePreAllocatedOutputBuffer() => Base64.EncodeWithPreAllocatedOutputBuffer(data);

        [Benchmark]
        public char[] EncodeOptimizedBitOperations() => Base64.EncodeWithOptimizedBitOperations(data);

        [Benchmark]
        public char[] EncodePointersToAvoidBoundsChecking() => Base64.EncodeWithPointersToAvoidBoundsChecking(data);

        [Benchmark(Baseline=true)]
        public string BuildInToBase64String() => Convert.ToBase64String(data);
    }

    class Program
    {
        static void TestEncode()
        {
            // Converts three bytes (24 bits) into four (24 bits) encoded characters
            var source = Encoding.ASCII.GetBytes("Man");
            var encoded = Base64.EncodeWithNaive(source);
            var encoded1 = new string(Base64.EncodeWithPreAllocatedOutputBuffer(source));
            var encoded2 = new string(Base64.EncodeWithOptimizedBitOperations(source));
            var encoded3 = new string(Base64.EncodeWithPointersToAvoidBoundsChecking(source));
            Debug.Assert(encoded == "TWFu");
            Debug.Assert(encoded == encoded1);
            Debug.Assert(encoded == encoded2);
            Debug.Assert(encoded == encoded3);
            Debug.Assert(encoded == Convert.ToBase64String(source));

            // Convert two bytes (16 bits) into four (24 bits) encoded characters
            source = Encoding.ASCII.GetBytes("Ma");
            encoded = Base64.EncodeWithNaive(source);
            encoded1 = new string(Base64.EncodeWithPreAllocatedOutputBuffer(source));
            encoded2 = new string(Base64.EncodeWithOptimizedBitOperations(source));
            encoded3 = new string(Base64.EncodeWithPointersToAvoidBoundsChecking(source));
            Debug.Assert(encoded == "TWE=");
            Debug.Assert(encoded == encoded1);
            Debug.Assert(encoded == encoded2);
            Debug.Assert(encoded == encoded3);
            Debug.Assert(encoded == Convert.ToBase64String(source));

            // Convert one bytes (8 bits) into four (24 bits) encoded characters
            source = Encoding.ASCII.GetBytes("M");
            encoded = Base64.EncodeWithNaive(source);
            encoded1 = new string(Base64.EncodeWithPreAllocatedOutputBuffer(source));
            encoded2 = new string(Base64.EncodeWithOptimizedBitOperations(source));
            encoded3 = new string(Base64.EncodeWithPointersToAvoidBoundsChecking(source));
            Debug.Assert(encoded == "TQ==");
            Debug.Assert(encoded == encoded1);
            Debug.Assert(encoded == encoded2);
            Debug.Assert(encoded == encoded3);
            Debug.Assert(encoded == Convert.ToBase64String(source));

            // Longer sample from Wikipedia
            source = Encoding.ASCII.GetBytes("Man is distinguished, not only by his reason, but by this singular passion from other animals, which is a lust of the mind, that by a perseverance of delight in the continued and indefatigable generation of knowledge, exceeds the short vehemence of any carnal pleasure.");
            encoded = Base64.EncodeWithNaive(source);
            encoded1 = new string(Base64.EncodeWithPreAllocatedOutputBuffer(source));
            encoded2 = new string(Base64.EncodeWithOptimizedBitOperations(source));
            encoded3 = new string(Base64.EncodeWithPointersToAvoidBoundsChecking(source));
            Debug.Assert(encoded == "TWFuIGlzIGRpc3Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWFzb24sIGJ1dCBieSB0aGlzIHNpbmd1bGFyIHBhc3Npb24gZnJvbSBvdGhlciBhbmltYWxzLCB3aGljaCBpcyBhIGx1c3Qgb2YgdGhlIG1pbmQsIHRoYXQgYnkgYSBwZXJzZXZlcmFuY2Ugb2YgZGVsaWdodCBpbiB0aGUgY29udGludWVkIGFuZCBpbmRlZmF0aWdhYmxlIGdlbmVyYXRpb24gb2Yga25vd2xlZGdlLCBleGNlZWRzIHRoZSBzaG9ydCB2ZWhlbWVuY2Ugb2YgYW55IGNhcm5hbCBwbGVhc3VyZS4=");
            Debug.Assert(encoded == encoded1);
            Debug.Assert(encoded == encoded2);
            Debug.Assert(encoded == encoded3);
            Debug.Assert(encoded == Convert.ToBase64String(source));
        }

        static void TestDecode()
        {
            // "TWFu"
            var decoded = Base64.Decode("TWFu");
            var source = Encoding.ASCII.GetBytes("Man");
            Debug.Assert(decoded.Length == source.Length);
            for (var i = 0; i < decoded.Length; i++)
            {
                Debug.Assert(decoded[i] == source[i]);
            }
        }

        static void Main(string[] args)
        {
            TestEncode();
            //var summary = BenchmarkRunner.Run<Benchmark>();
        }
    }
}
