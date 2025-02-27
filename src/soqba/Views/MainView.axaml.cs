using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using soqba.ViewModels;

namespace soqba.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetClipboardDel();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SetClipboardDel();
    }

    private void SetClipboardDel()
    {
        if(DataContext is not null)
        {
            if(DataContext is MainViewModel mvm)
            {
                mvm.CopyToClipboardRequest = null;
                var topLevel = TopLevel.GetTopLevel(this);
                if(topLevel is not null)
                {
                    var clipboardApi = topLevel.Clipboard;
                    if(clipboardApi is not null)
                    {
                        mvm.CopyToClipboardRequest = clipboardApi.SetTextAsync;
                    }
                }
            }
        }
    }
}
