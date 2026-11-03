using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nucleus.Bridge.Contracts
{
    /// <summary>
    /// The Patreon supporters credits (Settings &gt; About &gt; Supporters). The host downloads the list, because the
    /// UI's Content-Security-Policy only lets it reach the hub, and keeps a copy for offline starts.
    /// </summary>
    [BridgeService("supporters")]
    public interface ISupportersService
    {
        /// <summary>
        /// The supporters' names: downloaded (and saved for later), else the last saved copy. <c>unavailable</c> when the
        /// list can't be downloaded and no copy was saved yet.
        /// </summary>
        Task<SupportersList> Get(CancellationToken cancellationToken);
    }

    /// <summary>Answer to <see cref="ISupportersService.Get"/>.</summary>
    public sealed class SupportersList
    {
        /// <summary>Names, plain text, in the order of the published list. Never translated: they are data.</summary>
        public List<string> Names { get; set; } = new List<string>();

        /// <summary>True when the download failed and the names come from the copy saved by an earlier download.</summary>
        public bool FromCache { get; set; }

        /// <summary>When that copy was saved (UTC); set when <see cref="FromCache"/> is true.</summary>
        public DateTime? SavedAt { get; set; }

        /// <summary>
        /// URL of the credits music on the assets virtual host (<c>gui\audio\patreon_credits.mp3</c>), or null when the file
        /// is missing.
        /// </summary>
        public string? MusicUrl { get; set; }
    }
}
