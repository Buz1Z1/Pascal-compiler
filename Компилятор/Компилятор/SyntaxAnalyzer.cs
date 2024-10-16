using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Компилятор
{
    internal class SyntaxAnalyzer // 9 вариант
    {
        public void accept(byte symbolexpected) // код ожидаемого символа
        {
            if (LexicalAnalyzer.symbol == symbolexpected) //если совпадает, то сканирует следующий 
                LexicalAnalyzer.NextSym();
            else
            {
                InputOutput.Error(symbolexpected, LexicalAnalyzer.token);
            }
        }
        // для нейтрализации ошибок
        static bool Belong(byte element, HashSet<byte> set)/* номер искомого элемента и множество, в котором ищем элемент */
        {
            return set.Contains(element);
        }
        static void SkipTo(HashSet<byte> where)// пропускает, пока не встретит из множества
        {
            while (!Belong(LexicalAnalyzer.symbol, where))
                LexicalAnalyzer.NextSym();
        }
        static void SkipTo2(HashSet<byte> start, HashSet<byte> follow)// проверка на принадлежность одному из множеств
        {
            while (!Belong(LexicalAnalyzer.symbol, start) ||
                Belong(LexicalAnalyzer.symbol, follow))
                LexicalAnalyzer.NextSym();
        }
        static void SetDisjunct(HashSet<byte> set1, HashSet<byte> set2, out HashSet<byte> set3)
        {
            set3 = new HashSet<byte>();
            set3.UnionWith(set2); set3.UnionWith(set1);
        }
        // описание блоков
        public void Program()
        {
            accept(LexicalAnalyzer.programsy);// program
            accept(LexicalAnalyzer.ident); // имя
            accept(LexicalAnalyzer.semicolon); // ;
            StFoll obj = new StFoll();
            block(obj.sf[StFoll.begpart]); //анализ конструкций 
            accept(LexicalAnalyzer.point);// .

        }
        public void block(HashSet<byte> followers)// блок с анализом всех разделов
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.begpart]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.begpart], followers);
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.begpart]))
            {
                SetDisjunct(obj.sf[StFoll.st_typepsrt], followers, out ptra);
                SetDisjunct(obj.sf[StFoll.st_varpart], followers, out ptra);
                РазделТипов(ptra); //раздел типов
                SetDisjunct(obj.sf[StFoll.st_procfuncpart], followers, out ptra);
                РазделПеременных(ptra);//раздел переменных
                SetDisjunct(obj.sf[StFoll.st_final], followers, out ptra);
                РазделОператоров(ptra);// раздел операторов
            }

        }
        
        public void РазделТипов(HashSet<byte> followers)//описание записи
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.begpart]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.st_typepsrt], followers);//если type нет, то в var
            }
            if (LexicalAnalyzer.symbol == LexicalAnalyzer.typesy)
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);
                accept(LexicalAnalyzer.typesy);
                accept(LexicalAnalyzer.ident);
                accept(LexicalAnalyzer.equal);
                Тип(ptra);
                accept(LexicalAnalyzer.semicolon);
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.ident)
                {
                    accept(LexicalAnalyzer.ident);
                    accept(LexicalAnalyzer.equal);
                    Тип(ptra);
                    accept(LexicalAnalyzer.semicolon);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token);
                    SkipTo(followers);
                }
            }
        }
        public void Тип(HashSet<byte> followers)//определитель типов
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
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
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.st_type], followers);
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);

                if (LexicalAnalyzer.symbol == LexicalAnalyzer.leftpar)
                    ПеречислимыйТип(ptra);
                else
                {
                    if (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark 
                        || LexicalAnalyzer.symbol == LexicalAnalyzer.intc)
                        ОграниченныйТип(ptra);
                    else
                        ИмяТипа();
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.st_type], followers);
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);

                accept(LexicalAnalyzer.leftpar);
                accept(LexicalAnalyzer.ident);
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.comma)
                {
                    LexicalAnalyzer.NextSym();
                    accept(LexicalAnalyzer.ident);
                }
                accept(LexicalAnalyzer.rightpar);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.st_type], followers);
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_type]))
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);

                if (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark ||
                    LexicalAnalyzer.symbol == LexicalAnalyzer.intc)
                    LexicalAnalyzer.NextSym();
                accept(LexicalAnalyzer.twopoints);
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark ||
                    LexicalAnalyzer.symbol == LexicalAnalyzer.intc)
                    LexicalAnalyzer.NextSym();
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
                    SkipTo(followers);
                }
            }
        }
        void ИмяТипа()/*<имя типа>::=<имя>*/
        {
            if (LexicalAnalyzer.symbol == LexicalAnalyzer.realsy 
                || LexicalAnalyzer.symbol == LexicalAnalyzer.charsy
                || LexicalAnalyzer.symbol == LexicalAnalyzer.boolsy 
                || LexicalAnalyzer.symbol == LexicalAnalyzer.integersy
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.st_type], followers);
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_record]))
            {
                SetDisjunct(obj.sf[StFoll.st_record], followers, out ptra);
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.ident)
                {
                    ФиксированнаяЧасть(ptra);
                    if (LexicalAnalyzer.symbol == LexicalAnalyzer.casesy)
                    {
                        ВариантнаяЧасть();
                    }
                }
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.casesy)
                {
                    ВариантнаяЧасть();
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.st_type], followers);
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
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.st_type], followers);
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_record]))
            {
                SetDisjunct(obj.sf[StFoll.st_record], followers, out ptra);
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.ident)
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
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
            if (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark 
                || LexicalAnalyzer.symbol == LexicalAnalyzer.intc)
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
            if (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark
                || LexicalAnalyzer.symbol == LexicalAnalyzer.intc)//константа
            {
                LexicalAnalyzer.NextSym();
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.onequotmark 
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.intc)
                {
                    LexicalAnalyzer.NextSym();
                }
            }
            else
                InputOutput.Error(50, LexicalAnalyzer.token);//"ошибка в константе"
        }

        //анализ конструкции раздела переменных

        void РазделПеременных(HashSet<byte> followers)
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_varpart]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.id_starters], followers);//если не вар, то в бегин
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_varpart]))
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.varsy)
                {
                    accept(LexicalAnalyzer.varsy);
                    do
                    {
                        ОписаниеОднотипныхПеременных(ptra);
                        accept(LexicalAnalyzer.semicolon);
                    }
                    while (LexicalAnalyzer.symbol == LexicalAnalyzer.ident);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(2, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.id_starters], followers);//если не вар, то до ;
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
                accept(LexicalAnalyzer.colon);
                Тип(ptra);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
                    SkipTo(followers);
                }
            }
        }

        //анализ раздела операторов
        void РазделОператоров(HashSet<byte> followers)//<раздел операторов> ::= <составной оператор>
        {
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_varpart]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до ;
            }
            SetDisjunct(obj.sf[StFoll.st_begin], followers, out ptra);
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_varpart]))
            {
                СоставнойОператор(followers);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до ;
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_varpart]))
            {
                SetDisjunct(obj.sf[StFoll.after_var], followers, out ptra);
                accept(LexicalAnalyzer.beginsy);
                Оператор(ptra);
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.semicolon)
                {
                    LexicalAnalyzer.NextSym();
                    if (LexicalAnalyzer.symbol != LexicalAnalyzer.endsy)
                        Оператор(ptra);
                }
                accept(LexicalAnalyzer.endsy);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до ;
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_begin]))
            {
                НепомеченныйОператор(followers);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до ;
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_begin]))
            {
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.ident)
                {
                    ПростойОператор(followers);
                }
                else
                {
                    СложныйОператор(followers);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до ;
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.id_starters]))
            {
                ОператорПрисваивания(followers);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до ;
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.id_starters]))
            {
                SetDisjunct(obj.sf[StFoll.after_id], followers, out ptra);
                Переменная(ptra);
                accept(LexicalAnalyzer.assign);
                Выражение(ptra);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до ;
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.id_starters]))
            {
                SetDisjunct(obj.sf[StFoll.after_id], followers, out ptra);
                accept(LexicalAnalyzer.ident);//полная переменная
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.point)//компонента переменной
                {
                    LexicalAnalyzer.NextSym();
                    accept(LexicalAnalyzer.ident);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
                    SkipTo(followers);
                }
            }
        }
        void Выражение(HashSet<byte> followers)
        { //< выражение > ::= < простое выражение > | < простое выражение > < операция отношения >< простое выражение > 
            //<операция отношения> ::= = | <> | < | <= | >= | > | in
            HashSet<byte> ptra;//хранение внешних символов
            StFoll obj = new StFoll();
            if (!Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_expression]))
            {
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.id_starters], followers);//если не бегин, то до ;
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_expression]))
            {
                ПростоеВыражение(followers);
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.equal 
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.latergreater
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.later 
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.greater
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.laterequal 
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.greaterequal
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.insy)
                {
                    LexicalAnalyzer.NextSym();
                    ПростоеВыражение(followers);
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до ;
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_expression]))
            {
                if (LexicalAnalyzer.symbol == LexicalAnalyzer.plus 
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.minus)
                {
                    LexicalAnalyzer.NextSym();
                }
                Слагаемое();
                while (LexicalAnalyzer.symbol == LexicalAnalyzer.plus 
                    || LexicalAnalyzer.symbol == LexicalAnalyzer.minus)
                {
                    LexicalAnalyzer.NextSym();
                    Слагаемое();
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
                    SkipTo(followers);
                }
            }

        }
        void Слагаемое()//<слагаемое> ::= <множитель> { < * | / | div | mod | and> <множитель>}
        {
            Множитель();
            while (LexicalAnalyzer.symbol == LexicalAnalyzer.star 
                || LexicalAnalyzer.symbol == LexicalAnalyzer.slash
                || LexicalAnalyzer.symbol == LexicalAnalyzer.divsy
                || LexicalAnalyzer.symbol == LexicalAnalyzer.modsy
                || LexicalAnalyzer.symbol == LexicalAnalyzer.andsy)
            {
                LexicalAnalyzer.NextSym();
                Множитель();
            }
        }
        void Множитель()//переменная или число
        {
            if (LexicalAnalyzer.symbol == LexicalAnalyzer.ident ||
                LexicalAnalyzer.symbol == LexicalAnalyzer.intc ||
                LexicalAnalyzer.symbol==LexicalAnalyzer.floatc)
            {
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
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
                }
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
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
                InputOutput.Error(18, LexicalAnalyzer.token); //ошибка в разделе описаний
                SkipTo2(obj.sf[StFoll.after_var], followers);//если не бегин, то до точки с запятой
            }
            if (Belong(LexicalAnalyzer.symbol, obj.sf[StFoll.st_begin]))
            {
                SetDisjunct(obj.sf[StFoll.after_id], followers, out ptra);
                accept(LexicalAnalyzer.withsy);
                СписокПеременныхЗаписей(ptra);
                accept(LexicalAnalyzer.dosy);
                Оператор(ptra);
                if (!Belong(LexicalAnalyzer.symbol, followers))
                {
                    InputOutput.Error(6, LexicalAnalyzer.token); //запрещенный символ
                    SkipTo(followers);
                }
            }
        }
        void СписокПеременныхЗаписей(HashSet<byte> followers)//<список переменных-записей> ::= <переменная-запись> {, <переменная-запись>}
        {
            Переменная(followers);
            while (LexicalAnalyzer.symbol == LexicalAnalyzer.comma)
            {
                LexicalAnalyzer.NextSym();
                Переменная(followers);
            }
        }
    }
}
/*  /\_/\
   ( o.o )
    > ^ <
*/
