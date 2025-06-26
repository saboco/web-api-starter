namespace web_api;

public class AHttpClient(HttpClient httpClient)
{
    public async Task<string> GetSomething(string url)
    {
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> SaveSomething(string url, Something something)
    {
        var response = await httpClient.PostAsJsonAsync(url, something);
        return await response.Content.ReadAsStringAsync();
    }
}

public record Something(string A, string B)
{
}