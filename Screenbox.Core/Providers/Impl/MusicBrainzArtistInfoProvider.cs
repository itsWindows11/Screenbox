using MetaBrainz.MusicBrainz;
using Screenbox.Core.Enums;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Screenbox.Core.ViewModels;
using System.Reflection;

namespace Screenbox.Core.Providers.Impl;

public sealed class MusicBrainzArtistInfoProvider : IArtistInfoProvider
{
    private readonly HttpClient _client;

    public MusicBrainzArtistInfoProvider(HttpClient client)
    {
        _client = client;
    }

    public string Name { get; } = "MusicBrainz";

    public string Description { get; } = "MusicBrainz is a MetaBrainz project that aims to create a collaborative music database that is similar to the freedb project.";

    public Uri IconUri { get; } = new Uri("ms-appx:///Assets/Icons/Audio/FileAssociationAAC.targetsize-128.png");

    public async Task<string> GetAboutAsync(ArtistViewModel model, AboutSize size)
    {
        var q = new Query(_client, false);

        var result = (await q.FindArtistsAsync(model.Name)).Results.FirstOrDefault();

        if (result == null)
            return "About not found.";

        var about = result.Item.Disambiguation;

        return size == AboutSize.Short ? (about.Length > 250 ? about.Substring(0, 250) : about) : about;
    }

    public async Task<string> GetImageUriAsync(ArtistViewModel model)
    {
        if (model.Name == "Unknown artist")
        {
            return string.Empty;
        }

        var q = new Query(_client, false);

        var result = (await q.FindArtistsAsync(model.Name)).Results.FirstOrDefault();

        if (result == null)
            return string.Empty;

        var artist = await q.LookupArtistAsync(result.Item.Id, Include.UrlRelationships);

        var image = artist.Relationships.FirstOrDefault(x => x.Type == "image");

        if (image == null)
            return string.Empty;

        return image?.Url?.Resource?.ToString() ?? string.Empty;
    }
}