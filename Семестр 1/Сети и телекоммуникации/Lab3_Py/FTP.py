import socket
import re
import time
import ftplib

# Сервер и данные
serv = "ftp.slackware.com"
user = "anonymous"
password = "anonymous"

# Команды
CMD_USER = f"USER {user}\r\n"       # Пользователь
CMD_PASS = f"PASS {password}\r\n"   # Пароль
CMD_PASV = "PASV\r\n"               # Пассивный режим
CMD_LIST = "LIST\r\n"               # Лист
CMD_NLST = "NLST\r\n"               # Лист кратко
CMD_QUIT = "QUIT\r\n"               # Выход
CMD_NOOP = b"NOOP\r\n"              # Пустая команда
CMD_ABOR = "ABOR\r\n"               # Прервать передачу файла
CMD_PWD = "PWD\r\n"                 # Текущуй каталог
CMD_CDUP = "CDUP\r\n"               # Подняться на каталог выше
CMD_CWD = "CWD"                     # Открыть каталог
CMD_RETR = "RETR"                   # Скачать файл
CMD_MDTM = "MDTM"                   # Время модификации файла

# Отправка команд
def send_cmd(s:socket, cmd:str):
    s.send(cmd.encode())
    print(cmd[:-2])

    # Отдельный сценарий для QUIT
    if cmd == "QUIT\r\n":
        buf = s.recv(1024).decode('utf-8')
        return buf

    # Отдельный сценарий для USER
    if cmd[0:4] == "USER":
        buf = s.recv(1024).decode('utf-8')
        return buf

    # Отправляем пустую команду
    # (Ответ на неё будет как обозначение конца основного ответа сервера)
    s.send(CMD_NOOP)
    
    # Считываем ответ сервера
    buf = ""
    while buf[-20::].lower().count('noop') == 0:
        buf += s.recv(1024).decode('utf-8')

    return buf

# Просмотр файлов и папок в текущей директории
def send_list(s:socket, mode:int = 0):
    s1 = send_pasv(s)

    if mode == 0:
        send_cmd(s, CMD_LIST)
    if mode == 1:
        send_cmd(s, CMD_NLST)

    # Считываем весь список
    buf = receive_data(s1)

    lst = buf.split('\r\n')[:-1:]

    if mode == 1:
        return lst

    # Приводим к удобному виду
    for i in range(len(lst)):
        matches = re.split(r"\s+", lst[i])

        lst[i] = []
        lst[i].append(matches[0])                               # Разрешения
        lst[i].append(matches[1])                               # Кол-во ссылок
        lst[i].append(matches[2])                               # UID владельца
        lst[i].append(matches[3])                               # GID
        lst[i].append(matches[4])                               # Размер
        lst[i].append(matches[5] + matches[6] + matches[7])     # Дата и время модификации
        if matches.count('->') != 0:
            lst[i].append(matches[-3])                          # Имя для ссылки
        else:    
            lst[i].append(matches[-1])                          # Имя
            
    return lst

# Получение данных
def receive_data(s:socket):

    # Чтение данных пока не пойдут пустые строки
    buf = ""
    while True:
        time.sleep(0.1)
        k = s.recv(1024).decode('utf-8')
        if k == '':
            break
        buf += k

    print("Data received\n")

    s.close()
    
    return buf

# Переход в пассивный режим
def send_pasv(s:socket):

    # Команда на получение адреса подключение
    ret = send_cmd(s, CMD_PASV)

    # Поиск адреса подключения
    match = re.search(r"(\d+,\d+,\d+,\d+,\d+,\d+)", ret)

    # Приведение адреса к удобному виду
    match = re.split(r",", match[0])

    ip = ".".join(match[0:4])
    port = int(match[4])*256+int(match[5])

    print(f"ip {ip} port {port}")

    # Подключение
    s1 = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    try:
        s1.connect((ip, port))
        print("PASV Connection OK")
    except:
        print("PASV Error connection")
        return None
    
    return s1

