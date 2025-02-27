using System;

namespace soqba.ViewModels;

public partial class TextQuestionViewModel : ViewModelBase
{
    private string _text;

    public string Text {get => _text;}

    public TextQuestionViewModel(string text)
    {
        _text = text;
    }
}
