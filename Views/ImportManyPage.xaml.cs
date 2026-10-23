using Popote.ViewModels;

namespace Popote.Views;

public partial class ImportManyPage : ContentPage
{
    public ImportManyPage(ImportManyViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
