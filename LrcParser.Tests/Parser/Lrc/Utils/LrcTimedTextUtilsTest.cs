// Copyright (c) karaoke.dev <contact@karaoke.dev>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using LrcParser.Parser.Lrc.Utils;
using LrcParser.Tests.Helper;
using NUnit.Framework;

namespace LrcParser.Tests.Parser.Lrc.Utils;

public class LrcTimedTextUtilsTest
{
    #region Decode

    [TestCase("<00:17.97>帰<00:18.37>り<00:18.55>道<00:18.94>は<00:19.22>", "帰り道は", new[] { "[0,start]:17970", "[1,start]:18370", "[2,start]:18550", "[3,start]:18940", "[3,end]:19220" })]
    [TestCase(" <00:17.97>帰<00:18.37>り<00:18.55>道<00:18.94>は<00:19.22>", "帰り道は", new[] { "[0,start]:17970", "[1,start]:18370", "[2,start]:18550", "[3,start]:18940", "[3,end]:19220" })]
    [TestCase("<00:17.97>帰<00:18.37>り<00:18.55>道<00:18.94>は<00:19.22> ", "帰り道は", new[] { "[0,start]:17970", "[1,start]:18370", "[2,start]:18550", "[3,start]:18940", "[3,end]:19220" })]
    [TestCase("帰<00:18.37>り<00:18.55>道<00:18.94>は<00:19.22>", "帰り道は", new[] { "[0,start]:0", "[1,start]:18370", "[2,start]:18550", "[3,start]:18940", "[3,end]:19220" })]
    [TestCase("<00:17.97>帰<00:18.37>り<00:18.55>道<00:18.94>は", "帰り道は", new[] { "[0,start]:17970", "[1,start]:18370", "[2,start]:18550", "[3,start]:18940" })]
    [TestCase("帰り道は", "帰り道は", new string[] { })]
    [TestCase("", "", new string[] { })]
    [TestCase("   ", "", new string[] { })]
    [TestCase(null, "", new string[] { })]
    [TestCase("<00:51.00> <01:29.99><01:48.29>  <02:31.00> <02:41.99>You gotta fight !", "You gotta fight !", new[] { "[0,start]:161990" })] // multiple empty tags
    // Surrounding time tags
    [TestCase(
        "<00:06.84> Every <00:07.20>   <00:07.56> night <00:07.87>   <00:08.19> that <00:08.46>   <00:08.79> goes <00:09.19>   <00:09.59> between",
        "Every night that goes between",
        new[] { "[0,start]:6840", "[4,end]:7200", "[6,start]:7560", "[10,end]:7870", "[12,start]:8190", "[15,end]:8460", "[17,start]:8790", "[20,end]:9190", "[22,start]:9590" }
    )]
    // Alternating time tags, spaced on both sides
    [TestCase(
        "<00:06.84> Every <00:07.56> night <00:08.19> that <00:08.79> goes <00:09.59> between", "Every night that goes between",
        new[] { "[0,start]:6840", "[6,start]:7560", "[12,start]:8190", "[17,start]:8790", "[22,start]:9590" }
    )]
    // Alternating time tags, unspaced
    [TestCase(
        "<00:06.84>Every<00:07.56>night<00:08.19>that<00:08.79>goes<00:09.59>between", "Everynightthatgoesbetween",
        new[] { "[0,start]:6840", "[5,start]:7560", "[10,start]:8190", "[14,start]:8790", "[18,start]:9590" }
    )]
    [TestCase(
        "Every<00:07.56>night<00:08.19>that<00:08.79>goes<00:09.59>between", "Everynightthatgoesbetween",
        new[] { "[0,start]:0", "[5,start]:7560", "[10,start]:8190", "[14,start]:8790", "[18,start]:9590" }
    )]
    // Alternating time tags, prefix spaced
    [TestCase(
        "<00:06.84> Every<00:07.56> night<00:08.19> that<00:08.79> goes<00:09.59> between", "Every night that goes between",
        new[] { "[0,start]:6840", "[6,start]:7560", "[12,start]:8190", "[17,start]:8790", "[22,start]:9590" }
    )]
    [TestCase(
        "Every<00:07.56> night<00:08.19> that<00:08.79> goes<00:09.59> between", "Every night that goes between",
        new[] { "[0,start]:0", "[6,start]:7560", "[12,start]:8190", "[17,start]:8790", "[22,start]:9590" }
    )]
    // Alternating time tags, postfix spaced
    [TestCase(
        "<00:06.84>Every <00:07.56>night <00:08.19>that <00:08.79>goes <00:09.59>between", "Every night that goes between",
        new[] { "[0,start]:6840", "[6,start]:7560", "[12,start]:8190", "[17,start]:8790", "[22,start]:9590" }
    )]
    [TestCase(
        "Every <00:07.56>night <00:08.19>that <00:08.79>goes <00:09.59>between", "Every night that goes between",
        new[] { "[0,start]:0", "[6,start]:7560", "[12,start]:8190", "[17,start]:8790", "[22,start]:9590" }
    )]
    public void TestDecode(string text, string expectedText, string[] expectedTimeTags)
    {
        var (actualText, actualTimeTags) = LrcTimedTextUtils.TimedTextToObject(text, 0);

        Assert.That(actualText, Is.EqualTo(expectedText));
        Assert.That(actualTimeTags, Is.EqualTo(TestCaseTagHelper.ParseTimeTags(expectedTimeTags)));
    }

