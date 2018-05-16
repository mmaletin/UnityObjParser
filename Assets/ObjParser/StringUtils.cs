
namespace Obj
{
    using System.Collections.Generic;

    public static class StringUtils {

        public static void BufferSplit(this string s, List<string> buffer, char delimiter, bool addEmptyOnDoubleDelim = false)
        {
            buffer.Clear();

            int length = s.Length;

            int substringStart = 0, substringEnd = 0;

            for (int i = 0; i < length; i++)
            {
                var c = s[i];

                if (c == delimiter)
                {
                    bool nonZeroLength = substringStart != substringEnd;
                    if (nonZeroLength || addEmptyOnDoubleDelim)
                    {
                        buffer.Add(nonZeroLength ? s.Substring(substringStart, substringEnd - substringStart) : string.Empty);
                    }
                    substringStart = substringEnd = i + 1;
                }
                else
                {
                    substringEnd = i + 1;
                }

                if (i == length - 1 && substringStart != substringEnd)
                    buffer.Add(s.Substring(substringStart, substringEnd - substringStart));
            }
        }

        public static void BufferSplitIDs(this string s, List<string> buffer, List<int> ids, char delimiter, bool addEmptyOnDoubleDelim = false)
        {
            buffer.Clear();
            ids.Clear();

            int length = s.Length;

            int substringStart = 0, substringEnd = 0;

            for (int i = 0; i < length; i++)
            {
                var c = s[i];

                if (c == delimiter)
                {
                    bool nonZeroLength = substringStart != substringEnd;
                    if (nonZeroLength || addEmptyOnDoubleDelim)
                    {
                        buffer.Add(nonZeroLength ? s.Substring(substringStart, substringEnd - substringStart) : string.Empty);
                        ids.Add(substringStart);
                    }
                    substringStart = substringEnd = i + 1;
                }
                else
                {
                    substringEnd = i + 1;
                }

                if (i == length - 1 && substringStart != substringEnd)
                {
                    buffer.Add(s.Substring(substringStart, substringEnd - substringStart));
                    ids.Add(substringStart);
                }
            }
        }
    }
}