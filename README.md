# WPF + Stylet + Hosting

[.NET Generic Host - .NET | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host?tabs=appbuilder )

[canton7/Stylet: A very lightweight but powerful ViewModel-First MVVM framework for WPF for .NET Framework and .NET Core, inspired by Caliburn.Micro.](https://github.com/canton7/Stylet )

[如何在WPF项目中使用Hosting管理应用的配置、日志、服务等_哔哩哔哩_bilibili](https://www.bilibili.com/video/BV1Sx4y1b7xa)

## WpfAppTemplate1

基础的模版代码

## WpfAppTemplate1.Analyzers

WpfAppTemplate1 的分析器项目，用于自动发现 View/ViewModel 的类，然后自动注册到 IOC 容器中。
因为使用第三方（如微软的）的 IOC 容器之后，stylet 不再自动发现和注册 View/ViewModel，这里通过 Source Generator 来解决这个问题。

reference  
[SamplesInPractice/SourceGeneratorSample at main · WeihanLi/SamplesInPractice](https://github.com/WeihanLi/SamplesInPractice/tree/main/SourceGeneratorSample )  
[使用 Source Generator 在编译你的 .NET 项目时自动生成代码 - walterlv](https://blog.walterlv.com/post/generate-csharp-source-using-roslyn-source-generator )  
[.net - C# Source Generator - warning CS8032: An instance of analyzer cannot be created - Stack Overflow](https://stackoverflow.com/questions/65479888/c-sharp-source-generator-warning-cs8032-an-instance-of-analyzer-cannot-be-cre )  
