namespace CivitaiFirehose;

public static class TagExtractor
{
    public static List<string> GetTagsFromResponse(Item item)
    {
        var tags = new HashSet<string>
        {
            "source:civitai-firehose",
            $"image id:{item.id}",
            $"post id:{item.postId}",
            $"nsfw level:{item.nsfwLevel}",
            $"creator:{item.username}",
            $"base model:{item.baseModel}"
        };
        
        if (item.meta is null) return tags.ToList();

        if (!string.IsNullOrWhiteSpace(item.meta.prompt))
        {
            var prompt = item.meta.prompt.Replace("\n", " ").Trim();
            tags.Add($"prompt:{prompt}");

            var promptedTags = prompt
                .Split(",")
                .Select(x =>
                {
                    // Gets rid of some syntax when invoking LORAs in prompts.
                    x = x.Replace("<", string.Empty).Replace(">", string.Empty);
                    return x.Trim();
                })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            
            foreach (var promptedTag in promptedTags)
            {
                tags.Add(promptedTag);
            }
        }

        if (!string.IsNullOrWhiteSpace(item.meta.negativePrompt))
        {
            tags.Add($"negative prompt:{item.meta.negativePrompt.Replace("\n", " ").Trim()}");
        }

        return tags.ToList();
    }
}