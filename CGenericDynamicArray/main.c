#include <stdio.h>
#include "generic_dynamic_array.h"
#include "utils.h"

typedef struct {
    char *name;
    u8 age;
} person;

define_array(people, person_array, person*);

int main(int argc, char **argv)
{
    person p1 = {
        .name = "John Doe",
        .age = 42
    };

    person p2 = {
        .name = "Jane Doe",
        .age = 35
    };

    person_array *ps = people_new(100);    
    people_append(ps, &p1);
    people_append(ps, &p2);

    for (int i = 0; i < ps->count; i++) {
        printf("%s, %d\n", ps->items[i]->name, ps->items[i]->age);
    }

    people_delete(ps);
    return 0;
}