namespace ConsoleOptions
{
    public class ParamAttribute : Attribute
    {
        public string Name {get; }
        public bool Optional { get; }
        public string Description {get;}
        public ParamAttribute(string name, bool optional, string description)
        {
            Name = name;
            Optional = optional;
            Description = description;
        }
    }
}