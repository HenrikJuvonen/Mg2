using System.Collections.Generic;
using System.Linq;

namespace Mg2
{    
    internal enum Operator
    {
        NULL,
        NOT,
        OR,
        AND,
        EQUAL,
        NOTEQUAL,
        GREATERTHAN,
        LESSTHAN,
        GREATERTHANOREQUAL,
        LESSTHANOREQUAL
    }

    internal class OperatorRef
    {
        public Operator Operator;

        public OperatorRef(Operator Operator = Operator.NULL)
        {
            this.Operator = Operator;
        }
    }

    internal interface IOperand
    {
    }

    internal class Expression : IOperand
    {
        public string Value;

        public Expression(string Value)
        {
            this.Value = Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    internal class UnaryPredicate : IOperand
    {
        public Operator Operator;
        public IOperand Operand;

        public UnaryPredicate(Operator Operator = Operator.NULL, IOperand Operand = null)
        {
            this.Operator = Operator;
            this.Operand = Operand;
        }

        public override string ToString()
        {
            return string.Format("({0} {1})", Operator, Operand);
        }
    }

    internal class BinaryPredicate : IOperand
    {
        public Operator Operator;
        public IOperand Operand1;
        public IOperand Operand2;

        public BinaryPredicate(Operator Operator = Operator.NULL, IOperand Operand1 = null, IOperand Operand2 = null)
        {
            this.Operator = Operator;
            this.Operand1 = Operand1;
            this.Operand2 = Operand2;
        }

        public override string ToString()
        {
            return string.Format("({0} {1} {2})", Operand1, Operator, Operand2);
        }
    }

    internal static class QueryParser
    {
        private static Operator ParseOperator(string str)
        {
            switch (str)
            {
                case "not":   return Operator.NOT;
                case "or":    return Operator.OR;
                case "and":   return Operator.AND;
                case "==":    return Operator.EQUAL;
                case "!=":    return Operator.NOTEQUAL;
                case ">":     return Operator.GREATERTHAN;
                case "<":     return Operator.LESSTHAN;
                case ">=":    return Operator.GREATERTHANOREQUAL;
                case "<=":    return Operator.LESSTHANOREQUAL;
            }

            return Operator.NULL;
        }

        private static List<object> FindComponents(string str)
        {
            var components = new List<object>();

            var quote = false;
            var depth = 0;
            var x1 = 0;
            var x2 = 0;

            for (var i = 0; i < str.Count(); i++)
            {
                if (str[i] == '"')
                    quote = !quote;

                if (!quote)
                {
                    if (str[i] == ' ' && depth == 0)
                    {
                        var sub = str.Substring(0, i).Trim('"');
                        var op = ParseOperator(sub);
                        str = str.Substring(i + 1).Trim();
                        i = 0;
                        if (op == Operator.NULL)
                            components.Add(new Expression(sub));
                        else
                            components.Add(new OperatorRef(op));
                    }
                    if (str[i] == '(')
                    {
                        if (depth == 0)
                        {
                            x1 = i+1;
                        }

                        depth++;
                    }
                    if (str[i] == ')')
                    {
                        depth--;

                        if (depth == 0)
                        {
                            x2 = i;
                            var sub = str.Substring(x1, x2 - x1);
                            str = str.Substring(x2 + 1).Trim();
                            i = 0;
                            components.Add(FindComponents(sub));
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(str))
            {
                var op = ParseOperator(str);

                if (op == Operator.NULL)
                    components.Add(new Expression(str.Trim('"')));
                else
                    components.Add(new OperatorRef(op));
            }

            return components;
        }

        private static IOperand GetOperand(object component)
        {
            var list = component as List<object>;

            if (list != null)
            {
                var ops = list.Where(n => n is OperatorRef).ToList();

                var ops4 = list.Where(n =>
                {
                    var op = n as OperatorRef; return op != null && op.Operator > Operator.AND;
                }).ToArray();

                foreach (var op in ops4)
                {
                    var i = list.IndexOf(op);
                    var Operator = ((OperatorRef)list[i]).Operator;
                    var Operand1 = GetOperand(list[i - 1]);
                    var Operand2 = GetOperand(list[i + 1]);
                    list.RemoveRange(i - 1, 3);
                    list.Insert(i - 1, new BinaryPredicate(Operator, Operand1, Operand2));
                }

                var ops5 = list.Where(n =>
                {
                    var op = n as OperatorRef; return op != null && op.Operator == Operator.NOT;
                }).ToArray();

                foreach (var op in ops5)
                {
                    var i = list.IndexOf(op);
                    var Operator = ((OperatorRef)list[i]).Operator;
                    var Operand = GetOperand(list[i + 1]);
                    list.RemoveRange(i, 2);
                    list.Insert(i, new UnaryPredicate(Operator, Operand));
                }

                var ops6 = list.Where(n =>
                {
                    var op = n as OperatorRef; return op != null && op.Operator == Operator.AND;
                }).ToArray();

                foreach (var op in ops6)
                {
                    var i = list.IndexOf(op);
                    var Operator = ((OperatorRef)list[i]).Operator;
                    var Operand1 = GetOperand(list[i - 1]);
                    var Operand2 = GetOperand(list[i + 1]);
                    list.RemoveRange(i - 1, 3);
                    list.Insert(i - 1, new BinaryPredicate(Operator, Operand1, Operand2));
                }

                var ops7 = list.Where(n =>
                {
                    var op = n as OperatorRef; return op != null && op.Operator == Operator.OR;
                }).ToArray();

                foreach (var op in ops7)
                {
                    var i = list.IndexOf(op);
                    var Operator = ((OperatorRef)list[i]).Operator;
                    var Operand1 = GetOperand(list[i - 1]);
                    var Operand2 = GetOperand(list[i + 1]);
                    list.RemoveRange(i - 1, 3);
                    list.Insert(i - 1, new BinaryPredicate(Operator, Operand1, Operand2));
                }

                var result = list.FirstOrDefault();

                if (result is IOperand)
                    return (IOperand)result;
                else
                    return GetOperand(result);
            }

            return component as IOperand;
        }

        internal static IOperand Parse(string query)
        {
            try
            {
                query = query.ToLowerInvariant().Trim();

                var components = FindComponents(query);
                var operand = GetOperand(components);

                //Console.WriteLine(operand);

                return operand;
            }
            catch
            {
            }

            return null;
        }
    }
}