using System;
using System.Linq;

namespace Mg2.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception Unwrap(this Exception exception)
        {
            var aggregate = exception as AggregateException;

            if (aggregate != null)
            {
                var exceptions = (from e in aggregate.Flatten().InnerExceptions select e).ToArray();

                switch (exceptions.Length)
                {
                    case 1:
                        return exceptions.FirstOrDefault();
                    case 2:
                        return new AggregateException(exceptions);
                }
            }

            if (exception.InnerException != null)
            {
                var inner = exception.InnerException.Unwrap();

                return new AggregateException(exception, inner);
            }

            return exception;
        }

        public static AggregateException ToAggregate(this Exception exception)
        {
            var aggregate = exception as AggregateException;
            return aggregate ?? new AggregateException(exception);
        }
    }
}