namespace Tailwind.Mail.Commands
{
    public class LinkClickedCommand
    {
        public LinkClickedCommand(string key)
        {
            Key = key;
        }

        public string Key { get; }

        public string Execute()
        {
            return $"Link clicked: {Key}";
        }
    }
}