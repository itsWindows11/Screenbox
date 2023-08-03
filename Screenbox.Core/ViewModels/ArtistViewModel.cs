using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Screenbox.Core.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Screenbox.Core.ViewModels
{
    public sealed partial class ArtistViewModel
    {
        public List<MediaViewModel> RelatedSongs { get; }

        public string Name { get; }

        public ArtistViewModel()
        {
            Name = string.Empty;
            RelatedSongs = new List<MediaViewModel>();
        }

        public ArtistViewModel(string artist) : this()
        {
            Name = artist;
        }

        [RelayCommand]
        public Task<string> GetImageAsync()
            => Ioc.Default.GetRequiredService<IArtistInfoProvider>().GetImageUriAsync(this);

        public override string ToString()
        {
            return Name;
        }
    }
}
