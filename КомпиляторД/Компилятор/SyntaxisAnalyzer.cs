using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Компилятор
{
    /*
    Разберемся, что надо сделать вообще: 
    1)Описание переменных стандартных типов: 
        1.integer (целое) 
        2.real (вещественное)
        3.boolean (логический)
        4.char (символьный).
    2)Описание простых типов. 
        1.Стандартные типы//это 1)
        2.Перечислимый тип:
            Имеет вид:
                Type <имя типа>=(список констант);
                Var <имя переменной>:<имя типа>;
            Пример:
                type
                    direction=(north, south, west, east);
                var
                    turn:direction;
            Или так:
                var
                    direction=(north, south, west, east);
        3.Ограниченный тип:
            Имеет вид:
                TYPE <имя типа>=константа1..константа2
            Пример:
                Type
                  Year = 1900..2000;
                Var
                  Y: Year;
            Или так:
                var a : 'a' .. 'c';
    3)Описание записей:
        Имеет вид:
            type < имя _ типа >=record
               <имя_поля1>: тип; 
               <имя_поля2>: тип; 
               …………………. 
               <имя_поля K >: тип 
            end;
        Пример:
            type anketa=record
                fio: string[45]; 
                pol: char; 
                dat_r: string[8]; 
                adres: string[50]; 
                curs: 1..5; 
                grupp: string[3];
            end;
            var 
                student: anketa; 
                student1: anketa;
    4.1)Операторы: присваивания(:=)
    4.2)присоединения:
        Имеет вид:
            with <имя_записи> do <действие с полем записи>;
        Пример:
            with pers do name := 'Иванов';
    4.3)составной (использовать простые переменные и поля записей):
        Этот оператор представляет собой совокупность произвольного числа операторов, 
        отделенных друг от друга точкой с запятой, и ограниченную операторными скобками begin и end. 
        Он воспринимается как единое целое и может находиться в любом месте программы, 
        где возможно наличие оператора. Иными словами, составной оператор позволяет объединить 
        несколько операторов в один.
    */
    class SyntaxisAnalyzer
    {
        static bool Belong(byte element, HashSet<byte> set)/* номер искомого элемента и множество, в котором ищем элемент */
        /* поиск элемента element в множестве set;
        функция возвращает значение "истина", если
        элемент присутствует в множестве, иначе — "ложь" */
        {
            return set.Contains(element);
        }
        static void SkipTo(HashSet<byte> where)
        {
            while (!Belong(LexicalAnalyzer.symbol, where))
                LexicalAnalyzer.NextSym();
        }
        static void SkipTo2(HashSet<byte> start, HashSet<byte> follow)
        {
            while (!Belong(LexicalAnalyzer.symbol, start) || //Чтобы было в "с чего начать" и не было во "что было"
                Belong(LexicalAnalyzer.symbol, follow))
                LexicalAnalyzer.NextSym();
        }
        static void SetDisjunct(HashSet<byte> set1, HashSet<byte> set2, out HashSet<byte> set3)
        {
            set3 = new HashSet<byte>();
            set3.UnionWith(set2); set3.UnionWith(set1);
        }
        static void accept(byte symbolExpected)//код ожидаемого символа
        {
            if (LexicalAnalyzer.symbol == symbolExpected)//сканируемый символ совпадает с ожидаемым 
                LexicalAnalyzer.NextSym();//сканировать следующий символ
            else InputOutput.Error(symbolExpected, LexicalAnalyzer.token);//сформировать сообщение об ошибке
        }
        //Начало//
        public void Программа()//анализ конструкции <программа>
        {
            Console.WriteLine("Начало");
            accept(LexicalAnalyzer.programsy);//program
            accept(LexicalAnalyzer.ident);//имя
            accept(LexicalAnalyzer.semicolon);//;
            StFoll obj = new StFoll();
            Блок(obj.sf[StFoll.begpart]); /* анализ конструкции <блок> */
            accept(LexicalAnalyzer.point);//.
            InputOutput.СинтАнализатор = false;
            Console.WriteLine("Конец");
            
        }
        void Блок(HashSet<byte> followers)// анализ конструкции <блок> 
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.begpart]))
            {
                InputOutput.Error(18,LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.begpart], followers);
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.begpart]))
            {
                SetDisjunct(obj.sf[StFoll.begpart], followers, out ptra);
                //labelpart();//раздел меток
                SetDisjunct(obj.sf[StFoll.st_typepsrt], followers, out ptra);
                //constpart();//раздел констант
                SetDisjunct(obj.sf[StFoll.st_varpart], followers, out ptra);
                РазделТипов(ptra); //раздел типов
                SetDisjunct(obj.sf[StFoll.st_procfuncpart], followers, out ptra);
                РазделПеременных(ptra);//раздел переменных
                //procfuncpart(); //раздел процедур и функций
                SetDisjunct(obj.sf[StFoll.st_final], followers, out ptra);
                РазделОператоров(ptra);// раздел операторов
                //if (!Belong(LexicalAnalyzer.symbol, followers))
                //{
                //    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                //    SkipTo(followers);
                //}
            }
        }
        //Раздел Типов//
        void РазделТипов(HashSet<byte> followers)//<раздел типов> ::=<пусто> | type <определение типа> ;{ <определение типа>;}
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.begpart]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.st_typepsrt], followers);//если тайпа нет, то пойдет в вар
            }
            Console.WriteLine("Раздел Типов");
            if (LexicalAnalyzer.symbol == LexicalAnalyzer.typesy)
            {
                LexicalAnalyzer.NextSym();//было type
                
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);
                
                accept(LexicalAnalyzer.ident);//<определение типа> ::= <имя> = <тип>
                accept(LexicalAnalyzer.equal);//=
                Тип(ptra);
                accept(LexicalAnalyzer.semicolon);
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.ident)//имя
                {
                    LexicalAnalyzer.NextSym();
                    accept(LexicalAnalyzer.equal);//=
                    Тип(ptra);
                    accept(LexicalAnalyzer.semicolon);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void Тип(HashSet<byte> followers)//<тип> ::= <простой тип> | <описание записей/комбинированный тип>//будем считать, что так и было
        /*<простой тип> ::= <перечислимый тип> | <ограниченный тип> | <имя типа>
          <комбинированный тип> ::= record <список полей> end*/
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.st_type], followers);
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                SetDisjunct(obj.sf[StFoll.st_record], followers, out ptra);

                if (LexicalAnalyzer.symbol == LexicalAnalyzer.recordsy)//если record, то это комбинированный тип
                {
                    LexicalAnalyzer.NextSym();
                    СписокПолей(ptra);
                    accept(LexicalAnalyzer.endsy);
                }
                else
                {
                    ПростойТип(ptra);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void ПростойТип(HashSet<byte> followers) /*<простой тип>::=<перечислимый тип> | <ограниченный тип> | <имя типа>*/
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.st_type], followers);
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);

                if (LexicalAnalyzer.symbol == LexicalAnalyzer.leftpar)//если скобочка
                    ПеречислимыйТип(ptra);
                else
                {
                    if (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark || LexicalAnalyzer.symbol == LexicalAnalyzer.intc)//если константа
                        ОграниченныйТип(ptra);
                    else
                        ИмяТипа();
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void ПеречислимыйТип(HashSet<byte> followers) /*<перечислимый тип>::=(<имя>{,<имя>})*/
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.st_type], followers);
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);

                accept(LexicalAnalyzer.leftpar);//(
                accept(LexicalAnalyzer.ident);//имя
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.comma)//,
                {
                    LexicalAnalyzer.NextSym();
                    accept(LexicalAnalyzer.ident);//имя
                }
                accept(LexicalAnalyzer.rightpar);//)
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void ОграниченныйТип(HashSet<byte> followers) /*<ограниченный тип>::=<константа>..<константа>*/
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.st_type], followers);
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);

                if (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark || LexicalAnalyzer.symbol == LexicalAnalyzer.intc)//константа это либо 'a', либо число
                    LexicalAnalyzer.NextSym();
                accept(LexicalAnalyzer.twopoints);//..
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark || LexicalAnalyzer.symbol == LexicalAnalyzer.intc)
                    LexicalAnalyzer.NextSym();
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void ИмяТипа()/*<имя типа>::=<имя>*/
        {
            if (LexicalAnalyzer.symbol == LexicalAnalyzer.realsy || LexicalAnalyzer.symbol == LexicalAnalyzer.charsy
                || LexicalAnalyzer.symbol == LexicalAnalyzer.booleansy || LexicalAnalyzer.symbol == LexicalAnalyzer.integersy
                || LexicalAnalyzer.symbol == LexicalAnalyzer.ident)
                LexicalAnalyzer.NextSym();
            else
                InputOutput.Error(10, LexicalAnalyzer.token);
        }
        void СписокПолей(HashSet<byte> followers)//<список полей> ::= <фиксированная часть> | <фиксированная часть> <вариантная часть>|<вариантная часть>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_record]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.st_type], followers);//если не из списка то, что можетт встретится в рекорд, то идет дальше по типам
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_record]))
            {
                SetDisjunct(obj.sf[StFoll.st_record], followers, out ptra);
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.ident)
                {
                    ФиксированнаяЧасть(ptra);//1 случай
                    if (LexicalAnalyzer.symbol == LexicalAnalyzer.casesy)//2 случай
                    {
                        ВариантнаяЧасть();
                    }
                }
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.casesy)//3 случай
                {
                    ВариантнаяЧасть();
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void ФиксированнаяЧасть(HashSet<byte> followers)//<фиксированная часть> ::= <секция записи> {; <секция записи>}
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_record]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.st_type], followers);//если не из списка то, что можетт встретится в рекорд, то идет дальше по типам
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_record]))
            {
                SetDisjunct(obj.sf[StFoll.st_record], followers, out ptra);
                СекцияЗаписи(ptra);
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.semicolon)
                {
                    LexicalAnalyzer.NextSym();
                    СекцияЗаписи(ptra);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void СекцияЗаписи(HashSet<byte> followers)//<секция записи> ::= <имя поля> {, <имя поля>}: <тип> | <пусто>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_record]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.st_type], followers);//если не из списка то, что можетт встретится в рекорд, то идет дальше по типам
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_record]))
            {
                SetDisjunct(obj.sf[StFoll.st_record], followers, out ptra);
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.ident)//так как возможно еще <пусто>
                {
                    LexicalAnalyzer.NextSym();
                    while (LexicalAnalyzer.symbol == LexicalAnalyzer.comma)
                    {
                        LexicalAnalyzer.NextSym();
                        accept(LexicalAnalyzer.ident);
                    }
                    accept(LexicalAnalyzer.colon);
                    Тип(ptra);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void ВариантнаяЧасть()//<вариантная часть> ::= case <поле признака > <имя типа> of <вариант> {; <вариант>}
        {
            accept(LexicalAnalyzer.casesy);
            ПолеПризнака();
            ИмяТипа();
            accept(LexicalAnalyzer.ofsy);
            Вариант();
            while (LexicalAnalyzer.symbol == LexicalAnalyzer.semicolon)
            {
                LexicalAnalyzer.NextSym();
                Вариант();
            }
        }
        void ПолеПризнака()//<поле признака> ::= <имя поля> : | <пусто>
        {
            if (LexicalAnalyzer.symbol == LexicalAnalyzer.ident)
            {
                LexicalAnalyzer.NextSym();
                accept(LexicalAnalyzer.colon);
            }
        }
        void Вариант()//<вариант> ::= <список меток варианта> : (<список полей>) | <пусто>
        {
            if (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark || LexicalAnalyzer.symbol == LexicalAnalyzer.intc)
            {
                СписокМетокВарианта();
                accept(LexicalAnalyzer.colon);
                accept(LexicalAnalyzer.leftpar);
                StFoll obj = new StFoll();
                СписокПолей(obj.sf[StFoll.st_varpart]);
                accept(LexicalAnalyzer.rightpar);
            }
        }
        void СписокМетокВарианта()//<список меток варианта> ::= <метка варианта> {, <метка варианта>}
                                  //<метка варианта> :: = <константа>
        {
            if (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark || LexicalAnalyzer.symbol == LexicalAnalyzer.intc)//константа
            {
                LexicalAnalyzer.NextSym();
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark || LexicalAnalyzer.symbol == LexicalAnalyzer.intc)
                {
                    LexicalAnalyzer.NextSym();
                }
            }
            else
                InputOutput.Error(50, LexicalAnalyzer.token);//"ошибка в константе"
        }
        //Раздел Переменных//
        void РазделПеременных(HashSet<byte> followers)//анализ конструкции <раздел переменных> 
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_varpart]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.id_starters], followers);//если не вар, то в бегин
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_varpart]))
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);
                Console.WriteLine("Раздел Переменных");
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.varsy)
                {
                    accept(LexicalAnalyzer.varsy);
                    do
                    {
                        ОписаниеОднотипныхПеременных(ptra);//если идет описание однотипных переменных
                        accept(LexicalAnalyzer.semicolon);
                    }
                    while (LexicalAnalyzer.symbol == LexicalAnalyzer.ident);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void ОписаниеОднотипныхПеременных(HashSet<byte> followers)//анализ конструкции <описание однотипных переменных>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.id_starters]))
            {
                InputOutput.Error(2, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.id_starters], followers);//если не вар, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.id_starters]))
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);
                accept(LexicalAnalyzer.ident);
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.comma)//,
                {
                    LexicalAnalyzer.NextSym();
                    accept(LexicalAnalyzer.ident);
                }
                accept(LexicalAnalyzer.colon);//:
                Тип(ptra);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        //Раздел Операторов//
        void РазделОператоров(HashSet<byte> followers)//<раздел операторов> ::= <составной оператор>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_varpart]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_varpart]))
            {
                //SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);
                Console.WriteLine("Раздел Операторов");
                СоставнойОператор(followers);//состовной оператор
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void СоставнойОператор(HashSet<byte> followers)//<составной оператор> ::= begin <оператор> {; <оператор>} end
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_varpart]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_varpart]))
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);
                accept(LexicalAnalyzer.beginsy);
                Оператор(ptra);//<оператор>
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.semicolon)
                {
                    LexicalAnalyzer.NextSym();
                    if(LexicalAnalyzer.symbol != LexicalAnalyzer.endsy)
                        Оператор(ptra);
                }
                accept(LexicalAnalyzer.endsy);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void Оператор(HashSet<byte> followers)//<оператор> ::= <непомеченный оператор> | <метка>:<непомеченный оператор>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_begin]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_begin]))
            {
                НепомеченныйОператор(followers);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void НепомеченныйОператор(HashSet<byte> followers)//<непомеченный оператор> ::= <простой оператор> | <сложный оператор>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_begin]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_begin]))
            {
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.ident)
                {
                    ПростойОператор(followers);//простые операторы начинаются с имени(то есть с идентификатора)
                }
                else
                { 
                        СложныйОператор(followers); 
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void ПростойОператор(HashSet<byte> followers)//<простой оператор> ::= <оператор присваивания> | <оператор процедуры>| <оператор перехода> | <пустой оператор>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.id_starters]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.id_starters]))
            {
                ОператорПрисваивания(followers);//будем считать, что все остальное тут тоже есть
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void ОператорПрисваивания(HashSet<byte> followers)//<оператор присваивания> ::= <переменная> := <выражение> | <имя функции> :=<выражение>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.id_starters]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.id_starters]))
            {
                SetDisjunct(obj.sf[StFoll.after_id], followers, out ptra);
                Переменная(ptra);//переменная или имя функции
                accept(LexicalAnalyzer.assign);
                Выражение(ptra);//выражение
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void Переменная(HashSet<byte> followers)//<переменная> ::= <полная переменная> | <компонента переменной> | <указанная переменная>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.id_starters]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.id_starters]))
            {
                accept(LexicalAnalyzer.ident);//полная переменная
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.point)//компонента переменной
                {
                    LexicalAnalyzer.NextSym();
                    accept(LexicalAnalyzer.ident);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void Выражение(HashSet<byte> followers) //выражение
        { //< выражение > ::= < простое выражение > | < простое выражение > < операция отношения >< простое выражение > 
            //<операция отношения> ::= = | <> | < | <= | >= | > | in
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_expression]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.id_starters], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_expression]))
            {
                ПростоеВыражение(followers);
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.equal || LexicalAnalyzer.symbol == LexicalAnalyzer.latergreater
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.later || LexicalAnalyzer.symbol == LexicalAnalyzer.greater
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.laterequal || LexicalAnalyzer.symbol == LexicalAnalyzer.greaterequal
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.insy)
                {
                    LexicalAnalyzer.NextSym();
                    ПростоеВыражение(followers);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void ПростоеВыражение(HashSet<byte> followers)//<простое выражение> ::= <знак> <слагаемое> { <+ | - | or> <слагаемое>}
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_expression]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_expression]))
            {
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.plus || LexicalAnalyzer.symbol == LexicalAnalyzer.minus)
                {
                    LexicalAnalyzer.NextSym();//это был знак, переводим
                }
                Слагаемое();
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.plus || LexicalAnalyzer.symbol == LexicalAnalyzer.minus)
                {
                    LexicalAnalyzer.NextSym();
                    Слагаемое();
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }

        }
        void Слагаемое()//<слагаемое> ::= <множитель> { < * | / | div | mod | and> <множитель>}
        {
            Множитель();
            while (LexicalAnalyzer.symbol == LexicalAnalyzer.star || LexicalAnalyzer.symbol == LexicalAnalyzer.slash
                || LexicalAnalyzer.symbol == LexicalAnalyzer.divsy || LexicalAnalyzer.symbol == LexicalAnalyzer.modsy
                || LexicalAnalyzer.symbol == LexicalAnalyzer.andsy)
            {
                LexicalAnalyzer.NextSym();
                Множитель();
            }
        }
        void Множитель()//переменная или число
        {
            if (LexicalAnalyzer.symbol == LexicalAnalyzer.ident || LexicalAnalyzer.symbol == LexicalAnalyzer.intc)//множитель(переменная или целое число)
            {
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.intc)
                {
                    LexicalAnalyzer.NextSym();
                    if (LexicalAnalyzer.symbol == LexicalAnalyzer.point)//на случай, если это вещественное число
                    {
                        if (LexicalAnalyzer.symbol == LexicalAnalyzer.intc)
                            LexicalAnalyzer.NextSym();
                        else
                            InputOutput.Error(10, LexicalAnalyzer.token);//ошибка в типе
                    }
                }
                else
                    LexicalAnalyzer.NextSym();
            }
            else
            {
                InputOutput.Error(2, LexicalAnalyzer.token);
                LexicalAnalyzer.NextSym();
            }
        }
        void СложныйОператор(HashSet<byte> followers)//<сложный оператор> ::= <составной оператор> | <циклы> |<оператор присоединения>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_begin]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_begin]))
            {
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.withsy)//оператор присоединения
                {
                    ОператорПрисоединения(followers);
                }
                else
                {
                    if (LexicalAnalyzer.symbol == LexicalAnalyzer.beginsy)//составной оператор
                        СоставнойОператор(followers);
                    //if (LexicalAnalyzer.symbol == LexicalAnalyzer.ifsy)//а еще форси и вайлси, ну и кейси туда же. Будем считать, что оно есть
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void ОператорПрисоединения(HashSet<byte> followers)//<оператор присоединения> ::=with <список переменных-записей> do <оператор>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_begin]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); /* ошибка в разделе описаний */
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_begin]))
            {
                accept(LexicalAnalyzer.withsy);
                СписокПеременныхЗаписей(followers);
                accept(LexicalAnalyzer.dosy);
                Оператор(followers);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); /* запрещенный символ */
                    SkipTo(followers);
                }
            }
        }
        void СписокПеременныхЗаписей(HashSet<byte> followers)//<список переменных-записей> ::= <переменная-запись> {, <переменная-запись>}
        {
            Переменная(followers);
            while(LexicalAnalyzer.symbol==LexicalAnalyzer.comma)
            {
                LexicalAnalyzer.NextSym();
                Переменная(followers);
            }
        }
    }
}
