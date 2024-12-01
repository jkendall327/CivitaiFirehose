using MediatR;

namespace CivitaiFirehose;

public record ImageData(string Url, string? Title);
public record NewImageNotification(ImageData Image) : INotification;