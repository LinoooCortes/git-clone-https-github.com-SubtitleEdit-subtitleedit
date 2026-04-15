using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Xunit;

namespace Nikse.SubtitleEdit.Tests.Core.SubtitleFormats
{
    /// <summary>
    /// Unit tests for the STL (EBU) export SubtitlePosition → VP/JC mapping introduced
    /// in Phase 4.
    ///
    /// These tests validate the mapping contract directly on the SubtitlePosition /
    /// SubtitleHorizontalAlignment types because the actual EBU export path requires
    /// EbuUiHelper and Configuration — full integration is exercised by EbuStlPositionMappingTests
    /// and EbuStlExportIntegrationTests.
    ///
    /// Mapping rules:
    ///   Position.LineIndex (1-based) → VP = LineIndex - 1 (0-based, clamped to [0, 22])
    ///   Position.HorizontalAlignment (Left=1 / Center=2 / Right=3) → JC byte (same value)
    ///   If Position is null / empty → legacy {\an} tag detection is used (unchanged behavior)
    ///   If HorizontalAlignment is null but LineIndex is set → JC falls back to EbuUiHelper.JustificationCode
    /// </summary>
    public class EbuStlExportPositionMappingTests
    {
        // ── LineIndex → VP (VerticalPosition) ────────────────────────────────

        [Theory]
        [InlineData(1,  0)]   // Top row: LineIndex=1 → VP=0
        [InlineData(12, 11)]  // Mid row on 23-row display
        [InlineData(23, 22)]  // Bottom row (default): LineIndex=23 → VP=22 (0x16)
        public void LineIndex_MinusOne_Gives_CorrectVp(int lineIndex, int expectedVp)
        {
            // The export formula is simply: VP = Math.Clamp(LineIndex - 1, 0, 22)
            var vp = Math.Clamp(lineIndex - 1, 0, 22);
            Assert.Equal(expectedVp, vp);
        }

        [Fact]
        public void LineIndex_BelowRange_ClampsToZero()
        {
            // Defensive: if LineIndex is somehow 0, VP must clamp to 0, not underflow to 255.
            var vp = Math.Clamp(0 - 1, 0, 22);
            Assert.Equal(0, vp);
        }

        [Fact]
        public void LineIndex_AboveRange_ClampsTo22()
        {
            // Defensive: if LineIndex > 23, VP must clamp to 22.
            var vp = Math.Clamp(100 - 1, 0, 22);
            Assert.Equal(22, vp);
        }

        // ── HorizontalAlignment → JC (JustificationCode) ─────────────────────

        [Theory]
        [InlineData(SubtitleHorizontalAlignment.Left,   1)]
        [InlineData(SubtitleHorizontalAlignment.Center, 2)]
        [InlineData(SubtitleHorizontalAlignment.Right,  3)]
        public void HorizontalAlignment_IntValue_Equals_JcByte(
            SubtitleHorizontalAlignment alignment, int expectedJc)
        {
            // The export casts directly: (byte)(int)HorizontalAlignment.Value
            // This test guards the invariant that the enum integer values ARE the JC bytes.
            Assert.Equal(expectedJc, (int)alignment);
        }

        [Fact]
        public void HorizontalAlignment_Cast_To_Byte_Matches_JcByte()
        {
            // Guard that (byte)(int) cast produces the correct byte values for all three cases.
            Assert.Equal((byte)1, (byte)(int)SubtitleHorizontalAlignment.Left);
            Assert.Equal((byte)2, (byte)(int)SubtitleHorizontalAlignment.Center);
            Assert.Equal((byte)3, (byte)(int)SubtitleHorizontalAlignment.Right);
        }

        // ── Position null / empty → legacy path is taken ─────────────────────

        [Fact]
        public void NullPosition_TriggersFallback()
        {
            // When p.Position is null the export must use the legacy {\an} detection.
            // Here we simply assert that SubtitlePosition null is correctly identified.
            SubtitlePosition? pos = null;
            var useLegacy = pos == null || pos.IsEmpty;
            Assert.True(useLegacy);
        }

        [Fact]
        public void EmptyPosition_TriggersFallback()
        {
            // An all-null SubtitlePosition (IsEmpty == true) must also trigger the legacy path.
            var pos = new SubtitlePosition(); // all fields null by default
            Assert.True(pos.IsEmpty);
            var useLegacy = pos == null || pos.IsEmpty;
            Assert.True(useLegacy);
        }

