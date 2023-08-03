using System;

namespace Screenbox.Core.Providers;

public interface IProvider
{
    string Name { get; }

    string Description { get; }

    Uri IconUri { get; }
}