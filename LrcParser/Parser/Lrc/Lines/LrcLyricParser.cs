// Copyright (c) karaoke.dev <contact@karaoke.dev>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using LrcParser.Model;
using LrcParser.Parser.Lines;
using LrcParser.Parser.Lrc.Metadata;
using LrcParser.Parser.Lrc.Utils;

namespace LrcParser.Parser.Lrc.Lines;

public class LrcLyricParser : SingleLineParser<LrcLyric>
{
    public override bool CanDecode(string text)
        => !string.IsNullOrWhiteSpace(text);

    public override LrcLyric Decode(string text)
    {
        var (startTimes, rawLyric) = LrcStartTimeUtils.SplitLyricAndTimeTag(text);

        // If there are multiple start times, it is possible that the given word time tags are incompatible with one or more start times.
        // For example, if a line starts at both [01:00.00] and [02:00.00],
        // word time tags with the values <01:10.00> and <01:20.00> would be incompatible with the second start time.
        // As there isn't an official LRC spec, this isn't clearly defined.
        // While the format is technically valid, we chose a reasonable behavior of ignoring the word time tags in this case
        // and returning the line as-is without parsing the word time tags.
        // The same applies to lines that have no start times:
        // As there might be lines like `Every <00:07.56> night`, the first word would not have a start time,
        // so we chose the same approach of ignoring the word time tags in this case.
        if (startTimes.Length is 0 or > 1)
        {
            return new LrcLyric
            {
                Text = rawLyric,
                StartTimes = startTimes,
                TimeTags = new SortedDictionary<TextIndex, int>()
            };
        }

        var (lyric, timeTags) = LrcTimedTextUtils.TimedTextToObject(rawLyric, startTimes[0]);

        return new LrcLyric
        {
            Text = lyric,
            StartTimes = startTimes,
            TimeTags = timeTags,
        };
    }

    public override string Encode(LrcLyric component, int index)
    {
        var lyricWithTimeTag = LrcTimedTextUtils.ToTimedText(component.Text, component.TimeTags);
        return LrcStartTimeUtils.JoinLyricAndTimeTag(component.StartTimes, lyricWithTimeTag);
    }
}
