//-----------------------------------------------------------------------
// <copyright file="PrimitiveTypes.cs" company="Sergio Inzunza">
//    Sergio Inzunza and Contributors
// </copyright>
//-----------------------------------------------------------------------
// This file is part of Linq2Csv and is dual licensed under MS-PL and Apache 2.0.
// www.Linq2csv.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
    Special thanks to: Ronnie Overby
    for publishing this list in: http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive
*/

namespace Linq2Csv
{
    public class PrimitiveTypes
    {
        public static readonly Type[] List;

        static PrimitiveTypes()
        {
            var types = new[]
                           {
                              typeof (Enum),
                              typeof (String),
                              typeof (Char),
                              typeof (Guid),

                              typeof (Boolean),
                              typeof (Byte),
                              typeof (Int16),
                              typeof (Int32),
                              typeof (Int64),
                              typeof (Single),
                              typeof (Double),
                              typeof (Decimal),

                              typeof (SByte),
                              typeof (UInt16),
                              typeof (UInt32),
                              typeof (UInt64),

                              typeof (DateTime),
                              typeof (DateTimeOffset),
                              typeof (TimeSpan),
                          };


            var nullTypes = from t in types
                            where t.IsValueType
                            select typeof(Nullable<>).MakeGenericType(t);

            List = types.Concat(nullTypes).ToArray();
        }
        public static bool IsPrimitive(Type type)
        {
            if (List.Any(x => x.IsAssignableFrom(type)))
                return true;

            var nut = Nullable.GetUnderlyingType(type);
            return nut != null && nut.IsEnum;
        }
    }
}
