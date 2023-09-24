import socket
import datetime

class MyTime():

    def __init__(self, _year = 0, _month = 0, _day = 0, _number = 0):
        self.year = _year
        self.month = _month
        self.day = _day
        self.number = _number

    def change(self, _year, _month, _day, _number):
        self.year = _year
        self.month = _month
        self.day = _day
        self.number = _number

    def encode(self):
        bytes = list()
        bytes.append(self.year.to_bytes(4, "little"))
        bytes.append(self.month.to_bytes(4, "little"))
        bytes.append(self.day.to_bytes(4, "little"))
        bytes.append(self.number.to_bytes(4, "little"))
        return bytes

    def decode(_bytes):
        if(_bytes.count != 0):
            _year = int.from_bytes(_bytes[0], "little")
            _month = int.from_bytes(_bytes[1], "little")
            _day = int.from_bytes(_bytes[2], "little")
            _number = int.from_bytes(_bytes[3], "little")
        else:
            _year = int.from_bytes(_bytes[0:4:1], "little")
            _month = int.from_bytes(_bytes[4:8:1], "little")
            _day = int.from_bytes(_bytes[8:12:1], "little")
            _number = int.from_bytes(_bytes[12:16:1], "little")

        _out = MyTime(_year, _month, _day, _number)

        return _out
    def toOut(self):
        print(f"year - {self.year}\nmonth - {self.month}\nday - {self.day}\nnumber - {self.number}")

with socket.socket() as sock: 
    sock.bind(('', 9090)) 
    sock.listen() 
    conn, addr = sock.accept() 
    print('connected:', addr) 
    answer = MyTime
    counter = 0
    while True: 
        data = conn.recv(1024) 
        if not data: 
            break 

        if(data[0:5:1].decode('utf-8') == 'Tick?'):
            date = datetime.datetime.now()
            answer.change(answer, date.year, date.month, date.day, counter)
            counter += 1

            for byte in answer.encode(answer):
                conn.send(byte)
        else:
            break
    conn.close()