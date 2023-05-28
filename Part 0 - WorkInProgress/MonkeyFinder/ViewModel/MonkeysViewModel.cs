using MonkeyFinder.Services;

namespace MonkeyFinder.ViewModel;

public partial class MonkeysViewModel : BaseViewModel
{
    #region Services

    private readonly MonkeyService _monkeyService;
    private readonly IConnectivity _connectivity;
    private readonly IGeolocation _geolocation;

    #endregion

    public MonkeysViewModel(MonkeyService monkeyService,
        IConnectivity connectivity, IGeolocation geolocation)
    {
        _monkeyService = monkeyService;
        _connectivity = connectivity;
        _geolocation = geolocation;
        Title = "Monkey Finder";
    }

    #region Observable Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyCanExecuteChangedFor(nameof(GetClosestMonkeyCommand), nameof(GetMonkeysCommand), nameof(ClearMonkeysListCommand))]
    private bool _isBusy;

    // DO NOT SET THIS PROPERTY => Always set 'IsBusy' instead
    public bool IsNotBusy => !IsBusy;

    // Use for the "Refresh list" gesture. When 'true' shows a spinning circle.
    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCollectionViewEmpty))]
    [NotifyPropertyChangedFor(nameof(IsCollectionViewNotEmpty))]
    [NotifyCanExecuteChangedFor(nameof(GetClosestMonkeyCommand), nameof(GetMonkeysCommand), nameof(ClearMonkeysListCommand))]
    private ObservableCollection<Monkey> _monkeys = new();

    public bool IsCollectionViewEmpty => !Monkeys.Any();

    public bool IsCollectionViewNotEmpty => !IsCollectionViewEmpty;

    #endregion

    #region Command - Get Monkeys
    private bool CanGetMonkeys => (IsNotBusy && IsCollectionViewEmpty);

    [RelayCommand(CanExecute = nameof(CanGetMonkeys))]
    async Task GetMonkeysAsync()
    {
        try
        {
            if (!CanGetMonkeys) return;

            // Busy section - Calling the service
            try
            {
                IsBusy = true;

                // Check Internet Connectivity
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("Internet Issue",
                        "No internet access. Check your connection and try again", "OK");
                    return;
                }

                var monkeysFromSvc = await _monkeyService.GetMonkeysAsync();

                // New collection to trigger the bound properties
                var newMonkeys = new ObservableCollection<Monkey>();
                foreach (var monkey in monkeysFromSvc)
                    newMonkeys.Add(monkey);
                Monkeys = newMonkeys;
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
        finally
        {
            // Reset the refresh
            IsRefreshing = false;
        }

    }

    #endregion

    #region Command - Get Closest Monkey

    private bool CanGetClosestMonkey => (IsNotBusy && IsCollectionViewNotEmpty);

    [RelayCommand(CanExecute = nameof(CanGetClosestMonkey))]
    //[RelayCommand]
    async Task GetClosestMonkeyAsync()
    {
        if (!CanGetClosestMonkey) return;

        try
        {
            IsBusy = true;

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
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Command - Clear Monkeys

    private bool CanClearMonkeysList => (IsNotBusy && IsCollectionViewNotEmpty);

    [RelayCommand(CanExecute = nameof(CanClearMonkeysList))]
    async Task ClearMonkeysListAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            // New collection to trigger the bound properties
            Monkeys = new ObservableCollection<Monkey>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"Unable to clear the list of monkeys: '{ex.Message}'", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Command - Go to Monkey Details page

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

    #endregion
}
