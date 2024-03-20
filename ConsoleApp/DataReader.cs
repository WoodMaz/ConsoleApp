namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Data.Common;

    public class DataReader
    {
        List<ImportedObject> ImportedObjects;

        public void ImportData(string fileToImport)
        {
            ImportedObjects = new List<ImportedObject>();

            var streamReader = new StreamReader(fileToImport);
            streamReader.ReadLine(); //read columns' names, there can be a verification if file is valid

            while (!streamReader.EndOfStream)
            {
                var values = streamReader.ReadLine().Split(';');
                if (values.Length != 7)
                {
                    continue;
                }

                var importedObject = new ImportedObject();

                importedObject.Type = values[0].Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();
                importedObject.Name = values[1].Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.Schema = values[2].Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentName = values[3].Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentType = values[4].Replace(" ", "").Replace(Environment.NewLine, "").ToUpper(); 
                importedObject.DataType = values[5];
                importedObject.IsNullable = values[6];

                ImportedObjects.Add(importedObject);
            }

            // assign number of children
            foreach (var importedObject in ImportedObjects)
            {
                foreach (var potentialChild in ImportedObjects)
                {
                    if (importedObject.IsParentOf(potentialChild))
                    {
                        ++importedObject.NumberOfChildren;
                    }
                }
            }
        }

        public void PrintData()
        {
            foreach (var database in ImportedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                    foreach (var table in FindChildren(ImportedObjects, database))
                    {
                        Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                        foreach (var column in FindChildren(ImportedObjects, table))
                        {
                            Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                        }
                    }
                }
            }
        }

        private List<ImportedObject> FindChildren(List<ImportedObject> objects, ImportedObject parent)
        {
            List<ImportedObject> children = new List<ImportedObject>();

            foreach (var obj in objects)
            {
                if (parent.IsParentOf(obj))
                {
                    children.Add(obj);
                }
            }

            return children;
        }
    }

    class ImportedObject
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Schema { get; set; }

        public string ParentName { get; set; }
        public string ParentType { get; set; }

        public string DataType { get; set; }
        public string IsNullable { get; set; }

        public double NumberOfChildren { get; set; }

        public bool IsParentOf(ImportedObject potentialChild)
        {
            if (potentialChild.ParentType == Type &&
                potentialChild.ParentName == Name)
            {
                return true;
            }

            return false;
        }
    }


}
