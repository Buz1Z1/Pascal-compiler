using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Компилятор
{
    internal class StFoll//StFоll - то, что может быть после ошибки
    {
        Dictionary<byte, HashSet<byte>> Sf = new Dictionary<byte, HashSet<byte>>();
        public Dictionary<byte, HashSet<byte>> sf
        {
            get { return Sf; }
        }

        public const byte
            begpart = 0, 
            st_typepsrt = 1, 
            st_varpart = 2, 
            st_procfuncpart = 3,
            id_starters = 4, 
            after_var = 5, 
            st_type = 6,
            st_record = 7,
            st_final = 8,
            st_begin = 9,
            st_expression = 10,
            after_id = 11,
            after_expression = 12;

        public StFoll()
        {
            HashSet<byte> tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.labelsy);
            tf.Add(LexicalAnalyzer.constsy);
            tf.Add(LexicalAnalyzer.typesy);
            tf.Add(LexicalAnalyzer.varsy);
            tf.Add(LexicalAnalyzer.functionsy);
            tf.Add(LexicalAnalyzer.procedurensy);
            tf.Add(LexicalAnalyzer.beginsy);
            Sf[begpart] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.typesy);
            tf.Add(LexicalAnalyzer.varsy);
            tf.Add(LexicalAnalyzer.functionsy);
            tf.Add(LexicalAnalyzer.procedurensy);
            tf.Add(LexicalAnalyzer.beginsy);
            Sf[st_typepsrt] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.varsy);
            tf.Add(LexicalAnalyzer.functionsy);
            tf.Add(LexicalAnalyzer.procedurensy);
            tf.Add(LexicalAnalyzer.beginsy);
            Sf[st_varpart] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.functionsy);
            tf.Add(LexicalAnalyzer.procedurensy);
            tf.Add(LexicalAnalyzer.beginsy);
            Sf[st_procfuncpart] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.ident);
            Sf[id_starters] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.semicolon);
            Sf[after_var] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.leftpar);//(
            tf.Add(LexicalAnalyzer.onequotmark);//'
            tf.Add(LexicalAnalyzer.intc);//целое число
            //tf.Add(LexicalAnalyzer.floatc);//вещественное число
            tf.Add(LexicalAnalyzer.recordsy);//record
            tf.Add(LexicalAnalyzer.integersy);
            tf.Add(LexicalAnalyzer.realsy);
            tf.Add(LexicalAnalyzer.charsy);
            tf.Add(LexicalAnalyzer.boolsy);
            tf.Add(LexicalAnalyzer.ident);
            Sf[st_type] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.ident);//имя
            tf.Add(LexicalAnalyzer.casesy);//case
            tf.Add(LexicalAnalyzer.endsy);
            Sf[st_record] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.point);
            Sf[st_final] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.ident);//имя
            tf.Add(LexicalAnalyzer.withsy);//with
            tf.Add(LexicalAnalyzer.beginsy);//вложенный begin
            tf.Add(LexicalAnalyzer.endsy);
            Sf[st_begin] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.ident);//имя
            tf.Add(LexicalAnalyzer.intc);//целое число
            tf.Add(LexicalAnalyzer.floatc);//вещественное
            tf.Add(LexicalAnalyzer.plus);//+
            tf.Add(LexicalAnalyzer.minus);//-
            Sf[st_expression] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.assign);
            tf.Add(LexicalAnalyzer.semicolon);
            tf.Add(LexicalAnalyzer.dosy);
            Sf[after_id] = tf;

            tf = new HashSet<byte>();
            tf.Add(LexicalAnalyzer.star);
            tf.Add(LexicalAnalyzer.plus);
            tf.Add(LexicalAnalyzer.minus);
            tf.Add(LexicalAnalyzer.orsy);
            tf.Add(LexicalAnalyzer.slash);
            tf.Add(LexicalAnalyzer.modsy);
            tf.Add(LexicalAnalyzer.divsy);
            tf.Add(LexicalAnalyzer.andsy);
            Sf[after_expression] = tf;
        }
    }
}
