//-----------------------------------------------------------------------
// <copyright file="Exporter.cs" company="Sergio Inzunza">
//    Sergio Inzunza and Contributors
// </copyright>
//-----------------------------------------------------------------------
// This file is part of Linq2Csv and is dual licensed under MS-PL and Apache 2.0.
// https://github.com/inxunxa/Linq2Csv

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Linq2Csv.DataAnnotations;

namespace Linq2Csv
{
    /// <summary>
    /// Class to Export a Linq Entity to a File
    /// </summary>
    public class Exporter : IDisposable
    {
        // class needed fields
        private StreamWriter _writer;
        
        private List<string> _csvHeader = new List<string>();
        private List<string> _binaryHeader = new List<string>();
        private List<List<object>> _csvRows = new List<List<object>>();
        private List<List<string>> _binaryRows = new List<List<string>>();

        private int _rowsPerObject;
        private bool _csvHeaderWriten;
        private bool _binaryHeaderWriten;


        /// <summary>
        /// The string that will be used to separete the columns
        /// </summary>
        public string Separator { get; set; } =  ",";

        /// <summary>
        /// How enumerable properties will be converted.
        /// <value>True</value> will create column(s) for the elements in the Enumerable property
        /// <value>False</value> will create a new row for each element in the Enumerable property
        /// </summary>
        public bool TreatEnumerablesAsColumns { get; set; } = true;

        /// <summary>
        /// Map an object (without anotations) to Rows
        /// </summary>
        /// <param name="entity">The entity to export</param>
        internal void AutoMap(object entity)
        {
            if (entity == null) return;
            Type objType = entity.GetType();
            
            PropertyInfo[] properties = objType.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object propValue = property.GetValue(entity, null);
                var elems = propValue as IList;
                if (elems != null)
                {                    
                    for (int i = 0; i < elems.Count; i++)
                    {
                        // element in elements will have the same GUID            
                        if (i > 0) NextLine(true);
                        AutoMap(elems[i]);
                    }                    
                }
                else
                {
                    if (PrimitiveTypes.IsPrimitive(property.PropertyType))
                    {
                        // Bottom node reached, consider value
                        AddRecord(property.DeclaringType?.Name + "." + property.Name, propValue);
                    }
                    else
                    {
                        // Brach node found, iterate through childs
                        AutoMap(propValue);
                    }
                }
            }
        }

        /// <summary>
        /// Maps and object to rows using the DataAnnotations
        /// </summary>
        /// <param name="entity">The objecto to be mapped</param>
        /// <param name="enumerableColumnId">The string to append to repeated columns (by enumerables) applicable only when <c>TreatEnumerablesAsColumns</c> is True</param>
        internal void Map(Object entity, string enumerableColumnId = "")
        {
            if (entity == null || entity.GetType().GetCustomAttribute<NonExportable>() != null) return;
            Type objType = entity.GetType();
            
            // Sort properties by the order property of the custon annotation, send nulls (the one that aren't decorated with the attribute) at bottom
            var properties = objType.GetProperties().Where(x => x.GetCustomAttribute<NonExportable>() == null).OrderBy(x => x.GetCustomAttribute<Exportable>() == null).ThenBy(x => x.GetCustomAttribute<Exportable>()?.Order);
            foreach (PropertyInfo property in properties)
            {
                object propValue = property.GetValue(entity, null);
                var elems = propValue as IList;
                if (elems != null)
                {
                    for (int i = 0; i < elems.Count; i++)
                    {
                        if (TreatEnumerablesAsColumns)
                        {
                            Map(elems[i], i.ToString());
                        }
                        else
                        {
                            if (i > 0) NextLine(true); // add new line only after the first element                        
                            Map(elems[i]);
                        }                              
                    }
                }
                else
                {
                    if (PrimitiveTypes.IsPrimitive(property.PropertyType))
                    {
                        var attribute = property.GetCustomAttribute<Exportable>();
                        var name = attribute?.Name ?? property.DeclaringType?.Name + "." + property.Name;
                        name += enumerableColumnId;
                        var globalPosition = attribute?.GlobalOrder ?? -1;

                        // Bottom node reached, consider value
                        AddRecord(name, propValue, globalPosition);
                    }
                    else
                    {
                        // Brach node found, iterate through childs
                        Map(propValue, enumerableColumnId);
                    }
                }
            }
        }

