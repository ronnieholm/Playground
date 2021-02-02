# IEEE 754

## Resources

- [Why 0.1 + 0.2 === 0.30000000000000004: Implementing IEEE 754 in JS](https://www.youtube.com/watch?v=wPBjd-vb9eI)
   - Video contains mistakes fixed below
   - Encoding floating point numbers is a kind of compression algorithm. We're
     compressing every real number from -infinity to +infinity into some number
     of bits
   - The range of numbers that a float can represented is traded of with how
     precisely these number can be represented
   - Most languages store floating point numbers using 64 bit double precision
     or 32 bit single precision
   - Our C implementation uses 16 bit representation
   - Floating point numbers are split into three parts
     - Sign (1 bit)
     - Exponent (5 bits)
       - Gives a rough area on a number line
       - With 5 bits we can represent numbers from -32 to +32
       - Think of the exponent not as a single number but a range
         - 2^n exponent range ->   Numerical range
           - [0, 1] -> [1, 2]
           - [1, 2] -> [2, 4]
           - [2, 3] -> [4, 8]
           - [3, 4] -> [8, 16]
           - [4, 5] -> [16, 32]
         - A number like 12.52571 would fall into the [3, 4] range, meaning the
           exponent would be 3
     - Mantissa (10 bits)
       - Zoomed in area on number line
       - With 12.52571, work out as a percentage where the number is in the
         range [8, 16]
         - 12.52571 - lower / upper - lower
         - 12.52571 - 8 / 16 - 8 = 0.56571375        
         - Verification: 8 * 0.56571375 = 12.52571
       - Encode 0.56571375 as a binary number
         - With a 10 bit mantissa, we can represent 1024 numbers
         - Round(0.56571375 * 1024 = 579.29088) = 579 = 1001000011
         - Rounding is where the compression is performed as we've given up on
           some precision
         - Converting in reverse we get
           mantissa = 579 / 1024 = 0.5654296875
         - 8 * 1.5654296875 = 12.5234375
         - 12.52571 (original) - 12.5234375 (encoded) ~ 0.002272 rounding error
     - With N being the number we want to represent
       - N = (-1)^sign * 1.mantissa * 2^exponent

- [Floating Point Numbers - Computerphile](https://www.youtube.com/watch?v=PZRI1IfStY0)
  - Floating point numbers are essentially scientific notation in base 2
  - Base 10 example
    - 300000000 * 0.00000015
    - 3 * 10^8 * 1.5^10-7
    - (3 + 1.5) * (8 + -7)
    - 4.5 * 10^1
    - 45
  - Benefits of floating point numbers are speed (perfected over many years,
    CPUs have become fast at it) and efficiency (handles large numbers, size of
    universe and handles small numbers, size of atom) without needing large
    amounts of storage space