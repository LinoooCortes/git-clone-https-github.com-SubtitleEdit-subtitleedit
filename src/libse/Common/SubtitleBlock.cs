namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Represents one independent text block within a subtitle event (Paragraph).
    ///
    /// Design principles:
    /// - A Paragraph in legacy mode (Blocks == null) is unaffected by this class.
    /// - When a Paragraph.Blocks list is populated each SubtitleBlock carries its
    ///   own text and optional position, enabling multi-speaker / multi-region
    ///   subtitle events without touching Paragraph.Text.
    /// - This class carries no serialisation logic.  Format parsers are responsible
    ///   for mapping their own multi-block constructs to/from this type.
    /// </summary>
    public class SubtitleBlock
    {
        /// <summary>
        /// The raw text content of this block.  Uses the same newline and inline-tag
        /// conventions as <see cref="Paragraph.Text"/> (i.e. '\n' line breaks,
        /// format-specific tags such as {\an8} are allowed but not required).
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Positioning metadata for this specific block.
        /// Null means "inherit from Paragraph.Position, or use format default".
        /// </summary>
        public SubtitlePosition? Position { get; set; }

        // Reserved for future expansion: SpeakerColor, StyleName, …

        /// <summary>
        /// Parameterless constructor — Text defaults to empty, Position to null.
        /// </summary>
        public SubtitleBlock()
        {
        }

        /// <summary>
        /// Convenience constructor.
        /// </summary>
        public SubtitleBlock(string text, SubtitlePosition? position = null)
        {
            Text = text;
            Position = position;
        }

        /// <summary>
        /// Copy constructor — creates an independent deep copy.
        /// </summary>
        public SubtitleBlock(SubtitleBlock source)
        {
            Text = source.Text;
            Position = source.Position == null ? null : new SubtitlePosition(source.Position);
        }

        /// <inheritdoc/>
        public override string ToString() => Text;
    }
}
