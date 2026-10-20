using System.Globalization;
using Designer.Modules.IconsFilter;

// Compile the production IconQuery.cs directly. These are owned examples, not
// a captured icon catalog or a replacement implementation of the matcher.
var suite = new Suite();

suite.Group("normalization", () =>
{
    (string Input, string Expected)[] cases =
    [
        ("", ""),
        (" \t\r\n ", ""),
        ("---_./:!?()[]{}", ""),
        ("Microphone", "microphone"),
        ("VOLUME Strike", "volume strike"),
        ("  Play   Pause  ", "play pause"),
        ("Fast-Forward", "fast forward"),
        ("Play/Pause", "play pause"),
        ("X_Circle", "x circle"),
        ("Page.Search", "page search"),
        ("A1 + B20", "a1 b20"),
        ("channel\t2\r\nleft", "channel 2 left"),
        ("speaker\u00a0audio\u2003sound", "speaker audio sound"),
        ("left\u0000right", "left right"),
        ("Volume’s", "volume s"),
        ("CAFÉ", "café"),
        ("ÅNGSTRÖM", "ångström"),
        ("ΣΗΜΑ", "σημα"),
        ("音量 １２", "音量 １２"),
        // No accent folding, Unicode composition or supplementary-code-point
        // conversion is promised by the current char-based normalization.
        ("cafe\u0301", "cafe"),
        ("A🔊B", "a b"),
    ];
    foreach (var (input, expected) in cases)
    {
        suite.Equal(expected, IconQuery.Normalize(input), $"normalize {Show(input)}");
        suite.Equal(expected, IconQuery.Normalize(expected), $"idempotent {Show(input)}");
    }

    string longInput = "  " + new string('A', 8192) + "\t---\n" + new string('9', 8192);
    suite.Equal(new string('a', 8192) + " " + new string('9', 8192),
        IconQuery.Normalize(longInput), "long input");
});

suite.Group("name fragments and all query terms", () =>
{
    (string Name, string Query, bool Expected)[] cases =
    [
        ("Projection Screen", "projection screen", true),
        ("Projection Screen", "screen projection", true),
        ("Projection Screen", "proj scr", true),
        ("Projection Screen", "SCREEN--proj", true),
        ("Projection Screen", "screen screen", true),
        ("Projection Screen", "screen missing", false),
        ("Projection Screen", "missing", false),
        ("Projection Screen", "projectionscreen", false),
        ("Projection Screen", "screenish", false),
        ("Stage 12 Left", "12 sta lef", true),
        ("Stage 12 Left", "21", false),
        ("Stage 12 Left", "right", false),
        ("Alpha-Beta", "beta alpha", true),
        ("Alpha-Beta", "alphabeta", false),
        ("Unknown Name", "known", true), // Fragments need not be whole words.
        ("Unknown Name", "settings", false),
        ("CAFÉ", "café", true),
        ("CAFÉ", "cafe", false),
        ("cafe\u0301", "café", false),
        ("音量 １２", "１２ 音", true),
        ("音量 １２", "12", false),
        ("", "", true),
        ("", "--- \t", true),
        ("", "speaker", false),
        ("Custom", "custom", true), // Matching a label does not perform import.
        ("Custom", "image", false),
        ("Any Name", "", true),
        ("Any Name", "\r\n", true),
        ("Any Name", "🛠️", true), // No remaining letter/digit terms.
        ("Any Name", "### missing !!!", false),
    ];
    foreach (var (name, query, expected) in cases)
        suite.Equal(expected, IconQuery.Matches(name, query),
            $"{Show(name)} matches {Show(query)}");

    suite.Equal(true, IconQuery.Matches(new string('x', 8192), new string('x', 8192)),
        "long exact name");
    suite.Equal(false, IconQuery.Matches(new string('x', 8192), new string('x', 8193)),
        "query fragment longer than name");
});

