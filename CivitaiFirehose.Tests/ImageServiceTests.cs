using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CivitaiFirehose.Tests;

public class ImageServiceTests
{
    private readonly ImageService _sut;
    private readonly IServiceProvider _serviceProvider;
    private readonly CivitaiSettings _settings = new();
    
    public ImageServiceTests()
    {
        var services = new ServiceCollection();

        services.AddCivitaiServices();
        services.AddLogging();

        var civitai = new CivitaiSettings { QueryDefaults = new() { Limit = 5 } };
        var options = Substitute.For<IOptions<CivitaiSettings>>();
        options.Value.Returns(civitai);

        services.AddSingleton(options);

        _serviceProvider = services.BuildServiceProvider();

        _sut = _serviceProvider.GetRequiredService<ImageService>();
    }

    [Fact]
    public async Task ShouldEnqueueNewImages()
    {
        var first = new ImageModel("url1", 1, "user1", [], DateTime.Now);
        var second = new ImageModel("url2", 1, "user2", [], DateTime.Now);

        await _sut.Enqueue([first, second]);

        _sut.Images.Should().HaveCount(2);
        _sut.Images.Should().Contain(first);
        _sut.Images.Should().Contain(second);
    }

    [Fact]
    public async Task ShouldNotEnqueueDuplicateImages()
    {
        var image = new ImageModel("url1", 1, "user1", [], DateTime.Now);
        
        await _sut.Enqueue([image, image]);

        _sut.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task ShouldNotEnqueueBlacklistedUsers()
    {
        var blacklist = _serviceProvider.GetRequiredService<BlacklistStore>();
        
        blacklist.BlacklistUser("blockedUser");

        var first = new ImageModel("url1", 1, "blockedUser", [], DateTime.Now);
        var second = new ImageModel("url2", 1, "allowedUser", [], DateTime.Now);

        await _sut.Enqueue([first, second]);

        _sut.Images.Should().HaveCount(1);
        _sut.Images.Should().NotContain(x => x.Username == "blockedUser");
        _sut.Images.Should().Contain(x => x.Username == "allowedUser");
    }

    [Fact]
    public async Task ShouldRaiseEventWithCorrectCount()
    {
        var newImageCount = 0;

        _sut.NewImagesFound += count =>
        {
            newImageCount = count;
            return Task.CompletedTask;
        };

        var first = new ImageModel("url1", 1, "user1", [], DateTime.Now);
        var second = new ImageModel("url2", 1, "user2", [], DateTime.Now);

        await _sut.Enqueue([first, second]);

        newImageCount.Should().Be(2);
    }

    [Fact]
    public async Task ShouldRespectQueueLimit()
    {
        var options = _serviceProvider.GetRequiredService<IOptions<CivitaiSettings>>().Value;

        var limit = options.QueryDefaults.Limit!.Value;
        
        var images = Enumerable
            .Range(0, limit * 2)
            .Select(i => new ImageModel($"url{i}", i, "user1", [], DateTime.Now))
            .ToList();

        await _sut.Enqueue(images);

        // Based on the limit we set in constructor
        _sut.Images.Should().HaveCount(5);
    }
}