// Copyright (c) karaoke.dev <contact@karaoke.dev>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using LrcParser.Parser.Lrc.Lines;
using LrcParser.Parser.Lrc.Metadata;
using LrcParser.Tests.Helper;
using LrcParser.Tests.Parser.Lines;
using NUnit.Framework;

namespace LrcParser.Tests.Parser.Lrc.Lines;

public class LrcLyricParserTest : BaseSingleLineParserTest<LrcLyricParser, LrcLyric>
{
    [TestCase("[00:17.97]帰[00:18.37]り[00:18.55]道[00:18.94]は[00:19.22]", true)]
    [TestCase("[00:17.97]<00:00.00>帰<00:00.00>り<00:00.00>道<00:00.00>は<00:00.00>", true)]
    [TestCase("[00:17.97]", true)] // only start time-tag can be decode.
    [TestCase("[00:17:97]", true)] // invalid time-tag might be decoded as lyric string with no time-tag info.
    [TestCase("karaoke", true)] // string might be parsed into lyric without any time info for now.
    [TestCase("", false)]
    [TestCase(null, false)]
    public void TestCanDecode(string text, bool expected)
    {
        var actual = CanDecode(text);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(testDecodeSource))]
    public void TestDecode(string lyric, LrcLyric expected)
    {
        var actual = Decode(lyric);

        Assert.That(actual, Is.EqualTo(expected));
    }

    private static IEnumerable<object[]> testDecodeSource => new object[][]
    {
        [
            "[00:17.00] <00:00.00>帰<00:01.00>り<00:02.00>道<00:03.00>は<00:04.00>",
            new LrcLyric
            {
                Text = "帰り道は",
                StartTimes = [17000],
                TimeTags = TestCaseTagHelper.ParseTimeTags(["[0,start]:0", "[1,start]:1000", "[2,start]:2000", "[3,start]:3000", "[3,end]:4000"]),
            },
        ],
        [
            "[00:17.00] 帰<00:01.00>り<00:02.00>道<00:03.00>は<00:04.00>",
            new LrcLyric
            {
                Text = "帰り道は",
                StartTimes = [17000],
                // [0,start]:17000 is created from the line time tag
                TimeTags = TestCaseTagHelper.ParseTimeTags(["[0,start]:17000", "[1,start]:1000", "[2,start]:2000", "[3,start]:3000", "[3,end]:4000"]),
            },
        ],
        [
            "[00:17.00] <00:00.00>帰<00:01.00>り<00:02.00>道<00:03.00>は",
            new LrcLyric
            {
                Text = "帰り道は",
                StartTimes = [17000],
                TimeTags = TestCaseTagHelper.ParseTimeTags(["[0,start]:0", "[1,start]:1000", "[2,start]:2000", "[3,start]:3000"]),
            },
        ],
        [
            "[00:17.00] 帰り道は",
            new LrcLyric
            {
                Text = "帰り道は",
                StartTimes = [17000],
                TimeTags = [],
            },
        ],
        [
            "[00:17.00][00:18.00] 帰り道は",
            new LrcLyric
            {
                Text = "帰り道は",
                StartTimes = [17000, 18000],
                TimeTags = [],
            },
        ],
        [
            "帰り道は",
            new LrcLyric
            {
                Text = "帰り道は",
                StartTimes = [],
                TimeTags = [],
            },
        ],
        [
            "",
            new LrcLyric
            {
                Text = "",
                TimeTags = [],
            },
        ],
        [
            null!,
            new LrcLyric
            {
                Text = "",
                TimeTags = [],
            },
        ],
        // Don't parse word time tags if multiple line time tags are found, as this is unsupported by LRC.
        // Instead, return the unparsed line without the line time and no word time tags.
        [
            "[00:17.00][00:18.00] <00:00.00>帰<00:01.00>り<00:02.00>道<00:03.00>は",
            new LrcLyric
            {
                Text = "<00:00.00>帰<00:01.00>り<00:02.00>道<00:03.00>は",
                StartTimes = [17000, 18000],
                TimeTags = [],
            },
        ],
    };

    [TestCaseSource(nameof(testEncodeSource))]
    public void TestEncode(LrcLyric lyric, string expected)
    {
        if (string.IsNullOrEmpty(expected))
        {
            Assert.That(() => Encode(lyric), Throws.InvalidOperationException);
        }
        else
        {
            var actual = Encode(lyric);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    private static IEnumerable<object[]> testEncodeSource => new object[][]
    {
        [
            new LrcLyric
            {
                Text = "帰り道は",
                StartTimes = [17000],
                TimeTags = TestCaseTagHelper.ParseTimeTags(["[0,start]:0", "[1,start]:1000", "[2,start]:2000", "[3,start]:3000", "[3,end]:4000"]),
            },
            "[00:17.00] <00:00.00>帰<00:01.00>り<00:02.00>道<00:03.00>は<00:04.00>",
        ],
        [
            new LrcLyric
            {
                Text = "帰り道は",
                StartTimes = [17000],
                TimeTags = TestCaseTagHelper.ParseTimeTags(["[1,start]:1000", "[2,start]:2000", "[3,start]:3000", "[3,end]:4000"]),
            },
            "[00:17.00] 帰<00:01.00>り<00:02.00>道<00:03.00>は<00:04.00>",
        ],
        [
            new LrcLyric
            {
                Text = "帰り道は",
                StartTimes = [17000],
                TimeTags = TestCaseTagHelper.ParseTimeTags(["[0,start]:0", "[1,start]:1000", "[2,start]:2000", "[3,start]:3000"]),
            },
            "[00:17.00] <00:00.00>帰<00:01.00>り<00:02.00>道<00:03.00>は",
        ],
        [
            new LrcLyric
            {
                Text = "帰り道は",
                StartTimes = [17000],
                TimeTags = [],
            },
            "[00:17.00] 帰り道は",
        ],
        [
            new LrcLyric
            {
                Text = "",
                TimeTags = [],
            },
            null!,
        ],
    };
}
