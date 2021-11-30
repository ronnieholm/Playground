#include <stdio.h>
#include <errno.h>
#include <string.h>
#include <stdlib.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <pthread.h>

void *connection_handler(void *socket_desc) {
	int sock = *(int*)socket_desc;
	char *message;
	
	message = "Greetings! I am your connection handler\n";
	send(sock, message, strlen(message), 0);
	message = "Its my duty to communicate with you\n";
	send(sock, message, strlen(message), 0);

    int read_size;
	char client_message[2048];
    while((read_size = recv(sock, client_message, 2048, 0)) > 0) {
		write(sock, client_message , strlen(client_message));
	}
	
    sleep(100);
	if(read_size == 0) {
		printf("Client disconnected\n");
		fflush(stdout);
	}
	else if (read_size == -1) {
		perror("Receive failed");
	}

	free(socket_desc);	
	return 0;
}

int main(int argc, char **argv)
{
    int server_sock = socket(AF_INET, SOCK_STREAM, 0);
    if (server_sock == -1) {
        perror("Error creating socket");
        exit(-1);
    }

    struct sockaddr_in server, client;
    server.sin_family = AF_INET;
    server.sin_addr.s_addr = INADDR_ANY;
    server.sin_port = htons(8000);

    if (bind(server_sock, (struct sockaddr *)&server, sizeof(server)) < 0) {
        perror("Error binding socket");
        exit(-1);
    }

    if (listen(server_sock, 3) == -1) {
        perror("Listen failed");
        exit(-1);
    }

    printf("Waiting for incoming connections\n");
    int c = sizeof(struct sockaddr_in);
    int client_sock;
    while ((client_sock = accept(server_sock, (struct sockaddr *)&client, (socklen_t *)&c))) {
        printf("Connection accepted\n");
        char *message = "Hello Client. I will assign a handler for you\n";
        send(client_sock, message, strlen(message), 0);

        pthread_t sniffer_thread;
        int *clone_sock = malloc(1);
        *clone_sock = client_sock;
        if (pthread_create(&sniffer_thread, NULL, connection_handler, (void*) clone_sock) != 0) {
			perror("Could not create thread");
			exit(-1);
		}

        //pthread_join(sniffer_thread, NULL);
		printf("Handler assigned\n");
        fflush(stdout);
    }

    if (client_sock < 0) {
		perror("Accept failed");
		exit(-1);
	}

    fflush(stdout);
    return 0;
}