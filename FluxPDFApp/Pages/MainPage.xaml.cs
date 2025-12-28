using FluxPDFApp.Models;
using FluxPDFApp.PageModels;

namespace FluxPDFApp.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}