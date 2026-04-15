using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Controls.VideoPlayer
{
    /// <summary>
    /// A no-op implementation of <see cref="ISubtitleOverlay"/> used as the
    /// default until a real rendering overlay is introduced.
    ///
    /// All methods are empty so the call sites in VideoPlayerControl and
    /// MainViewModel incur zero overhead and zero allocations.
    /// </summary>
    public sealed class NullSubtitleOverlay : ISubtitleOverlay
    {
        /// <inheritdoc/>
        public void SetSubtitles(IReadOnlyList<SubtitleLineViewModel> subtitles, TimeSpan currentTime)
        {
            // Intentional no-op.
        }

        /// <inheritdoc/>
        public void Invalidate()
        {
            // Intentional no-op.
        }
    }
}
