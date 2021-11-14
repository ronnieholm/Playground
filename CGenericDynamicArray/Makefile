flags += -std=c11
flags += -Wall -Wno-unused-function -Wno-unused-variable

.PHONY: all clean
all: 
	@$(CC) $(flags) main.c -o main
	@./main	

valgrind:
	valgrind -- ./main

clean:
	@rm main