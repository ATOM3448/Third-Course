from socket import *

from http_parser.http import HttpStream
from http_parser.reader import SocketReader
from html.parser import HTMLParser

http_server = "ftp.gnu.org"
page = ""

cSock = socket(AF_INET, SOCK_STREAM) 
cSock.connect((http_server, 80))

cSock.send(f"GET /{page} HTTP/1.0\r\n".encode())
print(f"GET /{page} HTTP/1.0\r\n")
cSock.send(f"HOST: {http_server}\r\n".encode())
print(f"HOST: {http_server}\r\n")
cSock.send(b"\r\n")

r = SocketReader(cSock)
p = HttpStream(r)

print("\nЗаголовки ответа")
headers = list(p.headers().keys())
for i in headers:
    print(f"{i}")

print()

body = p.body_file().read()

print()

class tHTMLParser(HTMLParser):
    def handle_starttag(self, tag, attrs):
        if tag == 'img':
            for attr in attrs:
                if attr[0] == 'src':
                    print(" Ссылка на изображение:", attr[1])

parser = tHTMLParser()
parser.feed(body.decode('utf8'))

cSock.close()