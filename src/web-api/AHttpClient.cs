namespace web_api;

public class AHttpClient(HttpClient httpClient)
{
    public async Task<string> GetSomething(string url)
    {
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
}