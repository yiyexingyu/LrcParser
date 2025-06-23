// Copyright (c) karaoke.dev <contact@karaoke.dev>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text;
using LrcParser.Model;
using LrcParser.Utils;
using static LrcParser.Parser.Lrc.Utils.TimeTagMode;

namespace LrcParser.Parser.Lrc.Utils;

internal static class LrcTimedTextUtils
{
    /// <summary>
    /// Parses the passed text for word time tags.
    /// </summary>
    /// <param name="timedText"></param>
    /// <param name="lineStartTime"></param>
    /// <returns></returns>
    internal static Tuple<string, SortedDictionary<TextIndex, int>> TimedTextToObject(string timedText, int lineStartTime)
    {
        if (string.IsNullOrWhiteSpace(timedText))
        {
            return new Tuple<string, SortedDictionary<TextIndex, int>>("", new SortedDictionary<TextIndex, int>());
        }

        var textLength = timedText.Length;
        var lyricText = new StringBuilder();
        var timeTags = new SortedDictionary<TextIndex, int>();

        var timeTagMatches = TimeTagUtils.WORD_TIME_TAG_REGEX.Matches(timedText);

        if (timeTagMatches.Count == 0)
        {
            // no word time tags, return lyric as-is
            return new Tuple<string, SortedDictionary<TextIndex, int>>(timedText, new SortedDictionary<TextIndex, int>());
        }

        var lastTimeTag = lineStartTime;
        var segmentStartIndex = 0;
        var insertSpace = false;
        var lastTagWasStartTag = false;

        foreach (var match in timeTagMatches.ToArray())
        {
            // Segment ends at the start of the next time tag
            var segmentEndIndex = match.Index;

            var segment = timedText[segmentStartIndex..segmentEndIndex];

            // Update next start index
            segmentStartIndex = segmentEndIndex + match.Length;

            if (string.IsNullOrWhiteSpace(segment))
            {
                // The last segment was a start tag, and the next segment is empty, insert end tag
                if (lastTagWasStartTag)
                {
                    timeTags.TryAdd(new TextIndex(lyricText.Length - 1, IndexState.End), lastTimeTag);
                    lastTagWasStartTag = false;
                }

                // Skip empty lyric, update start time
                lastTimeTag = TimeTagUtils.ConvertTimeTagToMilliseconds(match.Value, WordTimeTag);

                // Segment contains only whitespace but isn't empty, insert a space before an upcoming valid segment.
                if (segment.Length > 0) insertSpace = true;
                continue;
            }

            // If the last segment ended with whitespace, or the current starts with whitespace,
            // insert a single space before the next segment.
            if ((char.IsWhiteSpace(segment[0]) || insertSpace) && lyricText.Length > 0)
            {
                lyricText.Append(' ');
            }

            // Add start time tag for next lyric
            timeTags.TryAdd(new TextIndex(lyricText.Length), lastTimeTag);
            lastTagWasStartTag = true;

            // Append lyric segment without surrounding whitespace
            lyricText.Append(segment.Trim());

            // Update start time for the next segment
            lastTimeTag = TimeTagUtils.ConvertTimeTagToMilliseconds(match.Value, WordTimeTag);

            // Reset insertSpace flag after adding a segment,
            // and instead track whether this new segment ends with whitespace
            insertSpace = char.IsWhiteSpace(segment[^1]);
        }

        var remaining = timedText[segmentStartIndex..textLength];

        if (!string.IsNullOrWhiteSpace(remaining))
        {
            if ((char.IsWhiteSpace(remaining[0]) || insertSpace) && lyricText.Length > 0)
            {
                // Add space before the next segment
                lyricText.Append(' ');
            }

            // Add remaining text with start time tag
            timeTags.TryAdd(new TextIndex(lyricText.Length), lastTimeTag);
            lyricText.Append(remaining.Trim());
        }
        else
        {
            // No remaining text, last time tag was end tag
            timeTags.TryAdd(new TextIndex(lyricText.Length - 1, IndexState.End), lastTimeTag);
        }

        return new Tuple<string, SortedDictionary<TextIndex, int>>(lyricText.ToString(), timeTags);
    }

    internal static string ToTimedText(string text, SortedDictionary<TextIndex, int> timeTags)
    {
        var insertIndex = 0;

        var timedText = text;

        foreach (var (textIndex, time) in timeTags)
        {
            var timeTagString = TimeTagUtils.ConvertMillisecondsToTimeTag(time, WordTimeTag);
            var stringIndex = TextIndexUtils.ToGapIndex(textIndex);
            timedText = timedText.Insert(insertIndex + stringIndex, timeTagString);

            insertIndex += timeTagString.Length;
        }

        return timedText;
    }
}
