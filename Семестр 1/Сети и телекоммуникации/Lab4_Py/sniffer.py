import struct
import sys, os
import socket
import binascii

# Создаем сырой сокет для которого bind НЕ НУЖЕН
rawSocket = socket.socket(socket.PF_PACKET, socket.SOCK_RAW, socket.htons(0x0800))

# Считаем количество перехваченых пакетов
count = 0

# Перехватываем пакеты до посинения (или до  CTR + Z в терминале)
while True:
    # Увеличиваем счётчик
    count += 1

    # Получаем пакет
    recivedPacket = rawSocket.recv(2048)

    # Получаем заголовки пакета
    ipHeader = recivedPacket[14:34]

    # Распаковываем заголовки пакета
    ipHrd = struct.unpack("!12s4s4s", ipHeader)

    # Получаем айпишники откуда и куда пакет
    destinationIP = socket.inet_ntoa(ipHrd[2])
    sourceIP = socket.inet_ntoa(ipHrd[1])

    # Получаем номер протокола (в конце записаны все обозначения протоколов)
    protocolIP = struct.unpack('!B', ipHeader[9:10])[0]

    # Если TCP или ICMP - выводим о них инфу
    if (protocolIP == 6) or (protocolIP == 1):
        print(f"------{count}-----")
        print("\nDestination IP: " + destinationIP)
        print("Source IP: " + sourceIP)
        if protocolIP == 6:                         # TCP
            # Получаем и распаковываем заголовки для TCP
            tcpHeader = recivedPacket[34:54]
            tcpHdr = struct.unpack("!HH16s", tcpHeader)

            # Получаем порты отправителя и получателя
            sourcePort = tcpHdr[0]
            destinationPort = tcpHdr[1]

            # Выводим
            print("Protocol: TCP")
            print("Source port: " + str(sourcePort))
            print("Destination port: " + str(destinationPort))
        else:                                       # ICMP
            # Получаем и распаковываем заголовки для ICMP
            icmpHeader = recivedPacket[34:38]
            icmpHdr = struct.unpack("!BBH", icmpHeader)

            # Получаем тип и код сообщения ICMP
            icmpType = icmpHdr[0]
            icmpCode = icmpHdr[1]

            # Выводим
            print("Protocol: ICMP")
            print("ICMP type: " + str(icmpType))
            print("ICMP code: " + str(icmpCode))
        print()

# 0 - HOPOPT
# 1 - ICMP
# 2 - IGMP
# 6 - TCP
# 17 - UPD
# 41 - IPv6
# 43 - IPv6-Route
# 44 - IPv6-Frag
# 47 - GRE
# 50 - ESP
# 51 - AH
# 58 - IPv6-ICMP