namespace Html_Serializer
{
    public static class HtmlElementExtensions
    {
        // מחזירה את כל האלמנטים בעץ שעונים לסלקטור, בלי כפילויות
        public static List<HtmlElement> Find(this HtmlElement element, Selector selector)
        {
            HashSet<HtmlElement> results = new HashSet<HtmlElement>();

            FindRecursive(element, selector, results);

            return results.ToList();
        }

        // מחפשת ברקורסיה בצאצאים לפי הסלקטור הנוכחי, וממשיכה לסלקטור הבא עד האחרון
        private static void FindRecursive(HtmlElement element, Selector selector, HashSet<HtmlElement> results)
        {
            // Descendants() מחזירה גם את האלמנט עצמו, ולכן מדלגים עליו עם Skip(1):
            // מחפשים רק בצאצאים, כדי שאותו אלמנט לא יתאים לשתי רמות של הסלקטור
            var matches = element.Descendants()
                .Skip(1)
                .Where(e => MatchesSelector(e, selector))
                .ToList();

            if (selector.Child == null)
            {
                foreach (HtmlElement match in matches)
                {
                    results.Add(match);
                }

                return;
            }

            foreach (HtmlElement match in matches)
            {
                FindRecursive(match, selector.Child, results);
            }
        }

        // בודקת אם האלמנט מתאים לשם התגית, ל-id ול-classes של הסלקטור
        private static bool MatchesSelector(HtmlElement element, Selector selector)
        {
            if (selector.TagName != null && element.Name != selector.TagName)
            {
                return false;
            }

            if (selector.Id != null && element.Id != selector.Id)
            {
                return false;
            }

            if (!selector.Classes.All(c => element.Classes.Contains(c)))
            {
                return false;
            }

            return true;
        }
    }
}
