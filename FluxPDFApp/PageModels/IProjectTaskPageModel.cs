using CommunityToolkit.Mvvm.Input;
using FluxPDFApp.Models;

namespace FluxPDFApp.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}