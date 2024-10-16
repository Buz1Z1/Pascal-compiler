using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Компилятор
{
    class Program
    {
        static void Main()
        {
            FileStream zacode = new FileStream ("2.txt",FileMode.Create);//Создаем файл, куда будем записывать кодировованный код
            zacode.Close();
            FileStream erro = new FileStream("errors.txt", FileMode.Create);//Создаем файл, куда запишем информацию про ошибки
            erro.Close();

            InputOutput.Begin();

            InputOutput.Scan();//запускаем модуль ввода-вывода

            //SyntaxisAnalyzer.Trying();

            Console.WriteLine("\n"+"Введите что-нибудь: ");//это чтобы консоль не закрывалась сразу
            Console.ReadLine();
        }
    }
}
