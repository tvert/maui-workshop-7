namespace MonkeyFinder.ViewModel;

[QueryProperty(nameof(MonkeyDetailsViewModel.Monkey), nameof(Monkey))]
public partial class MonkeyDetailsViewModel : BaseViewModel
{
    [ObservableProperty]
    private Monkey _monkey;

    public MonkeyDetailsViewModel()
    {
        
    }
}
