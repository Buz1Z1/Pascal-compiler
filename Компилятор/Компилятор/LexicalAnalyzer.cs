using System;
namespace Компилятор
{
    class LexicalAnalyzer
    {
        public const byte
            star = 21, // *
            slash = 60, // /
            equal = 16, // =
            comma = 20, // ,
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
            quotmark = 22, //"          //добавлен
            onequotmark = 23, //'       //добавлен
            ident = 2,	// идентификатор
            floatc = 82,	// вещественная константа
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
            functionsy = 123,
            procedurensy = 124,
            integersy = 125,
            realsy = 126,
            boolsy = 127,
            charsy = 128;


        static public byte symbol; // код символа
        static public TextPosition token; // позиция символа
        static Keywords kk = new Keywords(); // словарь ключевых слов

        static public void NextSym()
        {
            while (InputOutput.Ch == ' ' || InputOutput.Ch=='\t') InputOutput.NextCh();
            token.lineNumber = InputOutput.positionNow.lineNumber;
            token.charNumber = InputOutput.positionNow.charNumber;
            //сканировать символ
            switch (InputOutput.Ch)
            {
                //    сканировать идентификатор или ключевое слово;
                case char c when char.IsDigit(c):
                    Int16 maxint = Int16.MaxValue;
                    string chislo = "";
                    byte check= 0;
                    while (InputOutput.Ch >= '0' && InputOutput.Ch <= '9'&& check!=2)
                    {
                        chislo += InputOutput.Ch;
                        InputOutput.NextCh();
                        if (InputOutput.Ch == '.')
                        {
                            chislo += ",";
                            check++; // счетчик точек в числе для вещественных чисел
                            InputOutput.NextCh();
                        }
                        if(check==2)
                            InputOutput.Error(203, InputOutput.positionNow);
                    }
                    int retint;
                    double retdouble;
                    if (check == 0)
                    {
                        retint = int.Parse(chislo);
                        if (retint <= maxint)
                            symbol = intc;//проверка на диапозон int
                        else
                        {
                            
                            InputOutput.Error(203, InputOutput.positionNow);
                        }
                    }
                    if(check==1)
                    {
                        retdouble = float.Parse(chislo);// проверка на диапозон float 
                        if (retdouble <= double.MaxValue)
                            symbol = floatc;
                        else // если в числе встретилась ошибка
                        {
                            InputOutput.Error(203, InputOutput.positionNow);
                        }
                    }
                    break;
                case char c when char.IsLetter(c):
                    string name = "";
                    while ((InputOutput.Ch >= 'a' && InputOutput.Ch <= 'z') ||
                            (InputOutput.Ch >= 'A' && InputOutput.Ch <= 'Z') ||
                            (InputOutput.Ch >= '0' && InputOutput.Ch <= '9'))
                    {
                        name += InputOutput.Ch;
                        InputOutput.NextCh();
                    }
                    if ((name.Length>1&&name.Length<10)&&(kk.Kw[Convert.ToByte(name.Length)].ContainsKey(name)))
                        symbol = kk.Kw[Convert.ToByte(name.Length)][name];
                    //symbol = код идентификатора или код ключевого слова
                    else 
                        symbol = ident;
                    break;
                //case '/'' :
                //        сканировать символьную константу;
                //    break;
                case '*':
                    InputOutput.NextCh();
                    if (InputOutput.Ch == ')')
                    {
                        symbol = rcomment;
                        InputOutput.NextCh();
                    }
                    else symbol = star;
                    break;
                case '+':
                    symbol = plus;
                    InputOutput.NextCh();
                    break;
                case '-':
                    symbol =minus;
                    InputOutput.NextCh();
                    break;
                case ',':
                    symbol =comma;
                    InputOutput.NextCh();
                    break;
                case '^':
                    symbol = arrow;
                    InputOutput.NextCh();
                    break;
                case '{':
                    symbol = flpar;
                    InputOutput.NextCh();
                    break;
                case '}':
                    symbol = frpar;
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
                case '(':
                    InputOutput.NextCh();
                    if(InputOutput.Ch=='*')
                    {
                        symbol = lcomment;
                        InputOutput.NextCh();
                    }
                    else
                    symbol = leftpar;
                    break;
                case ')':
                    symbol = rightpar;
                    InputOutput.NextCh();
                    break;
                case '/':
                    symbol = slash;
                    InputOutput.NextCh();
                    break;
                case '=':
                    symbol = equal;
                    InputOutput.NextCh();
                    break;
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
                case '"':
                    symbol = quotmark; 
                    InputOutput.NextCh(); 
                    break;
                case '\'':
                    symbol = onequotmark;
                    InputOutput.NextCh();
                    break;
                default: // если запрещенный символ
                    InputOutput.Error(6, InputOutput.positionNow);
                    InputOutput.NextCh();
                    break;
            }
            //return symbol;
        }
    }
}