suite.Group("search-only alias vocabulary", () =>
{
    // Each row is an explicit expected example. Combined queries require every
    // listed alias term and catch omissions without reading the private table.
    (string Name, string Query)[] cases =
    [
        ("Microphone", "mic voice input"),
        ("Microphone None", "mic mute muted off"),
        ("Volume", "speaker audio sound"),
        ("Volume None", "speaker audio sound quiet silent zero"),
        ("Volume Strike", "speaker audio sound mute muted off"),
        ("Widget", "settings gear cog preferences configuration"),
        ("Wrench", "tools setup maintenance"),
        ("Magnifying Glass", "search find magnifier"),
        ("Page Search", "document find"),
        ("Alert", "warning caution attention error"),
        ("Info", "information help"),
        ("X", "close cancel dismiss remove"),
        ("X Circle", "close cancel dismiss error"),
        ("Trash", "delete bin recycle"),
        ("Page Delete", "document remove"),
        ("Check", "tick checkmark confirm done success"),
        ("Checkbox", "tick checkmark checked"),
        ("Prohibited", "blocked forbidden disabled ban"),
        ("Power", "on off standby shutdown"),
        ("Home", "house main dashboard"),
        ("Link", "chain hyperlink url"),
        ("Unlink", "broken chain disconnect hyperlink"),
        ("Lock", "locked secure padlock"),
        ("Unlock", "unlocked padlock"),
        ("Loop", "sync synchronize repeat reload"),
        ("Refresh", "reload reset"),
        ("Rewind", "backward reverse"),
        ("Fast Forward", "fastforward"),
        ("Next", "skip next track"),
        ("Previous", "back skip previous track"),
        ("Play", "playback start"),
        ("Play Circle", "playback start"),
        ("Play Pause", "playback toggle"),
        ("Play Video", "playback start"),
        ("Pause", "playback"),
        ("Stop", "playback"),
        ("Record", "recording capture"),
        ("Projection Screen", "projector presentation"),
        ("Monitor", "screen display"),
        ("Laptop", "computer notebook"),
        ("Mobile", "phone smartphone"),
        ("Mobile Signal", "phone smartphone wireless"),
        ("Telephone", "phone call"),
        ("Telephone In", "phone incoming call"),
        ("Telephone Out", "phone outgoing call"),
        ("Telephone Down", "phone hangup end call"),
        ("Camera", "photo photograph snapshot"),
        ("Video", "camera movie film"),
        ("Photo", "image picture photograph"),
        ("Mail", "email envelope message"),
    ];
    foreach (var (name, query) in cases)
    {
        suite.Equal(true, IconQuery.Matches(name, query), $"aliases for {name}");
        suite.Equal(true, IconQuery.Matches(name.ToUpperInvariant(), query.ToUpperInvariant()),
            $"case-insensitive aliases for {name}");
        suite.Equal(true, IconQuery.Matches(name, name + " / " + query),
            $"native name and aliases for {name}");
        suite.Equal(false, IconQuery.Matches(name, query + " impossibleterm987"),
            $"reject missing extra term for {name}");
    }
    suite.Equal(true, IconQuery.Matches("Volume Strike", "vol mut aud"), "mixed name/alias fragments");
    suite.Equal(true, IconQuery.Matches("Widget", "GEAR--pref"), "alias punctuation and fragments");
    suite.Equal(true, IconQuery.Matches("Telephone In", "phone IN"), "mixed alias and native term");
    suite.Equal(false, IconQuery.Matches("Camera", "picture"), "aliases do not recursively expand Photo");
    suite.Equal(false, IconQuery.Matches("Video", "snapshot"), "aliases do not recursively expand Camera");
});

suite.Group("state-specific aliases and exact alias keys", () =>
{
    (string Name, string Query)[] rejected =
    [
        ("Microphone", "mute"),
        ("Microphone", "off"),
        ("Microphone None", "voice"),
        ("Volume", "mute"),
        ("Volume", "quiet"),
        ("Volume", "off"),
        ("Volume None", "mute"),
        ("Volume None", "muted"),
        ("Volume None", "off"),
        ("Volume Strike", "quiet"),
        ("Volume Strike", "silent"),
        ("Volume Strike", "zero"),
        ("Play Pause", "start"),
        ("Pause", "start"),
        ("Mobile", "wireless"),
        ("Telephone In", "outgoing"),
        ("Telephone Out", "incoming"),
        ("Telephone Down", "incoming"),
        ("Page Delete", "recycle"),
        ("Widget", "sound"),
        // Alias lookup is exact except for case; name matching still normalizes.
        ("Microphone!", "voice"),
        (" Microphone", "voice"),
        ("Microphone ", "voice"),
        ("Microphone  None", "muted"),
        ("Volume-Strike", "muted"),
        ("Custom Widget", "gear"),
        ("Microphone Extended", "voice"),
    ];
    foreach (var (name, query) in rejected)
        suite.Equal(false, IconQuery.Matches(name, query), $"reject {Show(query)} for {Show(name)}");
    suite.Equal(true, IconQuery.Matches("Volume-Strike", "volume strike"),
        "punctuated name remains searchable without aliases");
    suite.Equal(true, IconQuery.Matches(" Microphone ", "microphone"),
        "padded name remains searchable without aliases");
});

