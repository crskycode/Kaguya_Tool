using System.Text;

namespace StrArcTool.Extensions
{
    public static class StringExtensions
    {
        public static string Escape(this string s)
        {
            var sb = new StringBuilder(256);

            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append(c);
                        sb.Append(c);
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        public static string Unescape(this string s)
        {
            var sb = new StringBuilder(256);

            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\')
                {
                    // Handle escaped character

                    var j = i + 1;

                    if (j < s.Length)
                    {
                        // Look the character to unescape
                        switch (s[j])
                        {
                            case '\\':
                                sb.Append('\\');
                                i++;
                                break;
                            case 'r':
                                sb.Append('\r');
                                i++;
                                break;
                            case 'n':
                                sb.Append('\n');
                                i++;
                                break;
                            case 't':
                                sb.Append('\t');
                                i++;
                                break;
                            default:
                                // Unexpected character
                                break;
                        }
                    }
                    else
                    {
                        // Unexpected end of string
                        sb.Append('\\');
                    }
                }
                else
                {
                    // Handle normal character
                    sb.Append(s[i]);
                }
            }

            return sb.ToString();
        }
    }
}
