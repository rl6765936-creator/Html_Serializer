using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Html_Serializer
{
    public class HtmlElement
    {
        public string? Id { get; set; }
        public string Name { get; set; }
        public List<string> Attributes { get; set; }
        public List<string> Classes { get; set; }
        public string InnerHtml { get; set; }
        public HtmlElement? Parent { get; set; } 
        public List<HtmlElement> Children { get; set; }

        public HtmlElement()
        {
            Id = null;
            Name = "";
            Attributes = new List<string>();
            Classes = new List<string>();
            InnerHtml = "";
            Parent = null;
            Children = new List<HtmlElement>();
        }

        // מחזירה רשימה שטוחה של האלמנט וכל צאצאיו מכל הדורות (באמצעות תור, בלי ריקורסיה)
        public IEnumerable<HtmlElement> Descendants()
        {
            Queue<HtmlElement> queue = new Queue<HtmlElement>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                HtmlElement current = queue.Dequeue();

                yield return current;

                foreach (HtmlElement child in current.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        // מחזירה רשימה שטוחה של כל אבות האלמנט, מהאבא הישיר ועד השורש
        public IEnumerable<HtmlElement> Ancestors()
        {
            HtmlElement? current = Parent;

            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }
    }
}
