using System;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
{
    struct TextPosition//позиция литеры в тексте
    {
        public uint lineNumber; // номер строки
        public byte charNumber; // номер позиции в строке

        public TextPosition(uint ln = 0, byte c = 0)
        {
            lineNumber = ln;
            charNumber = c;
        }
    }

    struct Err//информация об ошибке
    {
        public TextPosition errorPosition;//местоположение ошибки
        public byte errorCode;//код ошибки

        public Err(TextPosition errorPosition, byte errorCode)
        {
            this.errorPosition = errorPosition;
            this.errorCode = errorCode;
        }
    }

    class InputOutput
    {
        const byte ERRMAX = 9;//максимально число ошибок, о которых будет получать информацию пользователь
        public static char Ch { get; set; }
        public static TextPosition positionNow = new TextPosition();//позиция литеры в тексте
        public static string line;//строка
        //public static byte lastInLine = (byte)(line.Length-1);//последняя литера в строке
        public static List<Err> err;//список ошибок
        static StreamReader File { get;  set; }//файл
        static uint errCount = 0;//количество ошибок
        public static bool permisson = true, newline = false;

        static public void Begin()//Компиляция программы
        {
            permisson = true;
            File = new StreamReader("code_ps.txt");
            positionNow = new TextPosition();
            ReadNextLine();
            Ch = line[0];

            LexicalAnalyzer.NextSym();

            SyntaxisAnalyzer SA = new SyntaxisAnalyzer();
            SA.Программа();
        }
        static public void Scan()//Кодирование программы. Прогоняем через Лексический Анализатор
        {
            permisson = true;//в энде это поменяется, чтобы мы закончили программу и не читали там больше ничего
            File = new StreamReader("code_ps.txt");//файл, где лежит код
            positionNow = new TextPosition();
            ReadNextLine();//считываем первую строку
            Ch = line[0];
            StreamWriter SW = new StreamWriter("2.txt", true);//файл, куда записываем кодировку
            while(permisson)//пока не конец
            {
                LexicalAnalyzer.NextSym();//запускаем лексический анализатор
                //if(permisson)
                //{
                //    SW.Write(LexicalAnalyzer.symbol + " ");
                //}
                SW.Write(LexicalAnalyzer.symbol + " ");//записываем кодировку в файл
            }
            SW.Close();//закрываем файл, чтобы не потерять данные
            erro.Close();
        }

        static public void NextCh()//чтение следующей литеры
        {
            if (positionNow.charNumber == (line.Length - 1))//если текущая литера - последняя в строке
            {
                ListThisLine();//печатаем строку
                if (err.Count > 0)//в текущей строке обнаружены ошибки
                    ListErrors();//печатаем сообщение
                ReadNextLine();//читаем следующую строку
                positionNow.lineNumber = positionNow.lineNumber + 1;//увеличиваем номер строки
                positionNow.charNumber = 0;//на нулевую позицию в строке
            }
            else ++positionNow.charNumber;//иначе устанавливаем в качестве текущей следующую литеру
            Ch = line[positionNow.charNumber];//запоминаем ее координаты
        }

        private static void ListThisLine()//печает строку
        {
            Console.WriteLine(line);
        }

        private static void ReadNextLine()//считывает следующую строку
        {
            if (!File.EndOfStream)//если файл не закончился
            {
                line = File.ReadLine();//считываем следующую строку
                err = new List<Err>();
            }
            else
            {
                permisson = false;
                End();//иначе конец
            }
        }
        static StreamWriter erro = new StreamWriter("errors.txt", true);
        static void End()//конец
        {
            Console.WriteLine($"Компиляция завершена: : ошибок — {errCount}!");
            permisson = false;
            //erro.Close();
        }
        static void ListErrors()//вывод сообщения об ошибке
        {
            if (!СинтАнализатор)
            {
                int pos = 7 - $"{positionNow.lineNumber}".Length;//позиция(типа место для номера ошибки *0?* - длина(номер строки с пробелом))
                string s;
                foreach (Err item in err)//перебираем список ошибок
                {
                    ++errCount;//количество ошибок
                    s = "*";
                    if (errCount < 10) s += "0";//если меньше десяти, то номер ошибки начинается с 0
                    s += $"{errCount}*";//номер ошибки 
                    while (s.Length - 1 < pos + item.errorPosition.charNumber)//пока длина строки - 1 меньше позиции + номера литеры с ошибкой 
                        s += " ";//добавляем пробелы
                    s += $"^ ошибка код {item.errorCode}";//пишем код ошибки
                    Console.WriteLine(s);//выводим
                    s = $"**** {Erro.errorre[item.errorCode]}";//получаем из библиотеки пояснение к ошибке
                    Console.WriteLine(s);//выводим

                    s = $"Позиция: {item.errorPosition.lineNumber}; {item.errorPosition.charNumber}. Код ошибки: {item.errorCode}. Пояснение: {Erro.errorre[item.errorCode]}";
                    erro.WriteLine(s);//записываем в файл данные об ошибке
                }
            }
        }

        static public bool СинтАнализатор = true;//для записи в файл на этапе синтаксического анализатора
        static public void Error(byte errorCode, TextPosition position)//ошибка(код ошибки и позиция в тексте)
        {
            Err e;
            if (err.Count <= ERRMAX)//если количество ошибок в списке не больше 9
            {
                e = new Err(position, errorCode);//записываем информацию об ошибке
                err.Add(e);//добавляем ошибку в список

                if (СинтАнализатор)
                {
                    ++errCount;//количество ошибок
                    string s = $"**** {Erro.errorre[errorCode]}";//получаем из библиотеки пояснение к ошибке
                    Console.WriteLine(s);//выводим
                    s = "*";
                    if (errCount < 10) s += "0";//если меньше десяти, то номер ошибки начинается с 0
                    s += $"{errCount}*";//номер ошибки 
                    int pos = 7 - $"{positionNow.lineNumber}".Length;
                    while (s.Length - 1 < pos + positionNow.charNumber)//пока длина строки - 1 меньше позиции + номера литеры с ошибкой 
                        s += " ";//добавляем пробелы
                    s += $"ошибка код {errorCode}";//пишем код ошибки
                    Console.WriteLine(s);//выводим

                    s = $"Позиция: {positionNow.lineNumber}; {positionNow.charNumber}. Код ошибки: {errorCode}. Пояснение: {Erro.errorre[errorCode]}";
                    erro.WriteLine(s);//записываем в файл данные об ошибке
                }
            }
        }

    }
}