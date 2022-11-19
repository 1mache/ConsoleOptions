namespace ConsoleOptions
{
    public class CommandAttribute : Attribute
    {
        
        public string Name { get;}
        public string Description{ get;}
        public CommandAttribute(string name, string description)
        {
            if(name[0].Equals('-'))
                throw new OptionException("Name of command must not have a - as first symbool");
            
            Name = name;
            Description = description;
        }
    }
}