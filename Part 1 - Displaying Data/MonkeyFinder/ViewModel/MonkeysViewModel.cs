using MonkeyFinder.Services;

namespace MonkeyFinder.ViewModel;

public partial class MonkeysViewModel : BaseViewModel
{
    private readonly MonkeyService _monkeyService;

    public MonkeysViewModel(MonkeyService monkeyService)
    {
        _monkeyService = monkeyService;
        Title = "Monkey Finder";
    }

    public ObservableCollection<Monkey> Monkeys { get; } = new();

    [RelayCommand]
    async Task GetMonkeysAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var monkeysFromSvc = await _monkeyService.GetMonkeysAsync();

            if (Monkeys.Any())
                Monkeys.Clear();

            foreach (var monkey in monkeysFromSvc)
                Monkeys.Add(monkey);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"Unable to get monkeys {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
