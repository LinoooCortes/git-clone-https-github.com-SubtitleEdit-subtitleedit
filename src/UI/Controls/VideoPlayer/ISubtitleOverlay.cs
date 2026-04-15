using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Controls.VideoPlayer
{
    /// <summary>
    /// Contract for an overlay layer that renders positioned subtitle text
    /// on top of the video frame.
    ///
    /// This interface is defined now so that <see cref="VideoPlayerControl"/>
    /// and the main view-model can reference a typed hook point without requiring
    /// any real rendering implementation to exist yet.
    ///
    /// The default implementation (<see cref="NullSubtitleOverlay"/>) is a
    /// no-op and imposes zero overhead.  A real Avalonia Canvas or SkiaSharp
    /// implementation will be swapped in during a later phase.
    /// </summary>
    public interface ISubtitleOverlay
    {
        /// <summary>
        /// Called whenever the video position changes so the overlay knows
        /// which subtitles are currently visible.
        /// </summary>
        /// <param name="subtitles">The full ordered subtitle list for the current file.</param>
        /// <param name="currentTime">Current playback position.</param>
        void SetSubtitles(IReadOnlyList<SubtitleLineViewModel> subtitles, TimeSpan currentTime);

        /// <summary>
        /// Requests the overlay to repaint on the next frame.
        /// Called after position or subtitle data changes.
        /// </summary>
        void Invalidate();
    }
}
