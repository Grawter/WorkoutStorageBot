using System.Text;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Converters
{
    internal class ResponseTextConverter : IStringConverter
    {
        private string? title;
        private string? content;
        private string target;
        private readonly string? separator;
        private readonly bool onlyTarget;

        internal ResponseTextConverter(string target)
        {
            this.target = target;

            this.onlyTarget = true;
        }

        internal ResponseTextConverter(string content, string target)
        {
            this.content = content;

            this.target = target;

            this.separator = "======================";
        }

        internal ResponseTextConverter(string title, string content, string target, string? separator = null)
        {
            this.title = title;

            this.content = content;

            this.target = target;

            if (!string.IsNullOrWhiteSpace(separator))
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

            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(title))
            {
                sb.AppendLine(title);
                sb.AppendLine(separator);
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                sb.AppendLine(content);
                sb.AppendLine(separator);
            }

            if (!string.IsNullOrWhiteSpace(target))
            {
                sb.AppendLine();
                sb.AppendLine(target);
            }

            return sb.ToString();
        }
    }
}