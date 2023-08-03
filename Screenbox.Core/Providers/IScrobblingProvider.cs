using System.Threading.Tasks;

namespace Screenbox.Core.Providers;

public interface IScrobblingProvider
{
    Task<bool> ScrobbleAsync(string title, string subtitle);
}