suite.Group("culture independence and invalid arguments", () =>
{
    CultureInfo previous = CultureInfo.CurrentCulture;
    CultureInfo previousUi = CultureInfo.CurrentUICulture;
    try
    {
        foreach (string name in new[] { "en-US", "tr-TR", "fr-FR" })
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name);
            suite.Equal("input info", IconQuery.Normalize("INPUT INFO"), $"invariant lowercasing under {name}");
            suite.Equal(true, IconQuery.Matches("mIcRoPhOnE", "INPUT"), $"alias key under {name}");
            suite.Equal(true, IconQuery.Matches("INFO", "INFORMATION"), $"alias query under {name}");
            suite.Equal(false, IconQuery.Matches("Volume None", "MUTED"), $"state exclusion under {name}");
        }
    }
    finally
    {
        CultureInfo.CurrentCulture = previous;
        CultureInfo.CurrentUICulture = previousUi;
    }
    // The production signatures are non-nullable. Record rejection rather than
    // silently treating an invalid caller argument as an empty/match-all query.
    suite.Throws<ArgumentNullException>(() => IconQuery.Normalize(null!), "null normalization input");
    suite.Throws<ArgumentNullException>(() => IconQuery.Matches(null!, "mic"), "null icon name");
    suite.Throws<ArgumentNullException>(() => IconQuery.Matches("Microphone", null!), "null query");
});

suite.Group("synthetic catalog query results", () =>
{
    // Unique glyph strings are synthetic owned markers, not vendor glyph data.
    var icons = new[]
    {
        new Icon("Volume", "owned:glyph:1"),
        new Icon("Volume None", "owned:glyph:2"),
        new Icon("Volume Strike", "owned:glyph:3"),
        new Icon("mIcRoPhOnE", "owned:glyph:4"),
        new Icon("Widget", "owned:glyph:5"),
        new Icon("Volume Strike", "owned:glyph:6"),
        new Icon("", "owned:glyph:7"),
        new Icon("Custom", "owned:glyph:8"),
    };
    (string Query, int[] Expected)[] cases =
    [
        ("speaker", [0, 1, 2, 5]),
        ("muted", [2, 5]),
        ("quiet", [1]),
        ("voice input", [3]),
        ("gear", [4]),
        ("custom", [7]),
        ("missing987", []),
        ("speaker missing987", []),
        ("", [0, 1, 2, 3, 4, 5, 6, 7]),
        ("---", [0, 1, 2, 3, 4, 5, 6, 7]),
        ("mute", [2, 5]),
        ("quiet", [1]), // A prior search must not accumulate alias/query state.
    ];
    foreach (var (query, expected) in cases)
    {
        var actual = icons.Where(icon => IconQuery.Matches(icon.Name, query)).ToArray();
        suite.Equal(expected.Length, actual.Length, $"result count for {Show(query)}");
        for (int i = 0; i < Math.Min(expected.Length, actual.Length); i++)
            suite.Equal(true, ReferenceEquals(icons[expected[i]], actual[i]),
                $"original result identity/order {i} for {Show(query)}");
    }
});

return suite.Finish();

static string Show(string text) => System.Text.Json.JsonSerializer.Serialize(text);

sealed record Icon(string Name, string Glyph);

sealed class Suite
{
    private int checks;
    private int failures;

    public void Group(string name, Action test)
    {
        int beforeChecks = checks;
        int beforeFailures = failures;
        try { test(); }
        catch (Exception error)
        {
            failures++;
            Console.Error.WriteLine($"FAIL {name}: unexpected {error}");
        }
        Console.WriteLine($"{(beforeFailures == failures ? "PASS" : "FAIL")} {name} ({checks - beforeChecks} checks)");
    }

    public void Equal<T>(T expected, T actual, string description)
    {
        checks++;
        if (EqualityComparer<T>.Default.Equals(expected, actual)) return;
        failures++;
        Console.Error.WriteLine($"FAIL {description}: expected {expected}, got {actual}");
    }

    public void Throws<T>(Action action, string description) where T : Exception
    {
        checks++;
        try { action(); }
        catch (T) { return; }
        catch (Exception error)
        {
            failures++;
            Console.Error.WriteLine($"FAIL {description}: expected {typeof(T).Name}, got {error.GetType().Name}");
            return;
        }
        failures++;
        Console.Error.WriteLine($"FAIL {description}: expected {typeof(T).Name}, no exception thrown");
    }

    public int Finish()
    {
        Console.WriteLine($"{checks} checks, {failures} failures");
        return failures == 0 ? 0 : 1;
    }
}
