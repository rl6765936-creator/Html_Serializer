using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Html_Serializer
{
    // מחלקת עזר (Singleton) שמספקת את רשימת תגיות ה-HTML ואת רשימת התגיות שלא דורשות סגירה
    public class HtmlHelper
    {
        private readonly static HtmlHelper _instance = new HtmlHelper();
        public static HtmlHelper Instance => _instance;

        // כל תגיות ה-HTML
        public string[] HtmlTags { get; private set; }

        // תגיות שלא דורשות תגית סגירה
        public string[] HtmlVoidTags { get; private set; }

        // בנאי פרטי - טוען פעם אחת את שני קבצי ה-JSON למערכים
        private HtmlHelper()
        {
            // נתיב מלא לתיקיית הפלט, כדי שהטעינה תעבוד מכל תיקייה נוכחית
            string baseDirectory = AppContext.BaseDirectory;

            string tagsJson = File.ReadAllText(Path.Combine(baseDirectory, "HtmlTags.json"));
            HtmlTags = JsonSerializer.Deserialize<string[]>(tagsJson)!;

            string voidTagsJson = File.ReadAllText(Path.Combine(baseDirectory, "HtmlVoidTags.json"));
            HtmlVoidTags = JsonSerializer.Deserialize<string[]>(voidTagsJson)!;
        }
    }
}
