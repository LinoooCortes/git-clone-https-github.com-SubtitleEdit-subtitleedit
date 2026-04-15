using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Main;

/// <summary>
/// Partial extension of <see cref="MainViewModel"/> that owns the
/// positioning system helpers introduced in Phase 2.
///
/// Split into a separate file to keep the changes surgically isolated
/// from the large MainViewModel.cs.
/// </summary>
public partial class MainViewModel
{
    // ── Overlay hook ──────────────────────────────────────────────────────────

    /// <summary>
    /// Subscribes the SubtitleOverlay hook to VideoPlayerControl.PositionChanged
    /// so the overlay is notified on every frame tick.
    ///
    /// Call this method immediately after <see cref="VideoPlayerControl"/> is
    /// assigned (i.e. inside InitVideoPlayer.MakeLayoutVideoPlayer, or wherever
    /// VideoPlayerControl is wired up).  The call is idempotent — re-wiring is
    /// safe because the previous handler is removed before a new one is added.
    /// </summary>
    public void InitializePositioningOverlay()
    {
        if (VideoPlayerControl == null)
        {
            return;
        }

        // Remove any previously registered handler to avoid duplicate calls
        // on video-player reload (InitVideoPlayer is called multiple times).
        VideoPlayerControl.PositionChanged -= OnVideoPositionChangedForOverlay;
        VideoPlayerControl.PositionChanged += OnVideoPositionChangedForOverlay;
    }

    private void OnVideoPositionChangedForOverlay(double positionSeconds)
    {
        VideoPlayerControl?.SubtitleOverlay.SetSubtitles(
            Subtitles,
            TimeSpan.FromSeconds(positionSeconds));
    }

    // ── Position read/write helpers ───────────────────────────────────────────

    /// <summary>
    /// Reads the <see cref="SubtitlePosition"/> from the currently selected
    /// <see cref="SubtitleLineViewModel"/> and returns it, or <c>null</c> if
    /// nothing is selected or no position is set.
    ///
    /// This is a pure read — it does not modify any state.
    /// </summary>
    private SubtitlePosition? UpdatePositionFromSelected()
    {
        var selected = SelectedSubtitle;
        if (selected == null || !selected.HasPosition)
        {
            return null;
        }

        return new SubtitlePosition
        {
            HorizontalAlignment = selected.HorizontalAlignment,
            VerticalAlignment = selected.VerticalAlignment,
            LineIndex = selected.LineIndex,
            OffsetX = selected.OffsetX,
            OffsetY = selected.OffsetY,
        };
    }

    /// <summary>
    /// Copies the position fields from a given <see cref="SubtitlePosition"/>
    /// to the currently selected <see cref="SubtitleLineViewModel"/>.
    ///
    /// Passing <c>null</c> clears all positioning on the selected entry.
    /// Does nothing when no entry is selected.
    /// </summary>
    private void PushPositionToSelected(SubtitlePosition? position)
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        if (position == null)
        {
            selected.HorizontalAlignment = null;
            selected.VerticalAlignment = null;
            selected.LineIndex = null;
            selected.OffsetX = null;
            selected.OffsetY = null;
        }
        else
        {
            selected.HorizontalAlignment = position.HorizontalAlignment;
            selected.VerticalAlignment = position.VerticalAlignment;
            selected.LineIndex = position.LineIndex;
            selected.OffsetX = position.OffsetX;
            selected.OffsetY = position.OffsetY;
        }
    }

    /// <summary>
    /// Clears all position data from the currently selected subtitle.
    /// Intended to be called from a "Clear position" button or menu item
    /// when the feature is surfaced to the user.
    /// </summary>
    private void ClearPositionForSelected()
    {
        PushPositionToSelected(null);
    }

    /// <summary>
    /// Propagates the selected subtitle's position to every entry in
    /// <paramref name="targetSubtitles"/>.  Used for bulk operations such as
    /// "apply this alignment to all selected rows".
    /// </summary>
    private static void CopyPositionToMany(
        SubtitleLineViewModel source,
        IEnumerable<SubtitleLineViewModel> targetSubtitles)
    {
        var position = source.HasPosition
            ? new SubtitlePosition
            {
                HorizontalAlignment = source.HorizontalAlignment,
                VerticalAlignment = source.VerticalAlignment,
                LineIndex = source.LineIndex,
                OffsetX = source.OffsetX,
                OffsetY = source.OffsetY,
            }
            : null;

        foreach (var target in targetSubtitles)
        {
            if (target == source)
            {
                continue;
            }

            target.HorizontalAlignment = position?.HorizontalAlignment;
            target.VerticalAlignment = position?.VerticalAlignment;
            target.LineIndex = position?.LineIndex;
            target.OffsetX = position?.OffsetX;
            target.OffsetY = position?.OffsetY;
        }
    }
}
