using CommunityToolkit.Mvvm.Input;
using DocScanner.Models;

namespace DocScanner.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}