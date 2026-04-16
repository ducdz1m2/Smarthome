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
        var token = await _tokenHandler.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
