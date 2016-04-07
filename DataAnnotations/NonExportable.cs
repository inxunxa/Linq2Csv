//-----------------------------------------------------------------------
// <copyright file="NonExportable.cs" company="Sergio Inzunza">
//    Sergio Inzunza and Contributors
// </copyright>
//-----------------------------------------------------------------------
// This file is part of Linq2Csv and is dual licensed under MS-PL and Apache 2.0.
// https://github.com/inxunxa/Linq2Csv

namespace Linq2Csv.DataAnnotations
{
    /// <summary>
    /// Specifys a Class or Property that do Not will be exported
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Property)]
    public class NonExportable : System.Attribute
    {
    }
}
