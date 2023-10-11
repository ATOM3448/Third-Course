import socket
import ssl
from socket import *
import base64
import UserRec

# Основной блок
mailserver = 'pop.gmail.com'

# Создание сокета с SSL соединением
cSock = socket()
cSock.connect((mailserver, 995))
cSock = ssl.wrap_socket(cSock)

#Проверка подключения
answer = cSock.recv(1024).decode('ascii')
if(answer[1:3] != 'OK'):
    print("Some err")

# Авторизация
cSock.send(f"USER {UserRec.User}\r\n".encode('ascii'))
answer = cSock.recv(1024).decode('ascii')
if(answer[1:3] != 'OK'):
    print("Some err")

cSock.send(f"PASS {UserRec.Password}\r\n".encode('ascii'))
answer = cSock.recv(1024).decode('ascii')
if(answer[1:3] != 'OK'):
    print("Some err")

# Получаем письмо
cSock.send("LIST 1\r\n".encode('ascii'))
answer = cSock.recv(1024).decode('ascii')
size = answer[6:-2:1]

answer = ''.encode('ascii')
cSock.send("RETR 1\r\n".encode('ascii'))
for i in range(int(size)+24):
    answer += cSock.recv(1)

norm = []

for i in answer.decode('utf-8').split('\r\n'):
    norm.append(i)

sub = 0
for i in norm:
    if "Subject" in i:
        sub = i[19:-2:]

content = 0
for i in norm:
    if "Content-Type: text/plain;" in i:
        content = norm[norm.index(i)+4]

print(f"Заголовок: {sub}\nСодержание: {content}")
