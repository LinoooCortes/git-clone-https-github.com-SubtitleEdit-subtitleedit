using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class Paragraph 
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public TimeCode StartTime { get; set; }

        public TimeCode EndTime { get; set; }

        public TimeCode Duration => new TimeCode(EndTime.TotalMilliseconds - StartTime.TotalMilliseconds);
        public double DurationTotalMilliseconds => EndTime.TotalMilliseconds - StartTime.TotalMilliseconds;
        public double DurationTotalSeconds => (EndTime.TotalMilliseconds - StartTime.TotalMilliseconds) / TimeCode.BaseUnit;

        public bool Forced { get; set; }

        /// <summary>
        /// Extra info (style name for ASSA).
        /// </summary>
        public string Extra { get; set; }

        public bool IsComment { get; set; }

        public string Actor { get; set; }
        public string Region { get; set; }

        public string MarginL { get; set; }
        public string MarginR { get; set; }
        public string MarginV { get; set; }

        public string Effect { get; set; }

        public int Layer { get; set; }

        public string Id { get; }

        public string Language { get; set; }

        public string Style { get; set; }

        public bool NewSection { get; set; }

        public string Bookmark { get; set; }

        /// <summary>
        /// Structured positioning metadata for this subtitle entry.
        /// Null means no explicit position is set; the renderer falls back
        /// to its default rules.  Paragraph.Text is never modified by the
        /// positioning system — all positioning lives here exclusively.
        /// </summary>
        public SubtitlePosition? Position { get; set; }

        /// <summary>
        /// Optional list of independent text blocks within this subtitle event.
        /// <para>
        /// When <c>null</c> (legacy mode) the subtitle is described entirely by
        /// <see cref="Text"/> and <see cref="Position"/>, and all existing code
        /// paths behave exactly as before — no migration or conversion is applied.
        /// </para>
        /// <para>
        /// When non-null (multi-block mode) each <see cref="SubtitleBlock"/> carries
        /// its own text and optional position, enabling multi-speaker / multi-region
        /// events.  <see cref="Text"/> is then used only as a serialisation fallback
        /// for formats that cannot express multiple blocks.
        /// </para>
        /// </summary>
        public List<SubtitleBlock>? Blocks { get; set; }

        /// <summary>
        /// Returns the effective display text for this paragraph.
        /// <list type="bullet">
        ///   <item><description>
        ///     Legacy mode (<see cref="Blocks"/> is <c>null</c>): returns <see cref="Text"/> unchanged.
        ///   </description></item>
        ///   <item><description>
        ///     Multi-block mode (<see cref="Blocks"/> is non-null): joins every block's text
        ///     with <see cref="Environment.NewLine"/> so callers that can only handle a
        ///     single string still receive meaningful content.
        ///   </description></item>
        /// </list>
        /// Existing code that reads <see cref="Text"/> directly is unaffected; this helper
        /// is provided as a forward-compatible alternative for new code paths.
        /// </summary>
        public string GetEffectiveText()
        {
            if (Blocks == null)
            {
                return Text;
            }

            return string.Join(Environment.NewLine, Blocks.Select(b => b.Text));
        }

        public bool IsDefault => Math.Abs(StartTime.TotalMilliseconds) < 0.01 && Math.Abs(EndTime.TotalMilliseconds) < 0.01 && string.IsNullOrEmpty(Text);

        private static string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        public Paragraph() : this(new TimeCode(), new TimeCode(), string.Empty)
        {
        }

        public Paragraph(TimeCode startTime, TimeCode endTime, string text)
        {
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
            Id = GenerateId();
        }

        public Paragraph(Paragraph paragraph, bool generateNewId = true)
        {
            Number = paragraph.Number;
            Text = paragraph.Text;
            StartTime = new TimeCode(paragraph.StartTime.TotalMilliseconds);
            EndTime = new TimeCode(paragraph.EndTime.TotalMilliseconds);
            Forced = paragraph.Forced;
            Extra = paragraph.Extra;
            IsComment = paragraph.IsComment;
            Actor = paragraph.Actor;
            Region = paragraph.Region;
            MarginL = paragraph.MarginL;
            MarginR = paragraph.MarginR;
            MarginV = paragraph.MarginV;
            Effect = paragraph.Effect;
            Layer = paragraph.Layer;
            Id = generateNewId ? GenerateId() : paragraph.Id;
            Language = paragraph.Language;
            Style = paragraph.Style;
            NewSection = paragraph.NewSection;
            Bookmark = paragraph.Bookmark;
            Position = paragraph.Position == null ? null : new SubtitlePosition(paragraph.Position);
            Blocks = paragraph.Blocks == null
                ? null
                : paragraph.Blocks.Select(b => new SubtitleBlock(b)).ToList();
        }

        public Paragraph(string text, double startTotalMilliseconds, double endTotalMilliseconds)
            : this(new TimeCode(startTotalMilliseconds), new TimeCode(endTotalMilliseconds), text)
        {
        }

        public void Adjust(double factor, double adjustmentInSeconds)
        {
            if (StartTime.IsMaxTime)
            {
                return;
            }

            StartTime.TotalMilliseconds = StartTime.TotalMilliseconds * factor + adjustmentInSeconds * TimeCode.BaseUnit;
            EndTime.TotalMilliseconds = EndTime.TotalMilliseconds * factor + adjustmentInSeconds * TimeCode.BaseUnit;
        }

        public override string ToString()
        {
            return $"{StartTime} --> {EndTime} {Text}";
        }

        public int NumberOfLines => Utilities.GetNumberOfLines(Text);

        public double WordsPerMinute
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return 0;
                }

                return 60.0 / DurationTotalSeconds * Text.CountWords();
            }
        }

        public double GetCharactersPerSecond()
        {
            if (DurationTotalMilliseconds < 1)
            {
                return 999;
            }

            return (double)Text.CountCharacters(true) / DurationTotalSeconds;
        }

        public double GetCharactersPerSecond(ICalcLength calc)
        {
            if (DurationTotalMilliseconds < 1)
            {
                return 999;
            }

            return (double)calc.CountCharacters(Text, true) / DurationTotalSeconds;
        }
    }
}
