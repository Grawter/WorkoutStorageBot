#region using

using System.Text;

#endregion

namespace WorkoutStorageBot.Helpers.Converters
{
    internal class ResponseTextConverter : IStringConverter
    {
        private StringBuilder? sb;
        private string? title;
        private string? content;
        private string target;
        private string? separator;
        private bool onlyTarget;

        internal ResponseTextConverter(string target)
        {
            this.target = target;

            onlyTarget = true;
        }

        internal ResponseTextConverter(string content, string target)
        {
            sb = new StringBuilder();

            this.content = content;

            this.target = target;

            separator = "======================";
        }

        internal ResponseTextConverter(string title, string content, string target, string? separator = null)
        {
            sb = new StringBuilder();

            this.title = title;

            this.content = content;

            this.target = target;

            if (!string.IsNullOrEmpty(separator))
                this.separator = separator;
            else
                this.separator = "======================";
        }

        internal void ResetTitle(string title)
        {
            this.title = title;
        }

        internal void ResetContent(string content)
        {
            this.content = content;
        }

        internal void ResetTarget(string target)
        {
            this.target = target;
        }

        public string Convert()
        {
            if (onlyTarget)
                return target;

            if (!string.IsNullOrEmpty(title))
            {
                sb.AppendLine(title);
                sb.AppendLine(separator);
            }

            if (!string.IsNullOrEmpty(content))
            {
                sb.AppendLine(content);
                sb.AppendLine(separator);
            }

            if (!string.IsNullOrEmpty(target))
            {
                sb.AppendLine();
                sb.AppendLine(target);
            }

            return sb.ToString();
        }
    }
}