using Ivy.Services;

namespace Ivy.Samples.Shared;

public class Secrets : IHaveSecrets
{
    public Secret[] GetSecrets()
    {
        return
        [
            new Secret("OpenAi:ApiKey"),
            new Secret("OpenAi:Endpoint")
        ];
    }
}