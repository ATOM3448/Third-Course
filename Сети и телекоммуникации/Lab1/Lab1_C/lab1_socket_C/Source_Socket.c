#pragma once

#include <stdio.h>
#include <stdlib.h>
#include <time.h>

#include <winsock2.h>
#pragma comment(lib, "ws2_32.lib")

#pragma warning(disable:4996)

typedef struct MyDate
{
	__int32 year;
	__int32 month;
	__int32 day;
	__int32 number;
}MyDate;

int main()
{
	WSADATA WsaData;
	if (WSAStartup(0x0101, &WsaData) == SOCKET_ERROR)
	{
		printf("WSAStartup() failed: %ld\n", GetLastError());

		getch();

		return 1;
	}

	int s = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	SOCKADDR_IN sin;
	sin.sin_family = AF_INET;
	sin.sin_port = htons(9090);
	sin.sin_addr.s_addr = INADDR_ANY;

	if (bind(s, (LPSOCKADDR)&sin, sizeof(sin)))
	{
		printf("bind() failed.\n");

		getch();

		return 1;
	}

	if (listen(s, SOMAXCONN))
	{
		printf("listen() failed.\n");

		getch();

		return 1;
	}

	SOCKADDR_IN from;
	int fromlen = sizeof(from);
	int s1 = accept(s, (struct sockaddr*)&from, &fromlen);

	if (s1 < 0)
	{
		printf("accept() failed.\n");

		getch();

		return 1;
	}

	// Начинаем работу
	for(int i = 0; ;i++)
	{
		char mes[6];
		
		if (recv(s1, &mes, sizeof(mes), 0) == -1)
		{
			printf("Recv() error");
			closesocket(s1);
			closesocket(s);
			return 0;
		}

		if (!((mes[0] == 'T') && (mes[1] == 'i') && (mes[2] == 'c') && (mes[3] == 'k') && (mes[4] == '?')))
			break;

		time_t myTime = time(NULL);
		struct tm* now = localtime(&myTime);

		MyDate answer;
		answer.year = now->tm_year+1900;
		answer.month = now->tm_mon+1;
		answer.day = now->tm_mday;
		answer.number = i;

		if (send(s1, &answer.year, sizeof(answer.year), 0) == -1)
		{
			printf("Send() error");
		}
		if (send(s1, &answer.month, sizeof(answer.month), 0) == -1)
		{
			printf("Send() error");
		}
		if (send(s1, &answer.day, sizeof(answer.day), 0) == -1)
		{
			printf("Send() error");
		}
		if (send(s1, &answer.number, sizeof(answer.number), 0) == -1)
		{
			printf("Send() error");
		}
	}

	closesocket(s1);
	closesocket(s);
	
	printf("\n\nProgramm \"Socket\" is done");

	getch();

	return 0;
}