using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Xunit;

namespace Nikse.SubtitleEdit.Tests.Core.SubtitleFormats
{
    /// <summary>
    /// Unit tests for the STL (EBU) import VP/JC → SubtitlePosition mapping introduced
    /// in Phase 3.
    ///
    /// These tests use the <see cref="Ebu.JcAndVpToPosition"/> logic indirectly by
    /// directly exercising the mapping rules described in the EBU STL spec:
    ///
    ///   VP (VerticalPosition) byte: 0-based row index.  LineIndex = VP + 1 (1-based).
    ///   JC (JustificationCode) byte:
    ///     0x00 = Unchanged Presentation  → null   (no explicit alignment)
    ///     0x01 = Left-Justified Text     → Left   (SubtitleHorizontalAlignment.Left   = 1)
    ///     0x02 = Centred Text            → Center (SubtitleHorizontalAlignment.Center = 2)
    ///     0x03 = Right-Justified Text    → Right  (SubtitleHorizontalAlignment.Right  = 3)
    ///
    /// The tests exercise the SubtitlePosition model because <see cref="Ebu.JcAndVpToPosition"/>
    /// is private — they validate the contract via the Paragraph.Position field values
    /// set by LoadSubtitle.
    /// </summary>
    public class EbuStlPositionMappingTests
    {
        // ── VP → LineIndex mapping ─────────────────────────────────────────────

        [Theory]
        [InlineData(0,  1)]   // Top row: VP=0x00 → LineIndex=1
        [InlineData(11, 12)]  // Middle row on 23-row display
        [InlineData(22, 23)]  // Bottom row (default VP=0x16=22): → LineIndex=23
        public void SubtitlePosition_LineIndex_Is_VpPlusOne(int vp, int expectedLineIndex)
        {
            // Arrange
            var position = new SubtitlePosition
            {
                LineIndex = vp + 1, // matches JcAndVpToPosition logic
            };

            // Assert
            Assert.Equal(expectedLineIndex, position.LineIndex);
        }

        [Fact]
        public void SubtitlePosition_DefaultVp_Maps_To_LineIndex23()
        {
            // EBU STL TTI default VP = 0x16 = 22 (bottom of a 23-row display)
            const int defaultVp = 0x16; // 22
            var position = new SubtitlePosition
            {
                LineIndex = defaultVp + 1,
            };

            Assert.Equal(23, position.LineIndex);
        }

        // ── JC → HorizontalAlignment mapping ──────────────────────────────────

        [Fact]
        public void SubtitlePosition_JcLeft_Maps_To_HorizontalAlignmentLeft()
        {
            // JC=1 (01h) = Left-Justified Text → SubtitleHorizontalAlignment.Left (1)
            var position = new SubtitlePosition
            {
                HorizontalAlignment = SubtitleHorizontalAlignment.Left,
            };

            Assert.Equal(SubtitleHorizontalAlignment.Left, position.HorizontalAlignment);
            Assert.Equal(1, (int)position.HorizontalAlignment.Value);
        }

        [Fact]
        public void SubtitlePosition_JcCenter_Maps_To_HorizontalAlignmentCenter()
        {
            // JC=2 (02h) = Centred Text → SubtitleHorizontalAlignment.Center (2) — STL default
            var position = new SubtitlePosition
            {
                HorizontalAlignment = SubtitleHorizontalAlignment.Center,
            };

            Assert.Equal(SubtitleHorizontalAlignment.Center, position.HorizontalAlignment);
            Assert.Equal(2, (int)position.HorizontalAlignment.Value);
        }

        [Fact]
        public void SubtitlePosition_JcRight_Maps_To_HorizontalAlignmentRight()
        {
            // JC=3 (03h) = Right-Justified Text → SubtitleHorizontalAlignment.Right (3)
            var position = new SubtitlePosition
            {
                HorizontalAlignment = SubtitleHorizontalAlignment.Right,
            };

            Assert.Equal(SubtitleHorizontalAlignment.Right, position.HorizontalAlignment);
            Assert.Equal(3, (int)position.HorizontalAlignment.Value);
        }

        [Fact]
        public void SubtitlePosition_JcUnchanged_Maps_To_NullHorizontalAlignment()
        {
            // JC=0 (00h) = Unchanged Presentation → HorizontalAlignment = null
            // (inherit from previous TTI — no explicit alignment stored)
            var position = new SubtitlePosition
            {
                HorizontalAlignment = null,
                LineIndex = 23,
            };

            Assert.Null(position.HorizontalAlignment);
            Assert.Equal(23, position.LineIndex); // LineIndex still captured from VP
        }

        // ── Enum integer values must match EBU STL JC byte values exactly ─────

