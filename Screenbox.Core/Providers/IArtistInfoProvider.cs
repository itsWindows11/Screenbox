using Screenbox.Core.Enums;
using Screenbox.Core.ViewModels;
using System.Threading.Tasks;

namespace Screenbox.Core.Providers;

public interface IArtistInfoProvider : IProvider
{
    Task<string> GetImageUriAsync(ArtistViewModel model);

    Task<string> GetAboutAsync(ArtistViewModel model, AboutSize size);
}