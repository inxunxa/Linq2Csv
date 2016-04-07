//-----------------------------------------------------------------------
// <copyright file="Exportable.cs" company="Sergio Inzunza">
//    Sergio Inzunza and Contributors
// </copyright>
//-----------------------------------------------------------------------
// This file is part of Linq2Csv and is dual licensed under MS-PL and Apache 2.0.
// www.Linq2csv.com

namespace Linq2Csv.DataAnnotations
{
    /// <summary>
    /// Specify that the Property will be Exported.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class Exportable : System.Attribute
    {
        /// <summary>
        /// Gets or Sets Name to display (associate) with the corresponding value
        /// <example>If exporting a <c>CSV</c> the name represent the Column Header</example>
        /// </summary>
        public string Name { get; set; }
            
        /// <summary>
        /// Gets or Sets The order in which the Property will be readed/exported with respect to other properties in the same Class
        /// If more than one property has the same <para>Order</para> value, the first encountered will be considered first
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Empty contructor.
        /// Initializes the Name to <c>string.Empty</c> and order to <c>int.MaxValue</c>
        /// </summary>
        public Exportable()
        {
            this.Name = string.Empty;
            this.Order = int.MaxValue;
        }
    }
}
