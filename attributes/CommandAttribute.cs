namespace ConsoleOptions
{
    class CommandAttribute : Attribute
    {
        public string Command { get;}
        public string Description{ get;}
        public CommandAttribute(string command, string description)
        {
            if((command.Length < 2) || !command[0].Equals('-'))
                throw new FormatException("Command should be in format '-command'");
            
            Command = command;
            Description = description;
        }
    }
}