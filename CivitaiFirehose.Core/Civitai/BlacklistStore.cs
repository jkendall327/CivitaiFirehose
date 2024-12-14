using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public sealed class BlacklistStore(IOptions<CivitaiSettings> options)
{
    private readonly HashSet<string> _blacklistedUsers = [..options.Value.ExcludedCreators];
    
    public bool IsBlacklisted(string user) => _blacklistedUsers.Contains(user);
    public void BlacklistUser(string username) => _blacklistedUsers.Add(username);
}