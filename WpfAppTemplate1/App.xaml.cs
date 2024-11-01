using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WpfAppTemplate1;

public partial class App : Application
{
    private static IServiceProvider? _serviceProvider;

    public static IServiceProvider ServiceProvider
    {
        get => _serviceProvider!;
        private set => _serviceProvider = value;
    }

    [STAThread]
    static void Main(string[] args)
    {
        using IHost host = CreateHostBuilder(args).Build();
        ServiceProvider = host.Services;
        host.StartAsync();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }

    public static T? GetService<T>()
        where T : class
    {
        return ServiceProvider.GetService<T>();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton(_ => Current.Dispatcher);
            });

        return builder;
    }
}
