# Linq2CSV

A very basic library to export any C# object to a csv or binarized format file

## Getting Started

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



## Contributing

This is a very basic funcionality resulting from a side project, separated and published as project for anyone that need it.

All contributions all welcome, please contact or ask for push permissions.


## Authors

* **Sergio Inunza** - *Initial work* - [Linq2Csv](https://github.com/inxunxa/Linq2Csv)


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

