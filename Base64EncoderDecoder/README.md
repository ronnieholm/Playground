# Base64EncoderDecoder

- Don't convert to a single big number because that doesn't allow gradual transformation and requires bignum support.
- Instead of using a more common approach (as in three some assembly video) to convert from one base system to another,
  base64 does the trick with 6 bit intervals. Presumable because converting a large chunks of bytes into
  one larger chunck is very costly compared to current, and required lots of intermediate space in the 80s.
- Read RFC on base 64.

## References

// https://derekwill.com/2015/03/05/bit-processing-in-c/
// https://github.com/dotnet/corefx/blob/3b24c535852d19274362ad3dbc75e932b7d41766/src/Common/src/CoreLib/System/Convert.cs#L2529
// https://github.com/dotnet/corefx/blob/3b24c535852d19274362ad3dbc75e932b7d41766/src/Common/src/CoreLib/System/Convert.Base64.cs