        private void BinarizeRows(int firstColumnsToSkip)
        {
            foreach (var csvRow in _csvRows)
            {
                NextBinaryLine();
                for (int j = 0; j < csvRow.Count; j++)
                {
                    AddBinaryRecord(_csvHeader[j], csvRow[j], !(j < firstColumnsToSkip));
                }
            }
        }

        /// <summary>
        /// Generates a Csv file with using the DataAnnotation as a guide
        /// </summary>
        /// <param name="entity">The object or List (IList implementation) of objects to writte</param>
        /// <param name="filePath">The full file name to save the resulting csv file</param>
        public void GenerateCsv(Object entity, string filePath)
        {
            Separator = ",";
            _writer = new StreamWriter(filePath);

            // verify if the object is a collection
            var objs = entity as IList;
            if (objs != null)
            {
                foreach (var obj in objs)
                {
                    Map(obj);
                    // flush each object to same a little memory
                    FlushObject();                                        
                }
            }
            else
            {
                Map(entity);
            }

            Flush();
        }

        /// <summary>
        /// Generates a binary dataset file with using the DataAnnotation as a guide. See: 
        /// <seealso cref="http://github.com/inxunxa/Linq2Csv"/> for more details
        /// </summary>
        /// <param name="entity">The object or List (IList implementation) of objects to writte</param>
        /// <param name="filePath">The full file name to save the resulting csv file</param>
        /// <param name="firstColumsToSkip">How many colums should be skip (of binarization)</param>
        public void GenerateBinaryFormat(Object entity, string filePath, int firstColumsToSkip = 0)
        {
            Separator = ",";            
            // verify if the object is a collection
            var objs = entity as IList;
            if (objs != null)
            {
                foreach (var obj in objs)
                {
                    Map(obj);
                    // unlike GenerateCsv, here we can't flush each object as we need them in memory
                    // just reset _rowsPerObject, and create a new line (in that order)
                    _rowsPerObject = 0;
                    NextLine();                    
                }
            }
            else
            {
                Map(entity);
            }

            BinarizeRows(firstColumsToSkip);
            _writer = new StreamWriter(filePath);
            FlushBinary();

        }

        /// <summary>
        /// Generate a CSV files whit the data of the object
        /// </summary>
        /// <param name="entity">The object to export</param>
        /// <param name="filePath">Full file name where for the Csv File</param>
        public void GenerateCsvAutoMap(Object entity, string filePath)
        {
            Separator = ",";
            _writer = new StreamWriter(filePath);

            // verify if the object is a collection
            // if so, iterate trough elements
            var objs = entity as IList;
            if (objs != null)
            {
                foreach (var obj in objs)
                {
                    AutoMap(obj);
                    FlushObject();
                }
            }
            else
            {
                AutoMap(entity);
            }

            Flush();
        }

        /// <summary>
        /// Write the Entity to the File
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void WriteEntity<T>(T entity)
        {
            if (entity == null) return;

            AutoMap(entity);
        }

