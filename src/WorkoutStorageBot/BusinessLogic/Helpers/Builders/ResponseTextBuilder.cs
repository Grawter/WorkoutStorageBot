using System.Text;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Converters
{
    internal class ResponseTextBuilder
    {
        private string? title;
        private string? content;
        private string target;
        private string? separator;
        private readonly bool onlyTarget;

        internal ResponseTextBuilder(string target)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(target);

            this.target = target;

            this.onlyTarget = true;
        }

        internal ResponseTextBuilder(string content, string target)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(content);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(target);

            this.content = content;
            this.target = target;
        }

        internal ResponseTextBuilder(string title, string content, string target, string? separator = null) : this(content, target)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(title);

            this.title = title;

            if (!string.IsNullOrWhiteSpace(separator))
                this.separator = separator;
        }

        internal ResponseTextBuilder ResetTitle(string title)
        {
            if (onlyTarget)
                throw new InvalidOperationException("Нельзя установить title, т.к ранее был установлен признак onlyTarget");

            this.title = title;

            return this;
        }

        internal ResponseTextBuilder ResetContent(string content)
        {
            if (onlyTarget)
                throw new InvalidOperationException("Нельзя установить content, т.к ранее был установлен признак onlyTarget");

            this.content = content;

            return this;
        }

        internal ResponseTextBuilder ResetTarget(string target)
        {
            this.target = target;

            return this;
        }

        internal string Build()
        {
            if (onlyTarget)
                return target;

            if (string.IsNullOrWhiteSpace(separator))
                this.separator = "======================";
   
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