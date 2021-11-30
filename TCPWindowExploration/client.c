#include <stdio.h>
#include <errno.h>
#include <string.h>
#include <stdlib.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <netinet/tcp.h>

// Test using `python3 -m http.server`.
// https://linuxgazette.net/136/pfeiffer.html

int main(int argc, char **argv)
{
    int fd = socket(AF_INET, SOCK_STREAM, 0);
    if (fd == -1) {
        perror("Error creating socket");
        exit(-1);
    }

    struct sockaddr_in server;
    server.sin_addr.s_addr = inet_addr("127.0.0.1");
    server.sin_family = AF_INET;
	server.sin_port = htons(8000);

    if (connect(fd, (struct sockaddr *)&server, sizeof(server)) < 0) {
        perror("Error binding socket");
        exit(-1);
	}

    printf("Connected\n");

    struct tcp_info tcp_info;
    int tcp_info_length = sizeof(tcp_info);
    if (getsockopt(fd, SOL_TCP, TCP_INFO, (void *)&tcp_info, &tcp_info_length) == 0) {
        printf("snd_cwnd: %u\nsnd_ssthresh: %u\nrcv_ssthresh: %u\nrtt: %u\nrtt_var: %u\n",
            tcp_info.tcpi_snd_cwnd,
            tcp_info.tcpi_snd_ssthresh,
            tcp_info.tcpi_rcv_ssthresh,
            tcp_info.tcpi_rtt,
            tcp_info.tcpi_rttvar);
        // printf("%u %u %u %u %u %u %u %u %u %u %u %u\n",
        //         //time_to_seconds( &time_start, &time_now ),
        //         tcp_info.tcpi_last_data_sent,
        //         tcp_info.tcpi_last_data_recv,
        //         tcp_info.tcpi_snd_cwnd,
        //         tcp_info.tcpi_snd_ssthresh,
        //         tcp_info.tcpi_rcv_ssthresh,
        //         tcp_info.tcpi_rtt,
        //         tcp_info.tcpi_rttvar,
        //         tcp_info.tcpi_unacked,
        //         tcp_info.tcpi_sacked,
        //         tcp_info.tcpi_lost,
        //         tcp_info.tcpi_retrans,
        //         tcp_info.tcpi_fackets);
    }

    char server_reply[2048];
    char *message = "GET / HTTP/1.1\r\n\r\n";
	if(send(fd, message, strlen(message), 0) < 0) {
		perror("Send failed");
		exit(-1);
	}
	printf("Data Send\n");

    if(recv(fd, server_reply, 2000, 0) < 0) {
		perror("recv failed");
        exit(-1);
	}
	printf("Reply received\n");
	printf("%s", server_reply);
    close(fd);

    return 0;
}