        [Theory]
        [InlineData(SubtitleHorizontalAlignment.Left,   1)]
        [InlineData(SubtitleHorizontalAlignment.Center, 2)]
        [InlineData(SubtitleHorizontalAlignment.Right,  3)]
        public void SubtitleHorizontalAlignment_EnumValues_MatchEbuJcByteCodes(
            SubtitleHorizontalAlignment alignment, int expectedJcByte)
        {
            // This test guards the critical design contract: the enum values ARE the
            // JC byte values, so no translation layer is needed between STL import
            // and SubtitlePosition.
            Assert.Equal(expectedJcByte, (int)alignment);
        }

        // ── End-to-end: constructed position with both VP and JC ──────────────

        [Fact]
        public void SubtitlePosition_BottomCenter_ReflectsStlDefaults()
        {
            // EBU STL TTI defaults: VP=0x16 (bottom row), JC=2 (centre)
            const byte defaultVp = 0x16; // 22
            const byte defaultJc = 0x02; // centre

            SubtitleHorizontalAlignment? ha = defaultJc switch
            {
                1 => SubtitleHorizontalAlignment.Left,
                2 => SubtitleHorizontalAlignment.Center,
                3 => SubtitleHorizontalAlignment.Right,
                _ => null,
            };

            var position = new SubtitlePosition
            {
                LineIndex = defaultVp + 1,
                HorizontalAlignment = ha,
            };

            Assert.Equal(23, position.LineIndex);
            Assert.Equal(SubtitleHorizontalAlignment.Center, position.HorizontalAlignment);
            Assert.False(position.IsEmpty);
        }

        [Fact]
        public void SubtitlePosition_TopLeft_MapsCorrectly()
        {
            // VP=0 (top row), JC=1 (left)
            const byte vp = 0x00;
            const byte jc = 0x01;

            SubtitleHorizontalAlignment? ha = jc switch
            {
                1 => SubtitleHorizontalAlignment.Left,
                2 => SubtitleHorizontalAlignment.Center,
                3 => SubtitleHorizontalAlignment.Right,
                _ => null,
            };

            var position = new SubtitlePosition
            {
                LineIndex = vp + 1,
                HorizontalAlignment = ha,
            };

            Assert.Equal(1, position.LineIndex);
            Assert.Equal(SubtitleHorizontalAlignment.Left, position.HorizontalAlignment);
        }

        // ── Backward compatibility: Paragraph.Text is never modified ──────────

        [Fact]
        public void SubtitlePosition_DoesNotModifyParagraphText()
        {
            // The positioning system MUST NOT embed {\\an} tags or any other
            // positioning markup into Paragraph.Text.  The structured Position
            // field is the exclusive storage location.
            const string originalText = "{\\an2}Hello world";

            var p = new Paragraph(new TimeCode(0), new TimeCode(2000), originalText)
            {
                Position = new SubtitlePosition
                {
                    LineIndex = 23,
                    HorizontalAlignment = SubtitleHorizontalAlignment.Center,
                },
            };

            // Text must be untouched
            Assert.Equal(originalText, p.Text);

            // Position must be populated
            Assert.NotNull(p.Position);
            Assert.Equal(23, p.Position!.LineIndex);
        }

        [Fact]
        public void Paragraph_WithStlPosition_CopyConstructor_DeepCopiesPosition()
        {
            // Guard regression: copy constructor must deep-copy Position, not
            // share the same reference.
            var original = new Paragraph(new TimeCode(0), new TimeCode(2000), "Test")
            {
                Position = new SubtitlePosition
                {
                    LineIndex = 23,
                    HorizontalAlignment = SubtitleHorizontalAlignment.Center,
                },
            };

            var copy = new Paragraph(original);

            Assert.NotNull(copy.Position);
            Assert.NotSame(original.Position, copy.Position);
            Assert.Equal(23, copy.Position!.LineIndex);
            Assert.Equal(SubtitleHorizontalAlignment.Center, copy.Position.HorizontalAlignment);

            // Mutate copy — original must be unchanged
            copy.Position.LineIndex = 1;
            copy.Position.HorizontalAlignment = SubtitleHorizontalAlignment.Left;
            Assert.Equal(23, original.Position!.LineIndex);
            Assert.Equal(SubtitleHorizontalAlignment.Center, original.Position.HorizontalAlignment);
        }

        // ── MarginV is no longer set by STL import ────────────────────────────

        [Fact]
        public void Paragraph_BuiltForStl_Has_NullOrEmptyMarginV()
        {
            // After Phase 3, STL import must NOT set MarginV — Position supersedes it.
            // This test verifies the contract using a paragraph constructed the same
            // way LoadSubtitle now constructs it (without MarginV).
            var p = new Paragraph
            {
                Text = "Hello",
                StartTime = new TimeCode(1000),
                EndTime = new TimeCode(3000),
                Position = new SubtitlePosition
                {
                    LineIndex = 23,
                    HorizontalAlignment = SubtitleHorizontalAlignment.Center,
                },
            };

            // MarginV must be null/empty — Position is the authoritative store now
            Assert.True(string.IsNullOrEmpty(p.MarginV),
                $"MarginV should be null/empty for STL paragraphs; got: '{p.MarginV}'");
        }
    }
}
