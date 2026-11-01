using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Html_Serializer
{
    public class Selector
    {
        public string? TagName { get; set; }
        public string? Id { get; set; }
        public List<string> Classes { get; set; }
        public Selector? Parent { get; set; }
        public Selector? Child { get; set; }

        public Selector()
        {
            TagName = null;
            Id = null;
            Classes = new List<string>();
            Parent = null;
            Child = null;
        }

        // ממירה רמה אחת של שאילתה (בלי רווחים) לאוביקט Selector
        public static Selector ParseSimple(string query)
        {
            Selector selector = new Selector();

            // פירוק לפי המפרידים # ו-. (נקודה)
            var parts = Regex.Matches(query, @"(^[^#\.]+)|([#\.][^#\.]+)");

            foreach (Match part in parts)
            {
                string value = part.Value;

                if (value.StartsWith("#"))
                {
                    selector.Id = value.Substring(1);
                }
                else if (value.StartsWith("."))
                {
                    selector.Classes.Add(value.Substring(1));
                }
                else
                {
                    // רק אם זה שם תקין של תגית HTML - שמים אותו ב-TagName
                    string tagName = value.ToLower();

                    if (HtmlHelper.Instance.HtmlTags.Contains(tagName))
                    {
                        selector.TagName = tagName;
                    }
                }
            }

            return selector;
        }

        // ממירה מחרוזת שאילתה לשרשרת של סלקטורים (אבא-בן) ומחזירה את השורש
        public static Selector? Parse(string query)
        {
            string[] parts = query.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
            );

            Selector? root = null;
            Selector? current = null;

            foreach (string part in parts)
            {
                Selector newSelector = ParseSimple(part);

                if (root == null)
                {
                    root = newSelector;
                }
                else
                {
                    current!.Child = newSelector;
                    newSelector.Parent = current;
                }

                current = newSelector;
            }

            return root;
        }
    }
}
