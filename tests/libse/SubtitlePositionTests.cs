using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Xunit;

namespace Nikse.SubtitleEdit.Tests.Core.Common
{
    /// <summary>
    /// Unit tests that guard the positioning system introduced in Phase 2.
    ///
    /// These tests act as a safety net against the most likely regressions:
    ///   1. Paragraph copy constructor silently drops Position.
    ///   2. SubtitlePosition copy constructor produces a shared reference.
    ///   3. Null-safety: existing code that does not set Position is unaffected.
    /// </summary>
    public class SubtitlePositionTests
    {
        // ── SubtitlePosition copy constructor ─────────────────────────────────

        [Fact]
        public void SubtitlePosition_CopyConstructor_CopiesAllFields()
        {
            var original = new SubtitlePosition
            {
                HorizontalAlignment = SubtitleHorizontalAlignment.Left,
                VerticalAlignment = SubtitleVerticalAlignment.Top,
                LineIndex = 5,
                OffsetX = 10.5,
                OffsetY = 85.0,
            };

            var copy = new SubtitlePosition(original);

            Assert.Equal(original.HorizontalAlignment, copy.HorizontalAlignment);
            Assert.Equal(original.VerticalAlignment, copy.VerticalAlignment);
            Assert.Equal(original.LineIndex, copy.LineIndex);
            Assert.Equal(original.OffsetX, copy.OffsetX);
            Assert.Equal(original.OffsetY, copy.OffsetY);
        }

        [Fact]
        public void SubtitlePosition_CopyConstructor_ProducesIndependentCopy()
        {
            var original = new SubtitlePosition
            {
                HorizontalAlignment = SubtitleHorizontalAlignment.Center,
                VerticalAlignment = SubtitleVerticalAlignment.Bottom,
                LineIndex = 22,
            };

            var copy = new SubtitlePosition(original);

            // Mutate the copy — original must not be affected
            copy.HorizontalAlignment = SubtitleHorizontalAlignment.Right;
            copy.LineIndex = 1;

            Assert.Equal(SubtitleHorizontalAlignment.Center, original.HorizontalAlignment);
            Assert.Equal(22, original.LineIndex);
        }

        [Fact]
        public void SubtitlePosition_IsEmpty_TrueWhenAllNull()
        {
            var pos = new SubtitlePosition();
            Assert.True(pos.IsEmpty);
        }

        [Fact]
        public void SubtitlePosition_IsEmpty_FalseWhenAnyFieldSet()
        {
            Assert.False(new SubtitlePosition { HorizontalAlignment = SubtitleHorizontalAlignment.Left }.IsEmpty);
            Assert.False(new SubtitlePosition { VerticalAlignment = SubtitleVerticalAlignment.Top }.IsEmpty);
            Assert.False(new SubtitlePosition { LineIndex = 1 }.IsEmpty);
            Assert.False(new SubtitlePosition { OffsetX = 50.0 }.IsEmpty);
            Assert.False(new SubtitlePosition { OffsetY = 50.0 }.IsEmpty);
        }

        // ── Paragraph copy constructor ─────────────────────────────────────────

        [Fact]
        public void Paragraph_CopyConstructor_CopiesPosition_WhenNonNull()
        {
            var original = new Paragraph(new TimeCode(1000), new TimeCode(3000), "Hello")
            {
                Position = new SubtitlePosition
                {
                    HorizontalAlignment = SubtitleHorizontalAlignment.Right,
                    VerticalAlignment = SubtitleVerticalAlignment.Bottom,
                    LineIndex = 23,
                    OffsetX = 5.0,
                    OffsetY = 90.0,
                },
            };

            var copy = new Paragraph(original);

            Assert.NotNull(copy.Position);
            Assert.Equal(original.Position!.HorizontalAlignment, copy.Position!.HorizontalAlignment);
            Assert.Equal(original.Position.VerticalAlignment, copy.Position.VerticalAlignment);
            Assert.Equal(original.Position.LineIndex, copy.Position.LineIndex);
            Assert.Equal(original.Position.OffsetX, copy.Position.OffsetX);
            Assert.Equal(original.Position.OffsetY, copy.Position.OffsetY);
        }

