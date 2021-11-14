# C generic dynamic array

To see what the `define_array` macro expands to:

    gcc -E main.c > expanded.c

In `expanded.c` look for `typedef struct person_array`.