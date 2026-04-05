using AutoMapper;
using Microsoft.Extensions.Configuration;
using Services;

namespace Services.Tests;

/// <summary>
/// Factoria centralizada para instancias reutilizables en los tests.
/// </summary>
internal static class TestHelpers
{
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return config.CreateMapper();
    }

    public static IConfiguration CreateConfiguration(Dictionary<string, string?>? extra = null)
    {
        var defaults = new Dictionary<string, string?>
        {
            ["auth:secretKey"] = "super-secret-key-for-tests-only-32chars!!",
            ["auth:authActivityTime"] = "30"
        };

        if (extra != null)
            foreach (var kv in extra)
                defaults[kv.Key] = kv.Value;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(defaults)
            .Build();
    }
}
