using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using soqba.ViewModels;

namespace soqba.ViewModels;

public partial class InputQuestionViewModel : ViewModelBase, IResultViewModel<string>
{
    private string _tip;
    public string Tip { get => _tip; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetResultCommand))]
    private string _userInput;

    public InputQuestionViewModel(string tip)
    {
        _tip = tip;
        _userInput = "";
    }

    public bool CanGetResult()
    {
        return !string.IsNullOrWhiteSpace(UserInput);
    }

    [RelayCommand(CanExecute = nameof(CanGetResult))]
    public Task<string> GetResult()
    {
        return Task.FromResult(UserInput);
    }
}
