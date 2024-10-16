using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
{
    class LexicalAnalyzer
    {
        public const byte
            star = 21, // *
            slash = 60, // /
            equal = 16, // =
            comma = 20, // ,
            quotmark = 22, //"          //добавила, так как этого не было
            onequotmark = 23, //'       //добавила, так как этого не было
            semicolon = 14, // ;
            colon = 5, // :
            point = 61,	// .
            arrow = 62,	// ^
            leftpar = 9,	// (
            rightpar = 4,	// )
            lbracket = 11,	// [
            rbracket = 12,	// ]
            flpar = 63,	// {
            frpar = 64,	// }
            later = 65,	// <
            greater = 66,	// >
            laterequal = 67,	//  <=
            greaterequal = 68,	//  >=
            latergreater = 69,	//  <>
            plus = 70,	// +
            minus = 71,	// –
            lcomment = 72,	//  (*
            rcomment = 73,	//  *)
            assign = 51,	//  :=
            twopoints = 74,	//  ..
            ident = 2,	// идентификатор
            doubletc = 82,	// вещественная константа
            intc = 15,	// целая константа
            casesy = 31,
            elsesy = 32,
            filesy = 57,
            gotosy = 33,
            thensy = 52,
            typesy = 34,
            untilsy = 53,
            dosy = 54,
            withsy = 37,
            ifsy = 56,
            insy = 100,
            ofsy = 101,
            orsy = 102,
            tosy = 103,
            endsy = 104,
            varsy = 105,
            divsy = 106,
            andsy = 107,
            notsy = 108,
            forsy = 109,
            modsy = 110,
            nilsy = 111,
            setsy = 112,
            beginsy = 113,
            whilesy = 114,
            arraysy = 115,
            constsy = 116,
            labelsy = 117,
            downtosy = 118,
            packedsy = 119,
            recordsy = 120,
            repeatsy = 121,
            programsy = 122,
            integersy = 10,       //добавила, так как этого не было//символ должен означать, какая ошибка выйдет в случае его отстуствия
            realsy = 10,       //добавила, так как этого не было//поэтомы выбрала 10
            charsy = 10,       //добавила, так как этого не было//ошибка в типе
            booleansy = 10,    //добавила, так как этого не было
            functionsy = 123,
            procedurensy = 124;

        public static byte symbol; // код символа
        public static TextPosition token; // позиция символа
        string addrName; // адрес идентификатора в таблице имен

        public static byte nomerident;//номер идентификатора, который мы будем писать после ident(наверное)

        public static void NextSym()
        {
            int nmb_int; // значение целой константы
            double nmb_double; // значение вещественной константы
            char one_symbol; // значение символьной константы

            while (InputOutput.Ch == ' '|| InputOutput.Ch=='\t') InputOutput.NextCh();//на пробелах переводим на следующую литеру
            token.lineNumber = InputOutput.positionNow.lineNumber;//меняем позицию
            token.charNumber = InputOutput.positionNow.charNumber;

            bool уже_закодировано = true;//так как буквы и цифры я кодирую не через кейсы, то нужно смотреть, чтобы после ифов не зашло в кейсы
            if ((InputOutput.Ch >= 'a' && InputOutput.Ch <= 'z') || (InputOutput.Ch >= 'A' && InputOutput.Ch <= 'Z'))
            {//если начинается с буквы
                string chars = "";//здесь храним все слово
                chars += InputOutput.Ch;//записываем первую литеру
                InputOutput.NextCh();//переводим дальше
                byte line1 = (byte)InputOutput.positionNow.lineNumber;//нужно смотреть, чтобы слово было в одной строке, а то если тут буквы и там буквы, а в конце строки нет пробела, то он считывает это как одно
                byte line2 = (byte)InputOutput.positionNow.lineNumber;
                while (((InputOutput.Ch >= 'a' && InputOutput.Ch <= 'z') || (InputOutput.Ch >= 'A' && InputOutput.Ch <= 'Z') 
                && (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')) && line1==line2)//пока буквы и цифры(первая литера цифрой быть не могла) и в одной строке
                {
                    chars += InputOutput.Ch;//записываем
                    InputOutput.NextCh();//переводим
                    line2 = (byte)InputOutput.positionNow.lineNumber;//обновляем лайн2
                }
                //Console.WriteLine(chars);
                if (chars.Length >= 2 && chars.Length <= 9)//если больше двух, так как все ключевые слова больше двух
                {
                    Keywords keywords = new Keywords();
                    Dictionary<byte, Dictionary<string, byte>> kw = keywords.kw;
                    Dictionary<string, byte> tmp = kw[(byte)chars.Length];//берем библиотеку, которая отвечает за слова такой длины
                    if (tmp.ContainsKey(chars))//если там есть наше слово
                    {
                        foreach (var keyword in tmp)
                        {
                            if (keyword.Key == chars)
                            {
                                symbol = keyword.Value;//в символ записываем его кодировку
                            }
                        }
                    }
                    else//если нет
                    {
                        symbol = ident;//пишем два
                        if (Identific.Ident.ContainsValue(chars))//если такой идент уже был
                        {
                            foreach (var i in Identific.Ident)
                            {
                                if (i.Value == chars)
                                {
                                    nomerident = i.Key;//в символ записываем его кодировку
                                }
                            }
                        }
                        else//если нет
                        { 
                            bool flag3 = true;//если записали, то заканчиваем
                            for (int i = 0; i < 257 && flag3; i++)
                            {
                                if (!Identific.Ident.ContainsKey((byte)i))//если нет
                                {
                                    flag3 = false;
                                    Identific.Ident.Add((byte)i,chars);//добавляем
                                    nomerident = (byte)i;//присваиваем 
                                }
                            }
                            
                        }
                    }
                }
                else//если нет
                {
                    symbol = ident;
                    if (Identific.Ident.ContainsValue(chars))//если такой идент уже был
                    {
                        foreach (var i in Identific.Ident)
                        {
                            if (i.Value == chars)
                            {
                                nomerident = i.Key;//в символ записываем его кодировку
                            }
                        }
                    }
                    else//если нет
                    {
                        bool flag3 = true;//если записали, то заканчиваем
                        for (int i = 0; i < 257 && flag3; i++)
                        {
                            if (!Identific.Ident.ContainsKey((byte)i))//если нет
                            {
                                flag3 = false;
                                Identific.Ident.Add((byte)i, chars);//добавляем
                                nomerident = (byte)i;//присваиваем 
                            }
                        }

                    }
                }
                уже_закодировано = false;
            }
            if (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')//если начинается с цифры
            {
                string chislo = "";
                chislo += InputOutput.Ch;
                InputOutput.NextCh();
                bool flag = true;
                while ((InputOutput.Ch >= '0' && InputOutput.Ch <= '9')/*||(InputOutput.Ch == '.')*/)//записываем все число, даже с точкой
                {
                    //if (InputOutput.Ch == '.')//если есть точка, то отправим потом к вещественным
                    //    flag = false;

                    chislo += InputOutput.Ch;
                    InputOutput.NextCh();
                }

                if (flag)//если нет точки
                {
                    byte digit;
                    Int16 maxint = Int16.MaxValue;
                    nmb_int = 0;
                    bool flag2 = true;//это чтобы выйти из фора, если вдруг где-то ошибка
                    for(int i = 0; i < chislo.Length && flag2; i++)
                    {
                        digit = (byte)(chislo[i] - '0');//цифра
                        if (nmb_int < maxint / 10 || //если значение меньше максимального (без последней цифры)
                            (nmb_int == maxint / 10 && digit <= maxint % 10))//или равно, но последняя цифра меньше максимума
                            nmb_int = 10 * nmb_int + digit;//то складываем
                        else
                        {
                            // константа превышает предел
                            InputOutput.Error(203, token);//ошибка
                            nmb_int = 0;//обнуляем цифру
                            flag2 = false;//пропускаем
                        }
                        //InputOutput.NextCh();
                    }
                    symbol = intc;
                }
                //else//если вещественное число
                //{
                //    byte digit;
                //    float maxdouble = (float)Double.MaxValue;//float побольше дабл. Чтобы понять, больше ли введенное число максимального 
                //    bool flag2 = true;//это чтобы выйти из фора, если вдруг где-то ошибка
                //    int toch = 0;//количество точек
                //    for(int i = 0;i<chislo.Length && toch<=1; i++)//больше 1 точки быть не может
                //    {
                //        if (chislo[i]=='.')
                //        {
                //            chislo = chislo.Remove(i, 1).Insert(i, ','.ToString());//заменяем на запятую, чтобы потом конвертировать в число для c#
                //            toch++;
                //        }
                //    }
                //    if (toch >= 2)//если больше одной, то
                //    {
                //        InputOutput.Error(201, token);//ошибка в вещественной константе: должна идти цифра
                //    }
                //    else//если все хорошо
                //    {
                //        float chi = (float)Convert.ToDouble(chislo);//конвертируем цыганскими фокусами

                //        if (chi > maxdouble)//если число больше максимально разрешенного в дабл
                //        {
                //            // константа превышает предел
                //            InputOutput.Error(207, token);//слишком большая вещественная константа
                //            nmb_double = 0;//обнуляем цифру
                //            flag2 = false;//пропускаем
                //        }
                //        //InputOutput.NextCh();
                //    }
                //    symbol = doubletc;
                //}
                уже_закодировано = false;
            }

            //сканировать символ
            if (уже_закодировано)//если это были не числа и слова, то кейсы
            {
                switch (InputOutput.Ch)
                {
                    case '<':
                        InputOutput.NextCh();
                        if (InputOutput.Ch == '=')
                        {
                            symbol = laterequal; InputOutput.NextCh();
                        }
                        else
                        if (InputOutput.Ch == '>')
                        {
                            symbol = latergreater; InputOutput.NextCh();
                        }
                        else
                            symbol = later;
                        break;
                    case ':':
                        InputOutput.NextCh();
                        if (InputOutput.Ch == '=')
                        {
                            symbol = assign; InputOutput.NextCh();
                        }
                        else
                            symbol = colon;
                        break;
                    case ';':
                        symbol = semicolon;
                        InputOutput.NextCh();
                        break;
                    case '.':
                        InputOutput.NextCh();
                        if (InputOutput.Ch == '.')
                        {
                            symbol = twopoints; InputOutput.NextCh();
                        }
                        else symbol = point;
                        break;
                    case '*':
                        InputOutput.NextCh();
                        if (InputOutput.Ch == ')')
                        {
                            symbol = rcomment; InputOutput.NextCh();
                        }
                        else
                            symbol = star;
                        break;
                    case '/':
                        symbol = slash; InputOutput.NextCh();
                        break;
                    case '=':
                        symbol = equal; InputOutput.NextCh();
                        break;
                    case ',':
                        symbol = comma; InputOutput.NextCh();
                        break;
                    case '^':
                        symbol = arrow; InputOutput.NextCh();
                        break;
                    case '(':
                        InputOutput.NextCh();
                        if (InputOutput.Ch == '*')
                        {
                            symbol = lcomment; InputOutput.NextCh();
                        }
                        else
                            symbol = leftpar;
                        break;
                    case ')':
                        symbol = rightpar;
                        InputOutput.NextCh();
                        break;
                    case '[':
                        symbol = lbracket;
                        InputOutput.NextCh();
                        break;
                    case ']':
                        symbol = rbracket;
                        InputOutput.NextCh();
                        break;
                    case '{':
                        symbol = flpar;
                        InputOutput.NextCh();
                        while (InputOutput.Ch != '}')//пропускаем комментарии
                        {
                            InputOutput.NextCh();
                        }
                        break;
                    case '}':
                        symbol = frpar;
                        InputOutput.NextCh();
                        break;
                    case '>':
                        InputOutput.NextCh();
                        if (InputOutput.Ch == '=')
                        {
                            symbol = greaterequal; InputOutput.NextCh();
                        }
                        else
                            symbol = greater;
                        break;
                    case '+':
                        symbol = plus; InputOutput.NextCh(); break;
                    case '-':
                        symbol = minus; InputOutput.NextCh(); break;
                    case '"':
                        symbol = quotmark; InputOutput.NextCh(); break;
                    case '\'':
                        symbol = onequotmark; InputOutput.NextCh(); 
                        //while (InputOutput.Ch != '\'')
                        //{
                        //    InputOutput.NextCh();
                        //}
                        break;
                    case '?':
                        InputOutput.err.Add(new Err(token,6));//ошибка 6: запрещенный символ
                        InputOutput.NextCh(); symbol = 6; break;
                    case '&':
                        InputOutput.err.Add(new Err(token, 6));
                        InputOutput.NextCh(); symbol = 6; break;
                    case '%':
                        InputOutput.err.Add(new Err(token, 6));
                        InputOutput.NextCh();  symbol = 6; break;
                    default: break;
                }
            }

            //FileStream zacode = new FileStream("1.txt", FileMode.Create);//Создаем файл для запики закодированых символов
            //StreamWriter file = new StreamWriter(zacode);
            //file.Write(symbol + " ");

            //file.Close();
            //zacode.Close();
            //Console.WriteLine(symbol + " ");
        }

    }
}
