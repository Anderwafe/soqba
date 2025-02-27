using System;
using CommunityToolkit.Mvvm.ComponentModel;
using soqba.ViewModels;

namespace soqba.ViewModels;

public partial class ChoiceViewModel : ViewModelBase
{
    private string _text;
    public string Text {get => _text;}

    [ObservableProperty]
    private bool _isChoosen;

    public ChoiceViewModel(string text) : this(text, false) {}

    public ChoiceViewModel(string text, bool initValue)
    {
        _text = text;
        _isChoosen = initValue;
    }
}
