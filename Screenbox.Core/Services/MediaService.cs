﻿#nullable enable

using System;
using Windows.Storage;
using Windows.Storage.AccessCache;
using LibVLCSharp.Shared;
using CommunityToolkit.Diagnostics;

namespace Screenbox.Core.Services
{
    public sealed class MediaService : IMediaService
    {
        private readonly LibVlcService _libVlcService;

        public MediaService(LibVlcService libVlcService)
        {
            _libVlcService = libVlcService;

            // Clear FA periodically because of 1000 items limit
            StorageApplicationPermissions.FutureAccessList.Clear();
        }

        public Media CreateMedia(object source)
        {
            return source switch
            {
                IStorageFile file => CreateMedia(file),
                string str => CreateMedia(str),
                Uri uri => CreateMedia(uri),
                _ => throw new ArgumentOutOfRangeException(nameof(source))
            };
        }

        public Media CreateMedia(string str)
        {
            if (Uri.TryCreate(str, UriKind.Absolute, out Uri uri))
            {
                return CreateMedia(uri);
            }

            Guard.IsNotNull(_libVlcService.LibVlc, nameof(_libVlcService.LibVlc));
            LibVLC libVlc = _libVlcService.LibVlc;
            return new Media(libVlc, str);
        }

        public Media CreateMedia(IStorageFile file)
        {
            Guard.IsNotNull(_libVlcService.LibVlc, nameof(_libVlcService.LibVlc));
            LibVLC libVlc = _libVlcService.LibVlc;
            string mrl = "winrt://" + StorageApplicationPermissions.FutureAccessList.Add(file, "media");
            return new Media(libVlc, mrl, FromType.FromLocation);
        }

        public Media CreateMedia(Uri uri)
        {
            Guard.IsNotNull(_libVlcService.LibVlc, nameof(_libVlcService.LibVlc));
            LibVLC libVlc = _libVlcService.LibVlc;
            return new Media(libVlc, uri);
        }

        public void DisposeMedia(Media media)
        {
            string mrl = media.Mrl;
            if (mrl.StartsWith("winrt://"))
            {
                try
                {
                    StorageApplicationPermissions.FutureAccessList.Remove(mrl.Substring(8));
                }
                catch (Exception)
                {
                    LogService.Log($"Failed to remove FAL: {mrl.Substring(8)}");
                }
            }

            media.Dispose();
        }
    }
}
