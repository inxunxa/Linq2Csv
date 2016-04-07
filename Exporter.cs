//-----------------------------------------------------------------------
// <copyright file="exporter.cs" company="Sergio Inzunza">
//    Sergio Inzunza and Contributors
// </copyright>
//-----------------------------------------------------------------------
// This file is part of Linq2Csv and is dual licensed under MS-PL and Apache 2.0.
// www.Linq2csv.com

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
        private string _separator = ",";
        private List<string> _header = new List<string>();
        private List<List<object>> _csvRows = new List<List<object>>();


        private int _rowsPerObject = 1;
        private bool _headerWriten;

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
                        if (i > 0) NextLineAutoMap(true);
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
        internal void Map(Object entity)
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
                        // element in elements will have the same GUID                    
                        if (i > 0) NextLineAutoMap(true); // add new line only after the first element                        
                        Map(elems[i]);
                    }
                }
                else
                {
                    if (PrimitiveTypes.IsPrimitive(property.PropertyType))
                    {
                        var attribute = property.GetCustomAttribute<Exportable>();
                        var name = attribute != null
                            ? attribute.Name
                            : property.DeclaringType?.Name + "." + property.Name;

                        // Bottom node reached, consider value
                        AddRecord(name, propValue);
                    }
                    else
                    {
                        // Brach node found, iterate through childs
                        Map(propValue);
                    }
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
            _separator = ",";
            _writer = new StreamWriter(filePath);

            // verify if the object is a collection
            // if so, iterate trough elements
            var objs = entity as IList;
            if (objs != null)
            {
                foreach (var obj in objs)
                {
                 //   if (obj.GetType().GetCustomAttribute<ExportableClass>() != null)
                 //   {
                        Map(obj);
                        FlushObject();
                 //   }
                }
            }
            else
            {
                Map(entity);
            }

            Flush();
        }

        /// <summary>
        /// Generate a CSV files whit the data of the object
        /// </summary>
        /// <param name="entity">The object to export</param>
        /// <param name="filePath">Full file name where for the Csv File</param>
        public void GenerateCsvAutoMap(Object entity, string filePath)
        {
            _separator = ",";
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
        private void AddRecord(string columnName, object value)
        {
            int index = _header.IndexOf(columnName);
            if (index < 0)
            {
                // new record create a column for it
                _header.Add(columnName);
                index = _header.Count -1;
            }

            if(!_csvRows.Any()) _csvRows.Add(new List<object>());

            for (int i = 1; i <= _rowsPerObject; i++)
            {
                int row = _csvRows.Count - i;
                while (_csvRows[row].Count <= index)
                    _csvRows[row].Add("");
                _csvRows[row][index] = value;
            }            
        }

        private void NextLineAutoMap(bool copyPrevious = false)
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
                _csvRows.Add(new List<object>());
                _rowsPerObject++;
            }            
        }


        private void WriteHeader()
        {
            if (!_headerWriten)
            {
                _writer.WriteLine(string.Join(_separator, _header.ToArray()));
                _headerWriten = true;
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
            _rowsPerObject = 1;
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
            _header.Clear();
            _csvRows.Clear();
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