    [TestCase(
        "<00:06.84>Every<00:07.56>night<00:08.19>that<00:08.79>goes<00:09.59>between", 6840, "Everynightthatgoesbetween",
        new[] { "[0,start]:6840", "[5,start]:7560", "[10,start]:8190", "[14,start]:8790", "[18,start]:9590" }
    )]
    [TestCase(
        "Every<00:07.56>night<00:08.19>that<00:08.79>goes<00:09.59>between", 6840, "Everynightthatgoesbetween",
        new[] { "[0,start]:6840", "[5,start]:7560", "[10,start]:8190", "[14,start]:8790", "[18,start]:9590" }
    )]
    public void TestDecodeWithStartTime(string text, int lineStartTime, string expectedText, string[] expectedTimeTags)
    {
        var (actualText, actualTimeTags) = LrcTimedTextUtils.TimedTextToObject(text, lineStartTime);

        Assert.That(actualText, Is.EqualTo(expectedText));
        Assert.That(actualTimeTags, Is.EqualTo(TestCaseTagHelper.ParseTimeTags(expectedTimeTags)));
    }

    #endregion

    #region Encode

    [TestCase("帰り道は", new[] { "[0,start]:17970", "[1,start]:18370", "[2,start]:18550", "[3,start]:18940", "[3,end]:19220" }, "<00:17.97>帰<00:18.37>り<00:18.55>道<00:18.94>は<00:19.22>")]
    [TestCase(" 帰り道は", new[] { "[1,start]:17970", "[2,start]:18370", "[3,start]:18550", "[4,start]:18940", "[4,end]:19220" }, " <00:17.97>帰<00:18.37>り<00:18.55>道<00:18.94>は<00:19.22>")]
    [TestCase("帰り道は ", new[] { "[0,start]:17970", "[1,start]:18370", "[2,start]:18550", "[3,start]:18940", "[3,end]:19220" }, "<00:17.97>帰<00:18.37>り<00:18.55>道<00:18.94>は<00:19.22> ")]
    [TestCase("帰り道は", new[] { "[1,start]:18370", "[2,start]:18550", "[3,start]:18940", "[3,end]:19220" }, "帰<00:18.37>り<00:18.55>道<00:18.94>は<00:19.22>")]
    [TestCase("帰り道は", new[] { "[0,start]:17970", "[1,start]:18370", "[2,start]:18550", "[3,start]:18940" }, "<00:17.97>帰<00:18.37>り<00:18.55>道<00:18.94>は")]
    [TestCase("帰り道は", new string[] { }, "帰り道は")]
    [TestCase("", new string[] { }, "")]
    public void TestEncode(string text, string[] timeTags, string expected)
    {
        var actual = LrcTimedTextUtils.ToTimedText(text, TestCaseTagHelper.ParseTimeTags(timeTags));

        Assert.That(actual, Is.EqualTo(expected));
    }

    #endregion
}
