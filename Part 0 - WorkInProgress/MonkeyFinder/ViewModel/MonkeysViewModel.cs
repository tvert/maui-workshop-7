using MonkeyFinder.Services;

namespace MonkeyFinder.ViewModel;

public partial class MonkeysViewModel : BaseViewModel
{
    private readonly MonkeyService _monkeyService;

    private readonly IConnectivity _connectivity;
    private readonly IGeolocation _geolocation;

    public MonkeysViewModel(MonkeyService monkeyService,
        IConnectivity connectivity, IGeolocation geolocation)
    {
        _monkeyService = monkeyService;
        _connectivity = connectivity;
        _geolocation = geolocation;
        Title = "Monkey Finder";
    }

    public ObservableCollection<Monkey> Monkeys { get; } = new();

    //[RelayCommand(CanExecute = nameof(CanGetClosestMonkey))]
    [RelayCommand]
    async Task GetClosestMonkeyAsync()
    {
        if (!CanGetClosestMonkey) return;

        try
        {
            var location = await _geolocation.GetLastKnownLocationAsync();
            if (location is null)
            {
                location = await _geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(15)
                });

                if (location is null)
                {
                    await Shell.Current.DisplayAlert("Location Issue",
                        "Unable to get closest monkey because unable to get your current location.", "OK");
                    return;
                }
            }

            var closest = Monkeys.OrderBy(m => 
                    location.CalculateDistance(m.Latitude, m.Longitude, DistanceUnits.Miles)).First();

            await Shell.Current.DisplayAlert("Closest Monkey",
                $"{closest.Name} in {closest.Location}", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"Unable to get closest monkey: '{ex.Message}'", "OK");
        }
    }

    private bool CanGetClosestMonkey => (IsNotBusy && Monkeys.Any());

    [RelayCommand]
    async Task GoToDetailsAsync(Monkey monkey)
    {
        if (monkey is null) return;

        await Shell.Current.GoToAsync($"{nameof(DetailsPage)}", true,
            new Dictionary<string, object>
            {
                { nameof(Monkey), monkey}
            });
    }

    [RelayCommand]
    async Task GetMonkeysAsync()
    {
        if (IsBusy) return;

        try
        {
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("Internet Issue",
                    "No internet access. Check your connection and try again", "OK");
                return;
            }

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
            await Shell.Current.DisplayAlert("Error!", $"Unable to get monkeys: '{ex.Message}'", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
