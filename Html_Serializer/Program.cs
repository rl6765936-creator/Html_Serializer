using Html_Serializer;

string url = args.Length > 0 ? args[0] : "https://www.w3.org/";

HtmlSerializer serializer = new HtmlSerializer();
HtmlElement root = await serializer.Serialize(url);

string[] queries =
{
    "header nav a",
    "div",
    "ul li a",
    "footer a",
};

foreach (string query in queries)
{
    PrintResults(root, query);
}

// מריצה שאילתה על העץ ומדפיסה כמה אלמנטים נמצאו
static void PrintResults(HtmlElement root, string query)
{
    Selector? selector = Selector.Parse(query);

    if (selector == null)
    {
        return;
    }

    List<HtmlElement> results = root.Find(selector);

    Console.WriteLine($"{query} -> {results.Count}");
}
