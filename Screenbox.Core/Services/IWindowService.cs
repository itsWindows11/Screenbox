﻿#nullable enable

using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Screenbox.Core;
using Screenbox.Core.Enums;
using Screenbox.Core.Events;

namespace Screenbox.Core.Services
{
    public interface IWindowService
    {
        public event EventHandler<ViewModeChangedEventArgs>? ViewModeChanged;
        public WindowViewMode ViewMode { get; }
        public bool TryEnterFullScreen();
        public void ExitFullScreen();
        public Task<bool> TryExitCompactLayoutAsync();
        public Task<bool> TryEnterCompactLayoutAsync(Size viewSize);
        double ResizeWindow(Size videoDimension, double scalar = 0);
        void HideCursor();
        void ShowCursor();
    }
}