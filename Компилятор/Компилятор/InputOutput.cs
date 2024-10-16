using System;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
{
    struct TextPosition
    {
        public uint lineNumber; // номер строки
        public byte charNumber; // номер позиции в строке

        public TextPosition(uint ln = 0, byte c = 0)
        {
            lineNumber = ln;
            charNumber = c;
        }
    }

    struct Err
    {
        public TextPosition errorPosition;
        public byte errorCode;

        public Err(TextPosition errorPosition, byte errorCode)
        {
            this.errorPosition = errorPosition;
            this.errorCode = errorCode;
        }
    }


    class InputOutput
    {
        const byte ERRMAX = 9;
        public static char Ch { get; set; }
        public static TextPosition positionNow = new TextPosition();
        static string line;
        static byte lastInLine = 0;
        public static List<Err> err;
        static StreamReader File { get; set; }
        static uint errCount = 0;
        static bool prov=true;

        static StreamWriter errors = new StreamWriter("errors.txt");
        static public void NextCh()
        {
            if (positionNow.charNumber == lastInLine)
            {
                ListThisLine();
                errors.WriteLine(line);
                if (err.Count > 0)
                    ListErrors();
                ReadNextLine();
                positionNow.lineNumber++;
                positionNow.charNumber = 0;
            }
            else
            {
                ++positionNow.charNumber;
                
            }
            Ch = line[positionNow.charNumber];
        }

        private static void ListThisLine()
        {
            Console.WriteLine(line);
        }

        private static void ReadNextLine()
        {
            if (!File.EndOfStream)
            {
                line = File.ReadLine()+" ";
                lastInLine = Convert.ToByte(line.Length-1);
                err = new List<Err>();
            }
            else
            {
                End();
            }
        }

        static void End()
        {
            Console.WriteLine($"Компиляция завершена: : ошибок — {errCount}!");
            prov = false;
        }

        static void ListErrors()
        {
            int pos = 6 - $"{positionNow.lineNumber} ".Length;
            foreach (Err item in err)
            {
                ++errCount;
                string s = "";
                while (s.Length< item.errorPosition.charNumber) s += " ";
                s += $"^ ошибка код {item.errorCode}";
                s += " **";
                if (errCount < 10) s += "0";
                s += $"{errCount}**";
                Console.WriteLine(s);
                errors.WriteLine(s);
            }
        }

        static public void Error(byte errorCode, TextPosition position)
        {
            Err e;
            if (err.Count <= ERRMAX)
            {
                e = new Err(position, errorCode);
                err.Add(e);
            }
        }
        static public void Start()
        {
            prov = true;
            File = new StreamReader("a1.txt");
            positionNow = new TextPosition();
            ReadNextLine();
            Ch = line[0];
            StreamWriter sw = new StreamWriter("a2.txt");
            SyntaxAnalyzer sa = new SyntaxAnalyzer();
            LexicalAnalyzer.NextSym();
            sa.Program();
            //while (prov)
            //{
            //    la.NextSym();
            //    if (prov)
            //    {
            //        sw.Write(la.symbol + " ");
            //    }
            //}
            sw.Close();
            errors.Close();
        }
    }
}