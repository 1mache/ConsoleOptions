class CommandAttribute
{
    public string Command { get;}
    public CommandAttribute(string command)
    {
        if((command.Length < 2) || !command[0].Equals('-'))
            throw new Exception("Command should be in format '-command'");
        
        Command = command;
    }
}