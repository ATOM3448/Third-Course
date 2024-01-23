import tkinter as tk
import Sender

from tkinter import *
from tkinter import messagebox

def mySend():
    # Получаем данные
    target = target_tf.get()
    subject = subject_tf.get()
    content = content_tf.get()

    Sender.finalySend(target, subject, content)

# Создаем и настраиваем окно
window = Tk()
window.title("Отправка почты")
window.geometry("400x300")

frame = Frame(
    window,
    padx = 10,
    pady = 10
)
frame.pack(expand = True)

# Получатель приглашение/ввод
target_lb = Label(
    frame,
    text = "Введите получателя письма"
)
target_lb.grid(row = 3, column = 1)

target_tf = Entry(
    frame
)
target_tf.grid(row = 3, column = 2)

# Тема приглашение/ввод
subject_lb = Label(
    frame,
    text = "Введите тему письма"
)
subject_lb.grid(row = 4, column = 1)

subject_tf = Entry(
    frame
)
subject_tf.grid(row = 4, column = 2)

# Контент приглашение/ввод
content_lb = Label(
    frame,
    text = "Введите содержимое письма"
)
content_lb.grid(row = 5, column = 1)

content_tf = Entry(
    frame
)
content_tf.grid(row = 5, column = 2)

# Кнопка отправки
send_btn = Button(
    frame,
    text = "Отправить",
    command = mySend
)
send_btn.grid(row = 6, column = 2)

# Не позволяем окну закрыться
window.mainloop()
    