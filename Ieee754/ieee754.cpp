#include <stdio.h>
#include <math.h>
#include <stdlib.h>

// Compile and run:
//
// $ g++ ieee754.cpp -lm -g -pedantic -o ieee754 && ./ieee754 12.52571
// Original: 12.525710
// Encoded: 3651
// Decoded: 12.523438
// Delta: 0.002273

// Bits: 0  1 2 3 4 5  6  7  8  9  10 11 12 13 14 15
// Type: S  E E E E E  M  M  M  M  M  M  M  M  M  M 

const int EXPONENT_BITS = 5;
const int MANTISSA_BITS = 10;
const int NON_SIGN_BITS = EXPONENT_BITS + MANTISSA_BITS;

int encode(double n)
{
    if (n > pow(2, EXPONENT_BITS)) {
        printf("Error: Cannot encode number\n");
        exit(1);
    }

    double a = fabs(n);
    int sign = signbit(n) == 0 ? 0 : 1;
    int exponent = floor(log(a) / log(2));
    int lower = pow(2, exponent);
    int upper = pow(2, exponent + 1);
    double percentage = (a - lower) / (upper - lower);
    int mantissa = pow(2, MANTISSA_BITS) * percentage;

    return sign << NON_SIGN_BITS |
           (exponent << MANTISSA_BITS) |
           mantissa;
}

double decode(int n)
{
    int sign = (n & 0b1000000000000000) >> NON_SIGN_BITS;
    int exponent = (n & 0b0111110000000000) >> MANTISSA_BITS;
    int mantissa = n & 0b0000001111111111;
    double percentage = mantissa / pow(2, MANTISSA_BITS);
    return pow(-1, sign) * (1 + percentage) * pow(2, exponent);
}

int main(int c, char** args)
{
    if (c != 2) {
        printf("Error: Expected single float argument\n");
        exit(1);
    }

    double n;
    sscanf(args[1], "%lf", &n);

    double original = n;
    printf("Original: %f\n", original);  

    int encoded = encode(original);
    printf("Encoded: %d\n", encoded);

    double decoded = decode(encoded);
    printf("Decoded: %f\n", decoded);

    double delta = fabs(original - decoded);
    printf("Delta: %f\n", delta);
}