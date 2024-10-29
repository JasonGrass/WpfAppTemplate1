using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Stylet;
using StyletIoC;
using WpfAppTemplate1.View;
using WpfAppTemplate1.ViewModel;

namespace WpfAppTemplate1;

public class Bootstrapper : MicrosoftDependencyInjectionBootstrapper<RootViewModel>
{
    protected override void OnStart()
    {
        // This is called just after the application is started, but before the IoC container is set up.
        // Set up things like logging, etc
    }

    protected override void Configure()
    {
        // This is called after Stylet has created the IoC container, so this.Container exists, but before the
        // Root ViewModel is launched.
        // Configure your services, etc, in here
    }

    protected override void ConfigureIoC(IServiceCollection services)
    {
        base.ConfigureIoC(services);
        services.AddSingleton<RootViewModel>();
        services.AddSingleton<RootView>();
    }

    protected override void OnLaunch()
    {
        // This is called just after the root ViewModel has been launched
        // Something like a version check that displays a dialog might be launched from here
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Called on Application.Exit
    }

    protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
    {
        // Called on Application.DispatcherUnhandledException
    }
}

public class MicrosoftDependencyInjectionBootstrapper<TRootViewModel> : BootstrapperBase
    where TRootViewModel : class
{
    private ServiceProvider? _serviceProvider;
    private TRootViewModel? _rootViewModel;

    protected virtual TRootViewModel RootViewModel =>
        this._rootViewModel ??= (TRootViewModel)this.GetInstance(typeof(TRootViewModel));

    public IServiceProvider ServiceProvider => this._serviceProvider!;

    protected override void ConfigureBootstrapper()
    {
        var services = new ServiceCollection();
        this.DefaultConfigureIoC(services);
        this.ConfigureIoC(services);
        this._serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Carries out default configuration of the IoC container. Override if you don't want to do this
    /// </summary>
    protected virtual void DefaultConfigureIoC(IServiceCollection services)
    {
        var viewManagerConfig = new ViewManagerConfig()
        {
            ViewFactory = this.GetInstance,
            ViewAssemblies = new List<Assembly>() { this.GetType().Assembly },
        };

        services.AddSingleton<IViewManager>(new ViewManager(viewManagerConfig));
        services.AddTransient<MessageBoxView>();

        services.AddSingleton<IWindowManagerConfig>(this);
        services.AddSingleton<IWindowManager, WindowManager>();
        services.AddSingleton<IEventAggregator, EventAggregator>();

        services.AddTransient<IMessageBoxViewModel, MessageBoxViewModel>(); // Not singleton!
        // Also need a factory
        services.AddSingleton<Func<IMessageBoxViewModel>>(() => new MessageBoxViewModel());
    }

    /// <summary>
    /// Override to add your own types to the IoC container.
    /// </summary>
    protected virtual void ConfigureIoC(IServiceCollection services) { }

    public override object GetInstance(Type type)
    {
        return this.ServiceProvider.GetRequiredService(type);
    }

    protected override void Launch()
    {
        base.DisplayRootView(this.RootViewModel);
    }

    public override void Dispose()
    {
        base.Dispose();

        ScreenExtensions.TryDispose(this._rootViewModel);
        if (this._serviceProvider != null)
        {
            this._serviceProvider.Dispose();
        }
    }
}
