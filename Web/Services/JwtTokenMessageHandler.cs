using System.Net.Http.Headers;

namespace Web.Services;

public class JwtTokenMessageHandler : DelegatingHandler
{
    private readonly JwtTokenHandler _tokenHandler;

    public JwtTokenMessageHandler(JwtTokenHandler tokenHandler)
    {
        _tokenHandler = tokenHandler;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _tokenHandler.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // Ignore errors when getting token (JS runtime not ready, etc.)
            // Continue without token - will get 401 if auth is required
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
