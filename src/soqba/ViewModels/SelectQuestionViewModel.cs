using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using soqba.ViewModels;

namespace soqba.ViewModels;

public partial class SelectQuestionViewModel : ViewModelBase, IResultViewModel<string>
{
    private int selectedAnswers;

    private string _tip;
    public string Tip {get => _tip;}

    private (int minChoiceCount, int maxChoiceCount) _canChoose;

    [ObservableProperty]
    private List<ChoiceViewModel> _choices;

    public SelectQuestionViewModel((int minChoiceCount, int maxChoiceCount) canChoose, string tip, string[] choices)
    {
        _tip = tip;

        selectedAnswers = 0;
        _canChoose = canChoose;

        this._choices = [..choices.Select(x => new ChoiceViewModel(x))];
        foreach(var choice in _choices)
        {
            choice.PropertyChanged += (s,e) => {
                if((_canChoose.maxChoiceCount - _canChoose.minChoiceCount) == 0)
                {
                    if(choice.IsChoosen)
                    {
                        if(selectedAnswers < _canChoose.minChoiceCount) selectedAnswers++;
                        else choice.IsChoosen = false;
                    }
                    else selectedAnswers--;
                }
                else
                    if(choice.IsChoosen)
                        if(selectedAnswers >=  canChoose.maxChoiceCount)
                            choice.IsChoosen = !choice.IsChoosen;
                        else
                            selectedAnswers++;
                    else
                        if(selectedAnswers <= canChoose.minChoiceCount)
                            choice.IsChoosen = !choice.IsChoosen;
                        else
                            selectedAnswers--;
                
                GetResultCommand.NotifyCanExecuteChanged();
            };
        }
    }

    public bool CanGetResult()
    {
        return selectedAnswers >= _canChoose.minChoiceCount && selectedAnswers <= _canChoose.maxChoiceCount;
    }

    [RelayCommand(CanExecute = nameof(CanGetResult))]
    public Task<string> GetResult()
    {
        return Task.FromResult<string>(string.Join('\n', Choices.Where(x => x.IsChoosen)));
    }
}
