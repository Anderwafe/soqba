using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using soqba.ViewModels;
using soqba.Views;
using System.Net.Http;
using System.IO;
using Avalonia.Rendering.Composition.Transport;
using System;
using System.Diagnostics;

namespace soqba;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        MainViewModel vm = new MainViewModel();
        Stream? input = null;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            if(File.Exists("/home/anderwafe/Programming/Gitted/Anderwafe/soqba/src/soqba/output.sor"))
            {
                Console.Error.WriteLine("Reading file");
                try
                {
                    input = File.OpenRead("/home/anderwafe/Programming/Gitted/Anderwafe/soqba/src/soqba/output.sor");
                }
                catch
                {
                    Console.Error.WriteLine("Error");
                    input = null;
                }
            }
            else
            {
                Console.Error.WriteLine("Reading github");
                HttpClient client = new HttpClient();
                var buf = client.GetStreamAsync("https://github.com/Anderwafe/soqba/src/soqba/output.sor");
                try
                {
                    buf.Wait();
                    input = buf.Result;
                }
                catch
                {
                    Console.Error.WriteLine("error");
                    input = null;
                }
            }

            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
                Console.Error.WriteLine("Reading github");
            HttpClient client = new HttpClient();
            var buf = client.GetStreamAsync("https://github.com/Anderwafe/soqba/src/soqba/output.sor");
            try
            {
                buf.Wait();
                input = buf.Result;
            }
            catch
            {
                input = null;
            }
            singleViewPlatform.MainView = new MainView
            {
                DataContext = vm
            };
        }

        vm.Initialize(input);

        input?.Dispose();
        input?.Close();
        input = null;

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
