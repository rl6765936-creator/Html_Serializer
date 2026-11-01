using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Html_Serializer
{
    // שירות שניגש לכתובת של דף אינטרנט, קורא את ה-HTML וממיר אותו לעץ של אוביקטי HtmlElement
    public class HtmlSerializer
    {
        private static readonly HttpClient _client = new HttpClient();

        // קוראת לכתובת אינטרנט ומחזירה את ה-HTML כמחרוזת
        public async Task<string> Load(string url)
        {
            var response = await _client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();
            return html;
        }

        // טוענת דף אינטרנט, ממירה אותו לעץ ומחזירה את שורש העץ
        public async Task<HtmlElement> Serialize(string url)
        {
            string html = await Load(url);
            return Parse(html);
        }

        // ממירה מחרוזת HTML לעץ של אוביקטי HtmlElement ומחזירה את שורש העץ
        public HtmlElement Parse(string html)
        {
            HtmlElement root = new HtmlElement();
            HtmlElement currentElement = root;

            foreach (string line in SplitToTags(html))
            {
                // תגית מתחילה ב-< ומסתיימת ב->, כל השאר הוא טקסט
                bool isTag = line.StartsWith("<") && line.EndsWith(">");

                // בתוך script או style הכול טקסט, עד תגית הסגירה שלהם
                if (currentElement.Name == "script" || currentElement.Name == "style")
                {
                    if (!isTag || !IsClosingTagOf(line, currentElement.Name))
                    {
                        currentElement.InnerHtml += line;
                        continue;
                    }
                }

                // טקסט פנימי של האלמנט הנוכחי
                if (!isTag)
                {
                    currentElement.InnerHtml += line;
                    continue;
                }

                // התוכן של התגית בלי הסוגריים < >
                string tagText = line.Substring(1, line.Length - 2).Trim();
                string firstWord = tagText.Split(' ', 2)[0].ToLower();

                // סוף מסמך HTML
                if (firstWord == "/html")
                {
                    break;
                }

                // תגית סוגרת - עולים בעץ עד האלמנט שנסגר, וחוזרים לאבא שלו
                if (firstWord.StartsWith("/"))
                {
                    string closingName = firstWord.Substring(1);

                    HtmlElement? closed = currentElement.Name == closingName
                        ? currentElement
                        : currentElement.Ancestors().FirstOrDefault(e => e.Name == closingName);

                    // אם אין אלמנט פתוח בשם הזה - מתעלמים מהתגית הסוגרת
                    if (closed != null && closed.Parent != null)
                    {
                        currentElement = closed.Parent;
                    }

                    continue;
                }

                // הסרת "/" צמוד לשם התגית, למשל <br/>
                string tagName = firstWord.TrimEnd('/');

                // תגית HTML חוקית
                if (HtmlHelper.Instance.HtmlTags.Contains(tagName))
                {
                    HtmlElement newElement = CreateElement(tagName, tagText);

                    newElement.Parent = currentElement;
                    currentElement.Children.Add(newElement);

                    // בדיקה אם התגית סוגרת את עצמה
                    bool isSelfClosing =
                        tagText.EndsWith("/") ||
                        HtmlHelper.Instance.HtmlVoidTags.Contains(tagName);

                    if (!isSelfClosing)
                    {
                        currentElement = newElement;
                    }
                }
            }

            return root;
        }

        // מפרקת את ה-HTML לרשימה של תגיות וטקסט, אחרי ניקוי הערות, DOCTYPE ורווחים מיותרים
        private IEnumerable<string> SplitToTags(string html)
        {
            // הסרת הערות HTML (גם הערות שנפרסות על כמה שורות)
            html = Regex.Replace(html, @"<!--.*?-->", "", RegexOptions.Singleline);

            // הסרת <!DOCTYPE ...> וכל הצהרה אחרת שמתחילה ב-!
            html = Regex.Replace(html, @"<![^>]*>", "");

            // פירוק לפי תגיות. התגיות נשארות עם הסוגריים < > כדי להבדיל בינן לבין טקסט.
            // [^>]* תופס גם תגיות שנפרסות על כמה שורות
            return Regex.Split(html, @"(<[^>]*>)")
                .Select(s => Regex.Replace(s, @"\s+", " ").Trim())
                .Where(s => s.Length > 0);
        }

        // יוצרת אלמנט חדש וממלאת את ה-Attributes, ה-Id וה-Classes שלו
        private HtmlElement CreateElement(string tagName, string line)
        {
            HtmlElement element = new HtmlElement();
            element.Name = tagName;

            // החלק שמכיל את המאפיינים (בלי שם התגית)
            string attributesText = line.Split(' ', 2).ElementAtOrDefault(1) ?? "";

            // זיהוי מאפיינים באמצעות Regex
            var attributes = Regex.Matches(
                attributesText,
                @"[^\s=]+(?:\s*=\s*(?:""[^""]*""|'[^']*'|[^\s]+))?"
            );

            foreach (Match attribute in attributes)
            {
                string value = attribute.Value;

                // דילוג על לוכסן סיום של תגית שסוגרת את עצמה
                if (value == "/")
                {
                    continue;
                }

                element.Attributes.Add(value);

                string[] parts = value.Split('=', 2);
                string name = parts[0].Trim().ToLower();
                string attributeValue = parts.Length > 1
                    ? parts[1].Trim().Trim('"', '\'')
                    : "";

                if (name == "id")
                {
                    element.Id = attributeValue;
                }
                else if (name == "class")
                {
                    element.Classes.AddRange(
                        attributeValue.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    );
                }
            }

            return element;
        }

        // בודקת אם התגית היא תגית הסגירה של האלמנט בשם הנתון, למשל </script>
        private bool IsClosingTagOf(string line, string name)
        {
            string tagText = line.Substring(1, line.Length - 2).Trim().ToLower();
            return tagText == "/" + name;
        }
    }
}
