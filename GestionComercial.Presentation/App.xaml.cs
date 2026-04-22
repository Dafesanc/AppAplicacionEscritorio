using System.Windows;
using GestionComercial.Infrastructure;
using GestionComercial.Presentation.Services;
using GestionComercial.Presentation.ViewModels;
using GestionComercial.Presentation.ViewModels.Modules;
using GestionComercial.Presentation.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GestionComercial.Presentation;

public partial class App : System.Windows.Application
{
    private IHost _host = null!;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/GestionComercial-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(System.IO.Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddInfrastructure(context.Configuration);

                // Sesión global del usuario autenticado
                services.AddSingleton<SessionService>();

                // ViewModels
                services.AddTransient<LoginViewModel>();
                services.AddTransient<ShellViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<VentasViewModel>();
                services.AddTransient<PesajesViewModel>();
                services.AddTransient<ClientesViewModel>();
                services.AddTransient<InventarioViewModel>();
                services.AddTransient<FacturasViewModel>();
                services.AddTransient<UsuariosViewModel>();
                services.AddTransient<VehiculosViewModel>();

                // Ventanas
                services.AddSingleton<LoginWindow>();
                services.AddSingleton<MainShellWindow>();
            })
            .UseSerilog()
            .Build();

        _host.Start();

        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.StopAsync().GetAwaiter().GetResult();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
