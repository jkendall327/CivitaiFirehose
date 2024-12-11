using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using FluentAssertions;
using NSubstitute;

namespace CivitaiFirehose.Tests;

public class CivitaiServiceTests
{
    private readonly TestHttpMessageHandler _handler = new();
    private readonly CivitaiService _service;
    private int _lastNewImageCount;

    public CivitaiServiceTests()
    {
        var httpClient = new HttpClient(_handler);
        
        var settings = new CivitaiSettings
        {
            PollingPeriod = TimeSpan.FromSeconds(15),
            QueryDefaults = new()
            {
                Limit = 20,
                Nsfw = NsfwFilter.None,
                Sort = SortOrder.Newest
            }
        };
        
        var options = Substitute.For<IOptions<CivitaiSettings>>();
        var optionsMonitor = Substitute.For<IOptionsMonitor<CivitaiSettings>>();
        
        options.Value.Returns(settings);
        optionsMonitor.CurrentValue.Returns(settings);
        
        var client = new CivitaiClient(httpClient, optionsMonitor, NullLogger<CivitaiClient>.Instance);
        
        _service = new(client, options, NullLogger<CivitaiService>.Instance);
        
        _service.NewImagesFound += count =>
        {
            _lastNewImageCount = count;
            return Task.CompletedTask;
        };
    }

    [Fact]
    public async Task HandlesEmptyResponse()
    {
        _handler.Response = """
            {
                "items": [],
                "metadata": {
                    "nextCursor": null,
                    "nextPage": null
                }
            }
            """;

        await _service.PollCivitai(CancellationToken.None);

        _service.Images.Should().BeEmpty();
        _lastNewImageCount.Should().Be(0);
    }

    [Fact]
    public async Task HandlesBasicResponse()
    {
        _handler.Response = """
            {
                "items": [
                    {
                        "id": 1,
                        "url": "https://example.com/1.png",
                        "hash": "abc123",
                        "width": 512,
                        "height": 512,
                        "nsfwLevel": "None",
                        "nsfw": false,
                        "browsingLevel": 0,
                        "createdAt": "2024-01-01",
                        "postId": 123,
                        "stats": {
                            "cryCount": 0,
                            "laughCount": 0,
                            "likeCount": 10,
                            "dislikeCount": 0,
                            "heartCount": 5,
                            "commentCount": 2
                        },
                        "meta": null,
                        "username": "testuser",
                        "baseModel": "SD 1.5"
                    }
                ],
                "metadata": {
                    "nextCursor": null,
                    "nextPage": null
                }
            }
            """;

        await _service.PollCivitai(CancellationToken.None);

        var images = _service.Images.ToList();
        images.Should().HaveCount(1);

        var image = images[0];
        image.ImageUrl.Should().Be("https://example.com/1.png");
        image.PostId.Should().Be(123);
        image.Username.Should().Be("testuser");
            
        _lastNewImageCount.Should().Be(1);
    }

    [Fact]
    public async Task SkipsDuplicateImages()
    {
        // First poll
        _handler.Response = """
            {
                "items": [
                    {
                        "id": 1,
                        "url": "https://example.com/1.png",
                        "hash": "abc123",
                        "width": 512,
                        "height": 512,
                        "nsfwLevel": "None",
                        "nsfw": false,
                        "browsingLevel": 0,
                        "createdAt": "2024-01-01",
                        "postId": 123,
                        "stats": { },
                        "meta": null,
                        "username": "testuser",
                        "baseModel": "SD 1.5"
                    }
                ],
                "metadata": { }
            }
            """;

        await _service.PollCivitai(CancellationToken.None);
        _lastNewImageCount.Should().Be(1);

        _lastNewImageCount = 0;
        
        // Second poll with same image
        await _service.PollCivitai(CancellationToken.None);
        _lastNewImageCount.Should().Be(0);
        
        _service.Images.Should().ContainSingle();
    }

    [Fact]
    public async Task SkipsBlacklistedUsers()
    {
        _handler.Response = """
            {
                "items": [
                    {
                        "id": 1,
                        "url": "https://example.com/1.png",
                        "hash": "abc123",
                        "width": 512,
                        "height": 512,
                        "nsfwLevel": "None",
                        "nsfw": false,
                        "browsingLevel": 0,
                        "createdAt": "2024-01-01",
                        "postId": 123,
                        "stats": { },
                        "meta": null,
                        "username": "testuser",
                        "baseModel": "SD 1.5"
                    }
                ],
                "metadata": { }
            }
            """;

        _service.BlacklistUser("testuser");
        await _service.PollCivitai(CancellationToken.None);

        _service.Images.Should().BeEmpty();
        _lastNewImageCount.Should().Be(0);
    }
}