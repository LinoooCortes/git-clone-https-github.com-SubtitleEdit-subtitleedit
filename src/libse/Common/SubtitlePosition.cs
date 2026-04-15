using Nikse.SubtitleEdit.Core.Enums;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Structured positioning metadata for a subtitle paragraph.
    ///
    /// Design principles:
    /// - All fields are nullable.  A SubtitlePosition where every field is null
    ///   is semantically equivalent to "no explicit position" — the renderer
    ///   falls back to its default rules (e.g. bottom-center for most formats).
    /// - OffsetX / OffsetY are expressed as percentages of the video frame
    ///   (0.0–100.0) so they are resolution-independent.
    /// - LineIndex is 1-based, matching the EBU STL vertical-position byte
    ///   (rows 1–23).
    /// - This class carries no serialisation logic.  Each subtitle format
    ///   parser is responsible for converting its own position tokens to/from
    ///   this type.
    /// - This class does NOT modify Paragraph.Text — positioning is always
    ///   stored out-of-band.
    /// </summary>
    public class SubtitlePosition
    {
        /// <summary>
        /// Horizontal alignment.  Null means "use format/style default".
        /// </summary>
        public SubtitleHorizontalAlignment? HorizontalAlignment { get; set; }

        /// <summary>
        /// Vertical alignment.  Null means "use format/style default".
        /// </summary>
        public SubtitleVerticalAlignment? VerticalAlignment { get; set; }

        /// <summary>
        /// 1-based row index (1–23), following the EBU STL convention.
        /// Null means "no explicit line index".
        /// </summary>
        public int? LineIndex { get; set; }

        /// <summary>
        /// Horizontal offset as a percentage of the video frame width (0.0–100.0).
        /// Null means "no explicit X offset".
        /// </summary>
        public double? OffsetX { get; set; }

        /// <summary>
        /// Vertical offset as a percentage of the video frame height (0.0–100.0).
        /// Null means "no explicit Y offset".
        /// </summary>
        public double? OffsetY { get; set; }

        /// <summary>
        /// Returns true when no explicit position information is stored,
        /// i.e. all fields are null.
        /// </summary>
        public bool IsEmpty =>
            HorizontalAlignment == null &&
            VerticalAlignment == null &&
            LineIndex == null &&
            OffsetX == null &&
            OffsetY == null;

        /// <summary>
        /// Parameterless constructor — all fields default to null.
        /// </summary>
        public SubtitlePosition()
        {
        }

        /// <summary>
        /// Copy constructor — creates an independent deep copy.
        /// </summary>
        public SubtitlePosition(SubtitlePosition source)
        {
            HorizontalAlignment = source.HorizontalAlignment;
            VerticalAlignment = source.VerticalAlignment;
            LineIndex = source.LineIndex;
            OffsetX = source.OffsetX;
            OffsetY = source.OffsetY;
        }

        /// <summary>
        /// Returns a human-readable summary for use in UI tooltips / header labels.
        /// Example outputs: "Bottom · Center", "Top · Left  (line 3)", "X 20% / Y 80%"
        /// </summary>
        public override string ToString()
        {
            var parts = new System.Text.StringBuilder();

            if (VerticalAlignment.HasValue || HorizontalAlignment.HasValue)
            {
                if (VerticalAlignment.HasValue)
                {
                    parts.Append(VerticalAlignment.Value.ToString());
                }

                if (HorizontalAlignment.HasValue)
                {
                    if (parts.Length > 0)
                    {
                        parts.Append(" · ");
                    }

                    parts.Append(HorizontalAlignment.Value.ToString());
                }

                if (LineIndex.HasValue)
                {
                    parts.Append($"  (line {LineIndex.Value})");
                }
            }
            else if (OffsetX.HasValue || OffsetY.HasValue)
            {
                parts.Append($"X {OffsetX?.ToString("F1") ?? "–"}% / Y {OffsetY?.ToString("F1") ?? "–"}%");
            }

            return parts.Length > 0 ? parts.ToString() : "(no position)";
        }
    }
}
