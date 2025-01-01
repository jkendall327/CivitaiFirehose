using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CivitaiFirehose.Tests;

public sealed class CivitaiServiceTests
{
    private readonly ICivitaiService _sut;
    private readonly TestHttpMessageHandler _handler;

    public CivitaiServiceTests()
    {
        var services = new ServiceCollection();

        _handler = new();

        services.AddHttpClient<CivitaiClient>().ConfigurePrimaryHttpMessageHandler(() => _handler);
        services.AddSingleton<ICivitaiService, CivitaiService>();
        services.AddSingleton<ImageMapper>();
        services.AddLogging();

        var settings = new CivitaiSettings
        {
            QueryDefaults = new()
            {
                Limit = 5,
                Sort = SortOrder.Newest
            }
        };

        var options = Substitute.For<IOptions<CivitaiSettings>>();
        options.Value.Returns(settings);
        services.AddSingleton(options);

        var provider = services.BuildServiceProvider();
        _sut = provider.GetRequiredService<ICivitaiService>();
    }

    [Fact]
    public async Task GetNewestImages_ShouldReturnMappedImages()
    {
        _handler.Response = """
                            {
                                        "items": [
                                            {
                                                "id": 1,
                                                "url": "https://example.com/1.jpg",
                                                "hash": "abc123",
                                                "width": 512,
                                                "height": 512,
                                                "nsfwLevel": "None",
                                                "nsfw": false,
                                                "browsingLevel": 0,
                                                "createdAt": "2024-03-14T12:00:00Z",
                                                "postId": 123,
                                                "username": "testuser",
                                                "baseModel": "SD 1.5",
                                                "stats": {
                                                    "cryCount": 0,
                                                    "laughCount": 0,
                                                    "likeCount": 10,
                                                    "dislikeCount": 0,
                                                    "heartCount": 5,
                                                    "commentCount": 2
                                                }
                                            }
                                        ],
                                        "metadata": {
                                            "nextCursor": null,
                                            "nextPage": null
                                        }
                                    }
                            """;

        var result = await _sut.GetNewestImages();

        var image = result.Single();

        image.ImageUrl.Should().Be("https://example.com/1.jpg");
        image.PostId.Should().Be(123);
        image.Username.Should().Be("testuser");
        image.CreatedAt.Should().Be(DateTime.Parse("2024-03-14T12:00:00Z"));
    }

    [Fact]
    public async Task GetImagesFromPost_ShouldReturnImagesFromSpecificPost()
    {
        _handler.Response = """
                            {
                                        "items": [
                                            {
                                                "id": 1,
                                                "url": "https://example.com/post1-img1.jpg",
                                                "hash": "abc123",
                                                "width": 512,
                                                "height": 512,
                                                "nsfwLevel": "None",
                                                "nsfw": false,
                                                "browsingLevel": 0,
                                                "createdAt": "2024-03-14T12:00:00Z",
                                                "postId": 456,
                                                "username": "testuser",
                                                "baseModel": "SD 1.5",
                                                "stats": {
                                                    "cryCount": 0,
                                                    "laughCount": 0,
                                                    "likeCount": 10,
                                                    "dislikeCount": 0,
                                                    "heartCount": 5,
                                                    "commentCount": 2
                                                }
                                            }
                                        ],
                                        "metadata": {
                                            "nextCursor": null,
                                            "nextPage": null
                                        }
                                    }
                            """;

        var result = await _sut.GetImagesFromPost(456);

        var image = result.Single();
        image.PostId.Should().Be(456);
        image.ImageUrl.Should().Be("https://example.com/post1-img1.jpg");
    }

    /// <summary>
    /// TODO: consider how meaningful this test really is.
    /// </summary>
    [Fact]
    public async Task GetImagesFromModel_ShouldReturnImagesFromSpecificModel()
    {
        _handler.Response = """
                            {
                                        "items": [
                                            {
                                                "id": 1,
                                                "url": "https://example.com/model1-img1.jpg",
                                                "hash": "abc123",
                                                "width": 512,
                                                "height": 512,
                                                "nsfwLevel": "None",
                                                "nsfw": false,
                                                "browsingLevel": 0,
                                                "createdAt": "2024-03-14T12:00:00Z",
                                                "postId": 789,
                                                "username": "testuser",
                                                "baseModel": "SD 1.5",
                                                "stats": {
                                                    "cryCount": 0,
                                                    "laughCount": 0,
                                                    "likeCount": 10,
                                                    "dislikeCount": 0,
                                                    "heartCount": 5,
                                                    "commentCount": 2
                                                }
                                            }
                                        ],
                                        "metadata": {
                                            "nextCursor": null,
                                            "nextPage": null
                                        }
                                    }
                            """;

        var result = await _sut.GetImagesFromModel(789);

        result.Single().ImageUrl.Should().Be("https://example.com/model1-img1.jpg");
    }
}