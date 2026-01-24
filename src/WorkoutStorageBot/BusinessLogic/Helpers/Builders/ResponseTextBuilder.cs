using System.Text;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Converters
{
    internal class ResponseTextBuilder : IBuilder
    {
        private string? title;
        private string? content;
        private string target;
        private readonly string? separator;
        private readonly bool onlyTarget;

        internal ResponseTextBuilder(string target)
        {
            this.target = target;

            this.onlyTarget = true;
        }

        internal ResponseTextBuilder(string content, string target)
        {
            this.content = content;

            this.target = target;

            this.separator = "======================";
        }

        internal ResponseTextBuilder(string title, string content, string target, string? separator = null)
        {
            this.title = title;

            this.content = content;

            this.target = target;

            if (!string.IsNullOrWhiteSpace(separator))
                this.separator = separator;
            else
                this.separator = "======================";
        }

        internal ResponseTextBuilder ResetTitle(string title)
        {
            this.title = title;

            return this;
        }

        internal ResponseTextBuilder ResetContent(string content)
        {
            this.content = content;

            return this;
        }

        internal ResponseTextBuilder ResetTarget(string target)
        {
            this.target = target;

            return this;
        }

        string IBuilder.Build()
            => Build();

        internal string Build()
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