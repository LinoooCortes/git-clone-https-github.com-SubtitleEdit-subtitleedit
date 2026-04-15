namespace Nikse.SubtitleEdit.Core.Enums
{
    /// <summary>
    /// Horizontal alignment for subtitle positioning.
    /// Integer values match the EBU STL Justification Code (JC) byte:
    ///   1 = Left, 2 = Center, 3 = Right.
    /// </summary>
    public enum SubtitleHorizontalAlignment
    {
        Left = 1,
        Center = 2,
        Right = 3,
    }
}
