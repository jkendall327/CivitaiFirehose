using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CivitaiFirehose.Tests;

public class ImageServiceTests
{
    private readonly ImageService _sut;

    public ImageServiceTests()
    {
        var services = new ServiceCollection();
        
        services.AddCivitaiServices();
        services.AddLogging();

        var civitai = new CivitaiSettings();
        var options = Substitute.For<IOptions<CivitaiSettings>>();
        options.Value.Returns(civitai);
        
        services.AddSingleton(options);
        
        var provider = services.BuildServiceProvider();
        
        _sut = provider.GetRequiredService<ImageService>();
    }
}