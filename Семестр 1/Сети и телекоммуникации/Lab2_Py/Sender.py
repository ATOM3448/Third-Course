import socket
import ssl
import base64
import UserSend as UserSend
from socket import *

# Отправка команды
def sendcmd(sock, cmd, content = ''):
    sock.send((cmd + ' ' + content + '\r\n').encode('utf-8'))

    recv = sock.recv(1024)
    code = recv[:3]

    if ((code != b'220') and (code != b'250') and (cmd != 'QUIT')):
        print('Somthing wrong.')
        return True

    return False

# Аунтификация
def auth(sock, user, password):
    sock.send("AUTH PLAIN\r\n".encode('utf-8'))

    recv = sock.recv(1024)
    if (recv[:3] != b'334'):
        print('334 reply not received from server after \"AUTH PLAIN\".')
        return True

    login = '\x00' + user + '\x00' + password

    sock.send(base64.b64encode(login.encode('ascii')) + b'\r\n')

    recv = sock.recv(1024)
    if (recv[:3] == b'535'):
        print('Authentication failed.')
        return True

    return False

def finalySend(target, subject, content):

    # Основной блок
    mailserver = 'smtp.gmail.com'

    # Создание сокета с SSL соединением
    cSock = socket()
    cSock.connect((mailserver, 465))
    cSock = ssl.wrap_socket(cSock)

    # Проверяем успешность соединения
    recv = cSock.recv(1024)

    if (recv[:3] != b'220'):
        print('220 reply not received from server.')

    # Отправляем HELO
    if (sendcmd(cSock, 'EHLO', mailserver)):
        quit()

    # Аунтификация
    if (auth(cSock, f'{UserSend.User}', f'{UserSend.Password}')):
        quit()

    cSock.send(f'MAIL FROM: <{UserSend.User}>\r\n'.encode('utf-8'))

    recv = cSock.recv(1024)
    if (recv[:3] != b'250'):
        print('250 reply not received from server after \"MAIL FROM\".')

    cSock.send(f'RCPT TO: <{target}>\r\n'.encode('utf-8'))

    recv = cSock.recv(1024)
    if (recv[:3] == b'550'):
        print('User not found.')
    if (recv[:3] != b'250'):
        print('250 reply not received from server after \"RCPT\".')

    cSock.send('DATA\r\n'.encode('utf-8'))

    recv = cSock.recv(1024)
    if (recv[:3] != b'354'):
        print('334 reply not received from server after \"DATA\".')

    # Содержание письма
    
    cSock.send("Content-Type: multipart/mixed; boundary=\"===============1==\"\r\n".encode('ascii'))
    cSock.send("MIME-Version: 1.0\r\n".encode('ascii'))
    cSock.send(f"To: {target}\r\n".encode('ascii'))
    cSock.send(f"From: {UserSend.User}\r\n".encode('ascii'))
    cSock.send(f"Subject: =?utf-8?b?".encode('ascii'))
    cSock.send(f"{subject}".encode('utf-8'))
    cSock.send("?=\r\n\n".encode('ascii'))

    cSock.send(f"--===============1==\r\nContent-Type: text/plain; charset=\"utf-8\"\r\nMIME-Version: 1.0\r\nContent-Transfer-Encoding: base64\r\n\n".encode('ascii'))
    cSock.send(f"{content}\r\n\n".encode('utf-8'))

    cSock.send(f"--===============1==\r\nContent-Type: application/octet-stream\r\nMIME-Version: 1.0\r\nContent-Transfer-Encoding: base64\r\nContent-Disposition: attachment; filename=\"text.txt\"\r\n\n".encode('ascii'))

    with open('text.txt', 'r') as file:
        cSock.send(base64.b64encode(file.read().encode('ascii')) + b'\r\n')

    cSock.send('\r\n.\r\n'.encode('ascii'))

    # Конец письма

    recv = cSock.recv(1024)
    if (recv[:3] != b'250'):
        print('250 reply not received from server after content.')

    # Выход

    sendcmd(cSock, 'QUIT')

    cSock.close()