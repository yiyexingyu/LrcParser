// Copyright (c) karaoke.dev <contact@karaoke.dev>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.RegularExpressions;
using static LrcParser.Parser.Lrc.Utils.TimeTagMode;

namespace LrcParser.Parser.Lrc.Utils;

public static class LrcStartTimeUtils
{
    /// <summary>
    /// Check if the line starts with a line time tag.
    /// </summary>
    /// <param name="line">The lyrics line in the LRC format.</param>
    /// <returns>
    /// <c>true</c> if the line starts with a line time tag, <c>false</c> otherwise.
    /// </returns>
    public static bool StartsWithLineTimeTag(string line)
    {
        var match = TimeTagUtils.LINE_TIME_TAG_REGEX.Match(line);
        return match.Success && match.Index == 0;
    }

    /// <summary>
    /// Split an LRC lyrics line into parsed line time tags and the lyric text.
    /// </summary>
    /// <param name="line">The lyrics line in the LRC format.</param>
    /// <returns>
    /// A tuple of the form <c>(startTimes, text)</c> where <c>startTimes</c> is an array of start times in milliseconds,
    /// and <c>text</c> is the lyric text without time tags.
    /// </returns>
    internal static Tuple<int[], string> SplitLyricAndTimeTag(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return new Tuple<int[], string>([], string.Empty);

        // get all matched startTime
        MatchCollection matches = TimeTagUtils.LINE_TIME_TAG_REGEX.Matches(line);

        var startTimes = matches.OfType<Match>().Select(x => TimeTagUtils.ConvertTimeTagToMilliseconds(x.Value, LineTimeTag)).ToArray();
        var lyric = TimeTagUtils.LINE_TIME_TAG_REGEX.Replace(line, "").Trim();

        return new Tuple<int[], string>(startTimes, lyric);
    }

    /// <summary>
    /// Combine the lyric format from:
    /// [60000, 66000]
    /// When the truth is found to be lies
    /// to:
    /// [01:00.00][01:06.00] When the truth is found to be lies
    /// </summary>
    /// <param name="startTimes"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    internal static string JoinLyricAndTimeTag(int[] startTimes, string text)
    {
        if (startTimes.Any() == false)
            throw new InvalidOperationException("Missing one or more start times.");

        if (TimeTagUtils.LINE_TIME_TAG_REGEX.Matches(text).OfType<Match>().Any() == false)
            throw new InvalidOperationException("lyric should not contain any line time tags.");

        if (startTimes.Length == 0)
            return text;

        var result = startTimes.Aggregate(string.Empty, (current, t) =>
            current + TimeTagUtils.ConvertMillisecondsToTimeTag(t, LineTimeTag)
        );

        return result + " " + text.Trim();
    }
}