        [Fact]
        public void Paragraph_CopyConstructor_PositionIsDeepCopy_NotSameReference()
        {
            var original = new Paragraph(new TimeCode(0), new TimeCode(2000), "Test")
            {
                Position = new SubtitlePosition
                {
                    HorizontalAlignment = SubtitleHorizontalAlignment.Left,
                    LineIndex = 10,
                },
            };

            var copy = new Paragraph(original);

            // Mutate the copy's position — original must not change
            copy.Position!.HorizontalAlignment = SubtitleHorizontalAlignment.Right;
            copy.Position.LineIndex = 99;

            Assert.Equal(SubtitleHorizontalAlignment.Left, original.Position!.HorizontalAlignment);
            Assert.Equal(10, original.Position.LineIndex);

            // Must be different instances
            Assert.NotSame(original.Position, copy.Position);
        }

        [Fact]
        public void Paragraph_CopyConstructor_PositionIsNull_WhenOriginalHasNoPosition()
        {
            var original = new Paragraph(new TimeCode(0), new TimeCode(1000), "No position");

            var copy = new Paragraph(original);

            Assert.Null(copy.Position);
        }

        [Fact]
        public void Paragraph_CopyConstructor_PreservesTextUnchanged()
        {
            const string text = "{\an8}Hello world";
            var original = new Paragraph(new TimeCode(0), new TimeCode(2000), text)
            {
                Position = new SubtitlePosition { VerticalAlignment = SubtitleVerticalAlignment.Top },
            };

            var copy = new Paragraph(original);

            // Paragraph.Text must never be modified by the positioning system
            Assert.Equal(text, copy.Text);
        }

        [Fact]
        public void Paragraph_IsDefault_NotAffectedByPosition()
        {
            // IsDefault checks Text, StartTime, EndTime — not Position.
            // A paragraph with only a position set is still considered "default"
            // by the IsDefault heuristic because no timing or text exists.
            var p = new Paragraph
            {
                Position = new SubtitlePosition { LineIndex = 5 },
            };

            Assert.True(p.IsDefault);
        }

        // ── Enum values ────────────────────────────────────────────────────────

        [Theory]
        [InlineData(SubtitleHorizontalAlignment.Left, 1)]
        [InlineData(SubtitleHorizontalAlignment.Center, 2)]
        [InlineData(SubtitleHorizontalAlignment.Right, 3)]
        public void SubtitleHorizontalAlignment_HasCorrectIntegerValues(SubtitleHorizontalAlignment alignment, int expected)
        {
            Assert.Equal(expected, (int)alignment);
        }

        [Theory]
        [InlineData(SubtitleVerticalAlignment.Top, 1)]
        [InlineData(SubtitleVerticalAlignment.Middle, 2)]
        [InlineData(SubtitleVerticalAlignment.Bottom, 3)]
        public void SubtitleVerticalAlignment_HasCorrectIntegerValues(SubtitleVerticalAlignment alignment, int expected)
        {
            Assert.Equal(expected, (int)alignment);
        }

        // ── ToString ───────────────────────────────────────────────────────────

        [Fact]
        public void SubtitlePosition_ToString_ReturnsUsefulSummary_WhenAlignmentSet()
        {
            var pos = new SubtitlePosition
            {
                VerticalAlignment = SubtitleVerticalAlignment.Bottom,
                HorizontalAlignment = SubtitleHorizontalAlignment.Center,
            };

            var result = pos.ToString();

            Assert.Contains("Bottom", result);
            Assert.Contains("Center", result);
        }

        [Fact]
        public void SubtitlePosition_ToString_IncludesLineIndex_WhenSet()
        {
            var pos = new SubtitlePosition
            {
                VerticalAlignment = SubtitleVerticalAlignment.Bottom,
                LineIndex = 3,
            };

            var result = pos.ToString();

            Assert.Contains("3", result);
        }

        [Fact]
        public void SubtitlePosition_ToString_ReturnsNoPosition_WhenEmpty()
        {
            var pos = new SubtitlePosition();
            Assert.Equal("(no position)", pos.ToString());
        }
    }
}
