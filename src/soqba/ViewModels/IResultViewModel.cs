using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace soqba.ViewModels;

public interface IResultViewModel<T>
{
    bool CanGetResult();

    public Task<T> GetResult();
}