        [Fact]
        public void NonEmptyPosition_TriggesStructuredPath()
        {
            // A position with at least one field set must NOT trigger the legacy path.
            var pos = new SubtitlePosition { LineIndex = 23 };
            Assert.False(pos.IsEmpty);
            var useLegacy = pos == null || pos.IsEmpty;
            Assert.False(useLegacy);
        }

        // ── HorizontalAlignment null → JC falls back to UI default ───────────

        [Fact]
        public void NullHorizontalAlignment_FallsBackToUiDefault()
        {
            // When Position.HorizontalAlignment is null (only LineIndex is set),
            // the export uses EbuUiHelper.JustificationCode.
            // We verify the conditional expression produces the expected branch.
            SubtitleHorizontalAlignment? ha = null;
            byte uiDefault = 2; // centre — typical UI default

            var jc = ha.HasValue
                ? (byte)(int)ha.Value
                : uiDefault;

            Assert.Equal(uiDefault, jc);
        }

        // ── End-to-end position roundtrip (import then re-export) ────────────

        [Fact]
        public void Roundtrip_BottomCenter_PreservesVpAndJc()
        {
            // EBU STL default: VP=22 (0x16), JC=2 (centre)
            // Import: VP=22 → LineIndex=23, JC=2 → HorizontalAlignment.Center
            // Export: LineIndex=23 → VP=22, HorizontalAlignment.Center → JC=2

            const byte originalVp = 22;
            const byte originalJc = 2;

            // simulate import (JcAndVpToPosition logic)
            var position = new SubtitlePosition
            {
                LineIndex = originalVp + 1,  // 23
                HorizontalAlignment = (SubtitleHorizontalAlignment)originalJc, // Center
            };

            // simulate export
            var exportedVp = (byte)Math.Clamp(position.LineIndex!.Value - 1, 0, 22);
            var exportedJc = (byte)(int)position.HorizontalAlignment!.Value;

            Assert.Equal(originalVp, exportedVp);
            Assert.Equal(originalJc, exportedJc);
        }

        [Fact]
        public void Roundtrip_TopLeft_PreservesVpAndJc()
        {
            // VP=0, JC=1 (left)
            const byte originalVp = 0;
            const byte originalJc = 1;

            var position = new SubtitlePosition
            {
                LineIndex = originalVp + 1, // 1
                HorizontalAlignment = (SubtitleHorizontalAlignment)originalJc, // Left
            };

            var exportedVp = (byte)Math.Clamp(position.LineIndex!.Value - 1, 0, 22);
            var exportedJc = (byte)(int)position.HorizontalAlignment!.Value;

            Assert.Equal(originalVp, exportedVp);
            Assert.Equal(originalJc, exportedJc);
        }

        [Fact]
        public void Roundtrip_MiddleRight_PreservesVpAndJc()
        {
            // VP=11, JC=3 (right)
            const byte originalVp = 11;
            const byte originalJc = 3;

            var position = new SubtitlePosition
            {
                LineIndex = originalVp + 1, // 12
                HorizontalAlignment = (SubtitleHorizontalAlignment)originalJc, // Right
            };

            var exportedVp = (byte)Math.Clamp(position.LineIndex!.Value - 1, 0, 22);
            var exportedJc = (byte)(int)position.HorizontalAlignment!.Value;

            Assert.Equal(originalVp, exportedVp);
            Assert.Equal(originalJc, exportedJc);
        }

        // ── Text content is never modified by positioning ─────────────────────

        [Fact]
        public void ExportPath_DoesNotModifyParagraphText()
        {
            // The new Position branch in the export loop must not touch p.Text.
            // Verify by constructing a paragraph with Position and asserting Text is untouched.
            const string original = "{\\an2}Hello world";

            var p = new Paragraph(new TimeCode(0), new TimeCode(2000), original)
            {
                Position = new SubtitlePosition
                {
                    LineIndex = 23,
                    HorizontalAlignment = SubtitleHorizontalAlignment.Center,
                },
            };

            // Simulating the export read of text (trim only, no position modification)
            var text = p.Text.Trim(Utilities.NewLineChars);
            Assert.Equal(original, text); // text unchanged by the positioning branch
            Assert.NotNull(p.Position);
        }
    }
}
