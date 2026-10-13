using System.Text.RegularExpressions;

namespace CMS.Presentation
{
    /// <summary>
    /// Masks network-share (UNC) paths in error text before it is shown to the user, so a failed
    /// open or download never reveals where files are stored. The rest of the message is kept.
    /// </summary>
    internal static class ErrorTextHelper
    {
        private const string Mask = "[ที่จัดเก็บไฟล์]";

        // Quoted first ('\\srv\share\a b.pdf' or "\\srv\..."), so paths containing spaces are masked whole.
        private static readonly Regex QuotedUnc = new Regex(@"(['""])\\\\[^'""]*\1", RegexOptions.Compiled);
        private static readonly Regex BareUnc = new Regex(@"\\\\[^\s'""]+", RegexOptions.Compiled);

        public static string MaskPaths(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string masked = QuotedUnc.Replace(text, m => m.Groups[1].Value + Mask + m.Groups[1].Value);
            return BareUnc.Replace(masked, Mask);
        }
    }
}
