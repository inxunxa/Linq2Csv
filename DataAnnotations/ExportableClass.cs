//-----------------------------------------------------------------------
// <copyright file="ExportableClass.cs" company="Sergio Inzunza">
//    Sergio Inzunza and Contributors
// </copyright>
//-----------------------------------------------------------------------
// This file is part of Linq2Csv and is dual licensed under MS-PL and Apache 2.0.
// www.Linq2csv.com

namespace Linq2Csv.DataAnnotations
{
    /// <summary>
    /// Specifys a Class that will be exported.
    /// All Properties of an <c>Exportable</c> class are exported
    /// Except for the specifically ommited with <see cref="NonExportable"/>
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ExportableClass : System.Attribute
    {        
    }
}
