#pragma once

#include <stdio.h>
#include <stdlib.h>

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
	sin.sin_addr.S_un.S_addr = inet_addr("127.0.0.1");

	if (connect(s, (LPSOCKADDR)&sin, sizeof(sin)))
	{
		printf("connect() failed.\n");

		getch();

		return 1;
	}

	// Начинаем работу
	char mode[] = "Tick?";

	MyDate date;

	for (int i = 0; i < 4; i++)
	{
		if (send(s, &mode, sizeof(mode), 0) == -1)
		{
			printf("Send() error");

			closesocket(s);

			printf("\n\n Programm \"Client\" is done");

			getch();

			return 0;
		}

		recv(s, &date.year, sizeof(date.year), 0);
		recv(s, &date.month, sizeof(date.month), 0);
		recv(s, &date.day, sizeof(date.day), 0);
		recv(s, &date.number, sizeof(date.number), 0);

		printf("\n\nyear - %hi\nmonth - %hi\ndate - %hi\nnumber of request - %d", date.year, date.month, date.day, date.number);
	}

	mode[0] = '0';
	send(s, &mode, sizeof(mode), 0);

	closesocket(s);

	printf("\n\n Programm \"Client\" is done");

	getch();

	return 0;
}