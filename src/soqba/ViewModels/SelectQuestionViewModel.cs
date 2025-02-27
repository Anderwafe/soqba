using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using soqba.ViewModels;

namespace soqba.ViewModels;

public partial class SelectQuestionViewModel : ViewModelBase, IResultViewModel<string>
{
    private int selectedAnswers
    {
        get => Choices.Count(x => x.IsChoosen);
    }

    private string _tip;
    public string Tip {get => _tip;}

    private (int minChoiceCount, int maxChoiceCount) _canChoose;
    public int CanChooseMin {get => _canChoose.minChoiceCount;}
    public int CanChooseMax {get => _canChoose.maxChoiceCount;}

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetResultCommand))]
    private List<ChoiceViewModel> _choices;

    public SelectQuestionViewModel((int minChoiceCount, int maxChoiceCount) canChoose, string tip, string[] choices)
    {
        _tip = tip;

        _canChoose = canChoose;

        this._choices = [..choices.Select(x => new ChoiceViewModel(x))];
        foreach(var choice in _choices)
        {
            choice.PropertyChanged += ChoiceChangedHandler;
        }
    }

    private void ChoiceChangedHandler(object? sender, PropertyChangedEventArgs changed)
    {
        var item = sender as ChoiceViewModel;
        if(item is null) return;

        item.PropertyChanged -= ChoiceChangedHandler;

        if(item.IsChoosen && ((_canChoose.maxChoiceCount - _canChoose.minChoiceCount) == 0) && (selectedAnswers > _canChoose.maxChoiceCount))
            item.IsChoosen = false;
        else OnPropertyChanged(nameof(Choices));

        item.PropertyChanged += ChoiceChangedHandler;
    }

    public bool CanGetResult()
    {
        return selectedAnswers >= _canChoose.minChoiceCount && selectedAnswers <= _canChoose.maxChoiceCount;
    }

    [RelayCommand(CanExecute = nameof(CanGetResult))]
    public Task<string> GetResult()
    {
        return Task.FromResult<string>(string.Join('\n', Choices.Where(x => x.IsChoosen).Select(x => x.Text)));
    }
}