        /// <summary>
        /// Add as a record to the records matrix.
        /// </summary>
        /// <param name="columnName">The corresponding column for the record</param>
        /// <param name="value">The value of the record</param>
        /// <param name="globalPosition">The position of the column respect the overall file</param>
        private void AddRecord(string columnName, object value, int globalPosition = -1)
        {
            if (!_csvRows.Any()) NextLine();

            // get the index of the corresponding column in the header.
            int index = _csvHeader.IndexOf(columnName);
            if (index < 0)
            {
                if (globalPosition >= 0) // put the column in specific index.
                {
                    if (globalPosition >= _csvHeader.Count) // empty cell are needed to create the globalPosition index. 
                    {
                        int addRange = globalPosition - _csvHeader.Count; // how many empty cells
                        for (int i = 0; i < addRange; i++)
                        {
                            _csvHeader.Add(string.Empty);
                            _csvRows.ForEach(x => x.Add(string.Empty));
                        }
                    }

                    _csvHeader.Insert(globalPosition, columnName);
                    _csvRows.ForEach(x => x.Insert(globalPosition, string.Empty));
                    index = globalPosition;
                }
                else // put the column in the first index available (or create one).
                {
                    // TODO: Get first empty string (if any) or add it to the end
                    _csvHeader.Add(columnName);
                    _csvRows.ForEach(x => x.Add(string.Empty));
                    index = _csvHeader.Count - 1;
                }
            }

            // put the value in the current row
            _csvRows.Last()[index] = value;

            // check if the above cell of the same object are empty
            // if so, copy the same value on all of them
            for (int i = 1; i < _rowsPerObject; i++) // the i start at 1, as the 0 position is forced in (_csvRows.Last()[index] = value;)
            {
                int row = (_csvRows.Count - 1) - i; // last row (_csvRows.Count - 1) minus i
                while (_csvRows[row].Count <= index) // create the index if needed
                    _csvRows[row].Add("");
                if(_csvRows[row][index] is string && string.IsNullOrEmpty(_csvRows[row][index].ToString()))   _csvRows[row][index] = value;
            }
        }

        private void AddBinaryRecord(string columnName, object value, bool isBinarized = true)
        {
            string strVal = value?.ToString() ?? "na";
            if (isBinarized)
            {
                columnName += ":" + strVal;
                strVal = "1";
            }


            int index = _binaryHeader.IndexOf(columnName);
            if (index < 0)
            {
                // new header
                _binaryHeader.Add(columnName);
                _binaryRows.ForEach(x => x.Add("0"));
                index = _binaryHeader.Count - 1;
            }

            _binaryRows.Last()[index] = strVal;
        }


        private void NextLine(bool copyPrevious = false)
        {
            /* When creating a csv, if one objects has a property (list) with two elements (for example)
               then two rows should result in the csv, coping all the base information, 
               the only diference should be the elements of the array.
            */
            if (copyPrevious)
            {
                if (_csvRows.Count > 0 && _csvRows.Last().Count > 0)
                {
                    // copy pnly if above line isn't empty
                    _csvRows.Add(new List<object>(_csvRows.Last().ToArray()));
                    _rowsPerObject++;
                }
            }
            else
            {
                _csvRows.Add(new List<object>(Enumerable.Repeat(string.Empty, _csvHeader.Count)));
                _rowsPerObject++;
            }            
        }

        private void NextBinaryLine()
        {           
            // add a new row to the list filled with ceros
            _binaryRows.Add(Enumerable.Repeat("0", _binaryHeader.Count).ToList());
        }
    
        private void WriteHeader()
        {
            if (!_csvHeaderWriten)
            {
                _writer.WriteLine(string.Join(Separator, _csvHeader.ToArray()));
                _csvHeaderWriten = true;
            }
        }

        private void WriteBinaryHeader()
        {
            if (!_binaryHeaderWriten)
            {
                _writer.WriteLine(string.Join(Separator, _binaryHeader.ToArray()));
                _binaryHeaderWriten = true;
            }
        }

        private void FlushObject()
        {
            WriteHeader();
            foreach (List<object> row in _csvRows)
            {
                _writer.WriteLine(string.Join(",", row.ToArray()));
            }
            _csvRows.Clear();
            _rowsPerObject = 0;
        }

        /// <summary>
        /// Flush (writes) the data to the file. And close the stream
        /// </summary>
        public void Flush()
        {
            WriteHeader();
            foreach (var row in _csvRows)
            {
                _writer.WriteLine(string.Join(",", row.ToArray()));
            }

            _writer.Close();
            _writer.Dispose();
            _csvHeader.Clear();
            _csvRows.Clear();
            _csvHeaderWriten = false;
        }

        private void FlushBinary()
        {
            WriteBinaryHeader();
            foreach (var row in _binaryRows)
            {
                _writer.WriteLine(string.Join(",", row.ToArray()));
            }

            _writer.Close();
            _writer.Dispose();
            _csvHeader.Clear();
            _binaryHeader.Clear();
            _csvRows.Clear();
            _binaryRows.Clear();
            _binaryHeaderWriten = false;
        }

        /// <summary>
        /// Dispose the unused object
        /// </summary>
        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
