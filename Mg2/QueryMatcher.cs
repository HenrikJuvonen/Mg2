using System;
using System.Linq;
using Mg2.Models;

namespace Mg2
{
    internal static class QueryMatcher
    {
        private static bool MatchExpression(PackageItem package, Expression expression)
        {
            switch (expression.Value)
            {
                case "installed": return package.IsInstalled;
                case "updatable": return package.PackageIdentity.Flags.Count(n => n == "updatable") != 0;
                case "latest": return package.IsLatest;
            }

            return package.Name.ToLowerInvariant().Contains(expression.Value.ToLowerInvariant());
        }

        private static Version AsVersion(string str)
        {
            var split = str.Split('.');
            var nums = new int[4];

            for (var i = 0; i < split.Length; i++)
                int.TryParse(split[i], out nums[i]);
            
            return new Version(nums[0], nums[1], nums[2], nums[3]);
        }

        private static bool MatchVersion(PackageItem package, BinaryPredicate predicate)
        {
            var expr2 = predicate.Operand2 as Expression;

            if (expr2 != null)
            {
                try
                {
                    switch (predicate.Operator)
                    {
                        case Operator.EQUAL: return AsVersion(package.PackageIdentity.Version) == AsVersion(expr2.Value);
                        case Operator.NOTEQUAL: return AsVersion(package.PackageIdentity.Version) != AsVersion(expr2.Value);
                        case Operator.GREATERTHAN: return AsVersion(package.PackageIdentity.Version) > AsVersion(expr2.Value);
                        case Operator.LESSTHAN: return AsVersion(package.PackageIdentity.Version) < AsVersion(expr2.Value);
                        case Operator.GREATERTHANOREQUAL: return AsVersion(package.PackageIdentity.Version) >= AsVersion(expr2.Value);
                        case Operator.LESSTHANOREQUAL: return AsVersion(package.PackageIdentity.Version) <= AsVersion(expr2.Value);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return true;
        }
        
        private static bool MatchPredicate(PackageItem package, BinaryPredicate predicate)
        {
            var expr1 = predicate.Operand1 as Expression;

            switch (predicate.Operator)
            {
                case Operator.OR:
                    return Match(package, predicate.Operand1) || Match(package, predicate.Operand2);
                case Operator.AND:
                    return Match(package, predicate.Operand1) && Match(package, predicate.Operand2);
                case Operator.EQUAL:
                case Operator.NOTEQUAL:
                case Operator.GREATERTHAN:
                case Operator.LESSTHAN:
                case Operator.GREATERTHANOREQUAL:
                case Operator.LESSTHANOREQUAL:
                    if (expr1 != null)
                    {
                        switch (expr1.Value)
                        {
                            case "version": return MatchVersion(package, predicate);
                        }
                    }
                    break;
            }

            return true;
        }

        private static bool MatchPredicate(PackageItem package, UnaryPredicate predicate)
        {
            switch (predicate.Operator)
            {
                case Operator.NOT:
                    return !Match(package, predicate.Operand);
            }

            return true;
        }

        private static bool MatchPredicate(PackageItem package, IOperand predicate)
        {
            var unaryPredicate = predicate as UnaryPredicate;
            if (unaryPredicate != null)
                return MatchPredicate(package, unaryPredicate);

            var binaryPredicate = predicate as BinaryPredicate;
            if (binaryPredicate != null)
                return MatchPredicate(package, binaryPredicate);

            return true;
        }

        internal static bool Match(PackageItem package, IOperand operand)
        {
            var expression = operand as Expression;

            if (expression != null)
                return MatchExpression(package, expression);
            return MatchPredicate(package, operand);
        }
    }
}
