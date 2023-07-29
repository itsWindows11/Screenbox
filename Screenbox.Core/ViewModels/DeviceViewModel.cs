using Screenbox.Core.Enums;

namespace Screenbox.Core.ViewModels;

public sealed class DeviceViewModel
{
    public string Name { get; }

    public DeviceType DeviceType { get; }

    public DeviceViewModel(DeviceType deviceType, string name)
    {
        DeviceType = deviceType;
        Name = name;
    }
}