# Linq2CSV

A very basic library to export any C# object to a csv or binarized format file.

The classes whose objects will be exported should be annotated with the included DataAnnotation tags.

## Getting Started

Annotate the classes that will be exported
```c#
using Linq2Csv.DataAnnotations;

public class User
{
	[Exportable(GlobalOrder = 0, Order = 0, Name = "userID")]    
    public int Id { get; set; }

    [Exportable]
    public virtual Demographic Demographic { get; set; }
  
  	[NonExportable
  	private string SomeSecreteValue {get; set;}
     
}
```


A usage example of the library.

```c#
public void ExportSomeObject<T>(string filePath, ICollection<T> data){
	Linq2Csv.Exporter.ShowDebugInfo = true; // for debug environment only
    using (var exporter = new Linq2Csv.Exporter())
    {
    	exporter.TreatEnumerablesAsColumns = false; // choose the best fit for your needs
        exporter.GenerateCsv(data, filePath);
    }
}
```


### Installing

Just include de Linq2Csv.dll to your project references, and add a reference to such dll

```csharp
using Linq2Csv;
```


### Configuration options
-----------------------------------------
| Property      | Default  | Description           |
| ------------- |----------|-------------|
| ShowDebugInfo            | False | Shown in console what's happening in the process|
| Separator                | , | Separator to use in file, this allow creation of other that csv file formats      |
| NullValue                | <empty> | What to put when a null valued property is found      |
| TreatEnumerableAsColumns | False | Each enumerable item will be a Column or a new Row     |




## Contributing

This is a very basic funcionality resulting from a side project, separated and published as project for anyone that need it.

All contributions all welcome, please contact or ask for push permissions.


## Authors

* **Sergio Inunza** - *Initial work* - [Linq2Csv](https://github.com/inxunxa/Linq2Csv)


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

