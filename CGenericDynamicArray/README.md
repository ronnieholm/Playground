# C generic dynamic array

To see what the `define_array` macro expands to:

    gcc -E main.c > expanded.c

In `expanded.c` look for `typedef struct person_array`.

See also the data structures in the Linux kernel, such as the circular doubly
linked list at
https://elixir.bootlin.com/linux/v5.15.2/source/include/linux/list.h.