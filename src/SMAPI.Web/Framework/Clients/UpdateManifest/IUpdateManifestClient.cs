// Copyright 2022 Jamie Taylor
using System;

namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest
{
    /// <summary>An HTTP client for fetching an update manifest from an arbitrary URL.</summary>
    internal interface IUpdateManifestClient : IModSiteClient, IDisposable { }
}

