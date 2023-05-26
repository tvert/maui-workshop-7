using System.Net.Http.Json;

namespace MonkeyFinder.Services;

public class MonkeyService
{
    private HttpClient _httpClient;

    private List<Monkey> _monkeys = new List<Monkey>();

    public MonkeyService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<Monkey>> GetMonkeysAsync()
    {
        if (_monkeys?.Any() ?? false)
            return _monkeys;

        var url = @"https://montemagno.com/monkeys.json";

        var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            _monkeys = await response.Content.ReadFromJsonAsync<List<Monkey>>();
        }

        return _monkeys;
    }
}
