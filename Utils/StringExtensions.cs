using System.Collections.Generic;
using System.Text;

namespace Chat.Utils;

public static class StringExtensions
{
    
    public static List<string> SplitNicely(this string input, int maxBytes = 512, string softWrapChars = "\n")
    {
        List<string> chunks = new List<string>();
        StringBuilder currentChunk = new StringBuilder();
        int currentBytes = 0;
        int lastSoftWrapIndex = -1;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            currentChunk.Append(c);
            int charBytes = Encoding.UTF8.GetByteCount(new[] { c });
            currentBytes += charBytes;

            // Check if this is a soft wrap candidate
            if (softWrapChars.IndexOf(c) >= 0)
            {
                lastSoftWrapIndex = currentChunk.Length;
            }

            if (currentBytes > maxBytes)
            {
                if (lastSoftWrapIndex > 0)
                {
                    // Soft wrap at previous good place
                    string chunk = currentChunk.ToString(0, lastSoftWrapIndex);
                    chunks.Add(chunk.TrimEnd());
                    
                    // Remove consumed portion
                    string remainder = currentChunk.ToString(lastSoftWrapIndex, currentChunk.Length - lastSoftWrapIndex);
                    currentChunk.Clear();
                    currentChunk.Append(remainder);

                    currentBytes = Encoding.UTF8.GetByteCount(currentChunk.ToString());
                }
                else
                {
                    // Hard wrap: split currentChunk before it overflows
                    string chunk = TruncateUtf8(currentChunk.ToString(), maxBytes);
                    chunks.Add(chunk);

                    string remainder = currentChunk.ToString(chunk.Length, currentChunk.Length - chunk.Length);
                    currentChunk.Clear();
                    currentChunk.Append(remainder);
                    currentBytes = Encoding.UTF8.GetByteCount(currentChunk.ToString());
                }

                // Reset soft wrap tracker
                lastSoftWrapIndex = -1;
            }
        }

        if (currentChunk.Length > 0)
            chunks.Add(currentChunk.ToString());

        return chunks;
    }

    // Safely truncate a string so that its UTF-8 byte length is <= maxBytes
    private static string TruncateUtf8(string input, int maxBytes)
    {
        int byteCount = 0;
        StringBuilder builder = new StringBuilder();

        foreach (char c in input)
        {
            int charBytes = Encoding.UTF8.GetByteCount(new[] { c });
            if (byteCount + charBytes > maxBytes)
                break;

            builder.Append(c);
            byteCount += charBytes;
        }

        return builder.ToString();
    }
}