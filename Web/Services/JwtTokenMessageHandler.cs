using System.Net.Http.Headers;

namespace Web.Services;

public class JwtTokenMessageHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;

    public JwtTokenMessageHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var tokenHandler = _serviceProvider.GetService<JwtTokenHandler>();
            if (tokenHandler != null)
            {
                var token = await tokenHandler.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }
        catch
        {
            // Ignore all errors - continue without token
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
