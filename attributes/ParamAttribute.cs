namespace ConsoleOptions
{
    public class ParamAttribute : Attribute
    {
        public bool Optional { get; }
        public string Description {get;}
        public ParamAttribute(bool optional, string description)
        {
            Optional = optional;
            Description = description;
        }
    }
}