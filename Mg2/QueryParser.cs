using System;
using System.Collections.Generic;
using System.Linq;

namespace Mg2
{    
    internal enum Operator
    {
        Null,
        Not,
        Or,
        And,
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    internal class OperatorRef
    {
        public Operator Operator;

        public OperatorRef(Operator Operator = Operator.Null)
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

        public Expression(string value)
        {
            Value = value;
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

        public UnaryPredicate(Operator Operator = Operator.Null, IOperand operand = null)
        {
            this.Operator = Operator;
            Operand = operand;
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

        public BinaryPredicate(Operator Operator = Operator.Null, IOperand operand1 = null, IOperand operand2 = null)
        {
            this.Operator = Operator;
            Operand1 = operand1;
            Operand2 = operand2;
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
                case "not":   return Operator.Not;
                case "or":    return Operator.Or;
                case "and":   return Operator.And;
                case "==":    return Operator.Equal;
                case "!=":    return Operator.NotEqual;
                case ">":     return Operator.GreaterThan;
                case "<":     return Operator.LessThan;
                case ">=":    return Operator.GreaterThanOrEqual;
                case "<=":    return Operator.LessThanOrEqual;
            }

            return Operator.Null;
        }

        private static List<object> FindComponents(string str)
        {
            var components = new List<object>();

            var quote = false;
            var depth = 0;
            var j = 0;

            for (var i = 0; i < str.Count(); i++)
            {
                if (str[i] == '"')
                    quote = !quote;

                if (quote)
                    continue;

                if (str[i] == ' ' && depth == 0)
                {
                    var sub = str.Substring(0, i).Trim('"');
                    var op = ParseOperator(sub);
                    str = str.Substring(i + 1).Trim();
                    i = 0;
                    if (op == Operator.Null)
                        components.Add(new Expression(sub));
                    else
                        components.Add(new OperatorRef(op));
                }
                if (str[i] == '(')
                {
                    if (depth == 0)
                    {
                        j = i+1;
                    }

                    depth++;
                }
                if (str[i] == ')')
                {
                    depth--;

                    if (depth == 0)
                    {
                        var sub = str.Substring(j, i - j);
                        str = str.Substring(i + 1).Trim();
                        i = 0;
                        components.Add(FindComponents(sub));
                    }
                }
            }

            if (!string.IsNullOrEmpty(str))
            {
                var op = ParseOperator(str);

                if (op == Operator.Null)
                    components.Add(new Expression(str.Trim('"')));
                else
                    components.Add(new OperatorRef(op));
            }

            return components;
        }

        private static IOperand GetOperand(object component)
        {
            while (true)
            {
                var list = component as List<object>;

                if (list == null)
                    return component as IOperand;

                //var ops = list.Where(n => n is OperatorRef).ToList();

                var ops4 = list.Where(n =>
                {
                    var op = n as OperatorRef;
                    return op != null && op.Operator > Operator.And;
                }).ToArray();

                foreach (var op in ops4)
                {
                    var i = list.IndexOf(op);
                    var Operator = ((OperatorRef) list[i]).Operator;
                    var operand1 = GetOperand(list[i - 1]);
                    var operand2 = GetOperand(list[i + 1]);
                    list.RemoveRange(i - 1, 3);
                    list.Insert(i - 1, new BinaryPredicate(Operator, operand1, operand2));
                }

                var ops5 = list.Where(n =>
                {
                    var op = n as OperatorRef;
                    return op != null && op.Operator == Operator.Not;
                }).ToArray();

                foreach (var op in ops5)
                {
                    var i = list.IndexOf(op);
                    var Operator = ((OperatorRef) list[i]).Operator;
                    var operand = GetOperand(list[i + 1]);
                    list.RemoveRange(i, 2);
                    list.Insert(i, new UnaryPredicate(Operator, operand));
                }

                var ops6 = list.Where(n =>
                {
                    var op = n as OperatorRef;
                    return op != null && op.Operator == Operator.And;
                }).ToArray();

                foreach (var op in ops6)
                {
                    var i = list.IndexOf(op);
                    var Operator = ((OperatorRef) list[i]).Operator;
                    var operand1 = GetOperand(list[i - 1]);
                    var operand2 = GetOperand(list[i + 1]);
                    list.RemoveRange(i - 1, 3);
                    list.Insert(i - 1, new BinaryPredicate(Operator, operand1, operand2));
                }

                var ops7 = list.Where(n =>
                {
                    var op = n as OperatorRef;
                    return op != null && op.Operator == Operator.Or;
                }).ToArray();

                foreach (var op in ops7)
                {
                    var i = list.IndexOf(op);
                    var Operator = ((OperatorRef) list[i]).Operator;
                    var operand1 = GetOperand(list[i - 1]);
                    var operand2 = GetOperand(list[i + 1]);
                    list.RemoveRange(i - 1, 3);
                    list.Insert(i - 1, new BinaryPredicate(Operator, operand1, operand2));
                }

                var result = list.FirstOrDefault();

                var resultOperand = result as IOperand;
                if (resultOperand != null)
                    return resultOperand;

                component = result;
            }
        }

        internal static IOperand Parse(string query)
        {
            try
            {
                query = query.ToLowerInvariant().Trim();

                var components = FindComponents(query);
                var operand = GetOperand(components);

                return operand;
            }
            catch
            {
                Console.WriteLine("Could not parse query: {0}", query);
            }

            return null;
        }
    }
}