# Скачать файл
def download(s:socket, name:str):
    # Подключаемся для получения данных
    s1 = send_pasv(s)

    # Запрашиваем скачивание с обработкой ошибки
    if send_cmd(s, f"{CMD_RETR} {name}\r\n")[0] == '5':
        print("Download some error")
        s1.close()
        return None
    
    # Получаем данные и записываем в файл
    data = receive_data(s1)
    if data != None:
            with open(f'download/{obj}', 'w') as f:
                f.write(data) 

# Просмотр времени модификации
def mod_time(s:socket, name:str):
    answ = send_cmd(s, f'{CMD_MDTM} {obj}\r\n')
    
    if answ[0] == '5':
        return "Couldn't get modification time"
    
    # Выделяем нужное и приводим ответ к читаемому виду
    answ = answ[4:18]

    return f"{answ[6:8:]}.{answ[4:6:]} {answ[:4:]} - {answ[8:10:]}:{answ[10:12:]}:{answ[12::]}"

# Смена каталога
def dir_change(s:socket, name:str):
    answ = send_cmd(s, f"{CMD_CWD} {name}\r\n")
    if answ[0] == '5':
        print("Directory change error")
    else:
        print("Directory changed")

# --- ОСНОВНОЙ БЛОК ---
with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    # Попытка подключиться
    try:
        s.connect((serv, 21))
        print("Connection OK")
    except:
        print("Error connection")
        quit()

    buf = s.recv(1024)
    print(buf.decode('utf-8'))

    # Аунтификация
    send_cmd(s, CMD_USER)
    send_cmd(s, CMD_PASS)

    # --- Работа с файлами ---

    while True:
        # Получаем список объектов (выделяем отдельно имена для дальнейшего удобства)
        lst = send_list(s, 0)
        lst_namesOnly = []
        for i in lst:
            lst_namesOnly.append(i[-1])

        # Делим объекты на категории
        dirs = []
        links = []
        files = []
        for i in lst:
            if i[0][0] == 'd':
                dirs.append(i)
                continue
            if i[0][0] == 'l':
                links.append(i)
                continue
            files.append(i)

        # Выводим имена по категориям
        print("\nFiles:")
        for i in files:
            print('\t' + i[-1])
        print("\nDirs:")
        for i in dirs:
            print('\t' + i[-1])
        print("\nLinks:")
        for i in links:
            print('\t' + i[-1])

        # Если не в корневой директории даем возможность подняться
        root = True
        if send_cmd(s, CMD_PWD).split('\"')[1] != '/':
            root = False
            print("\nTo climb, enter \"/\\\"")

        # Запрашиваем название объекта для дальнейшей работы
        if root:
            print()
        obj = input("Select an object:\n\t")

        # Пробуем найти объект (также проверяем, пробует ли пользователь подняться)
        try:
            indOfObj = lst_namesOnly.index(obj)
        except:
            if not(root) and (obj == '/\\'):
                send_cmd(s, CMD_CDUP)
                print("Climbed")
                continue
            print("Object not found")
            break

        # Запрашиваем дальнейшее действие
        action = input("\nSelect an action:\n\t1. Download\n\t2. Time of modification\n\t3. Open (for folders)\n\t4. Another object\n\tOther. Exit\n\t\t")

        # Скачивание
        if action == '1':
            download(s, obj)

        # Время модификации
        elif action == '2':
            print(mod_time(s, obj))

        # Смена директории
        elif action == '3':
            if lst[indOfObj][0][0] == 'd':
                dir_change(s, obj)
            else:
                print("Directory change error")

        # Другой объект
        elif action == '4':
            continue

        # Ничего из списка - выход
        else:
            break

        # После действий запрашиваем желание закончить
        action = input("\n\nВведите \"END\" чтобы закрыть программу\nЧто угодно другое чтобы продолжить\n\t")

        if action.lower() == 'end':
            break

    # ------------------------

    # Закрываем соединение
    send_cmd(s, CMD_QUIT)

# ---------------------