using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    /// <summary>
    /// Renders story content with optional inline images into a VisualElement. The content
    /// supports a simple markup: [img=path align=left|right width=NN]. Images are loaded
    /// from the Resources folder. Text outside the tags is rendered in a Label with the
    /// class "story-text". Aligns images left or right by adding flexible spacers.
    /// </summary>
    public static class RichStoryRenderer
    {
        private static readonly Regex ImgRegex = new Regex(
            @"\[img=(?<path>[^\s\]]+)(?:\s+align=(?<align>left|right))?(?:\s+width=(?<width>\d+))?\]",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static void Render(VisualElement container, string content)
        {
            if (container == null) return;
            container.Clear();

            if (string.IsNullOrEmpty(content)) return;

            int lastIndex = 0;
            foreach (Match m in ImgRegex.Matches(content))
            {
                // Preceding text
                if (m.Index > lastIndex)
                {
                    string textPart = content.Substring(lastIndex, m.Index - lastIndex).Trim();
                    if (!string.IsNullOrEmpty(textPart))
                    {
                        var label = new Label(textPart);
                        label.AddToClassList("story-text");
                        label.style.whiteSpace = WhiteSpace.Normal;
                        container.Add(label);
                    }
                }

                // Parse image attributes
                string path = m.Groups["path"].Value;
                string align = m.Groups["align"].Success ? m.Groups["align"].Value.ToLowerInvariant() : "right";
                // If no width is specified in the tag, use a larger default to
                // better showcase story illustrations.  Increase from 260 to 400px.
                int width = m.Groups["width"].Success ? int.Parse(m.Groups["width"].Value) : 400;

                var tex = Resources.Load<Texture2D>(path);
                if (tex != null)
                {
                    var img = new Image
                    {
                        scaleMode = ScaleMode.ScaleToFit,
                        image = tex
                    };
                    img.AddToClassList("story-image-inline");
                    img.style.width = width;
                    img.style.height = width * tex.height / (float)tex.width;

                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row, width = Length.Percent(100) } };
                    if (align == "left")
                    {
                        row.Add(img);
                        row.Add(new VisualElement { style = { flexGrow = 1 } });
                    }
                    else
                    {
                        row.Add(new VisualElement { style = { flexGrow = 1 } });
                        row.Add(img);
                    }
                    container.Add(row);
                }

                lastIndex = m.Index + m.Length;
            }

            // Trailing text
            if (lastIndex < content.Length)
            {
                string textPart = content.Substring(lastIndex).Trim();
                if (!string.IsNullOrEmpty(textPart))
                {
                    var label = new Label(textPart);
                    label.AddToClassList("story-text");
                    label.style.whiteSpace = WhiteSpace.Normal;
                    container.Add(label);
                }
            }
        }
    }
}