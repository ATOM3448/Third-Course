import socket

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
        print(f"\n\nyear - {self.year}\nmonth - {self.month}\nday - {self.day}\nnumber - {self.number}\n\n")

with socket.socket() as sock:
    sock.connect(('localhost', 9090)) 

    byte = list()

    for j in range(5):
        sock.send("Tick?".encode('utf-8')) 

        for i in range(4):
            byte.append(sock.recv(4))

        MyTime.decode(byte).toOut()

        byte.clear()

    sock.send("Stop".encode('utf-8'))