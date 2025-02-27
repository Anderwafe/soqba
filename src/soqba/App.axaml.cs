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
using System.Net;
using Avalonia.Remote.Protocol;

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
            //DisableAvaloniaDataAnnotationValidation();
            HttpClient client = new HttpClient();
            var buf = client.GetStreamAsync("https://github.com/Anderwafe/soqba/raw/refs/heads/main/src/soqba/output.sor");
            try
            {
                buf.Wait();
                input = buf.Result;
            }
            catch
            {
                input = null;
            }

            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            try
            {
                HttpClient client = new HttpClient(new SocketsHttpHandler());
                var response = client.Send(new HttpRequestMessage(HttpMethod.Get, "https://github.com/Anderwafe/soqba/raw/refs/heads/main/src/soqba/output.sor"));
                input = response.Content.ReadAsStream();
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
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

        base.OnFrameworkInitializationCompleted();
    }

    /* private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    } */
}
