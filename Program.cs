
class Program
{
    class MyOptions
    {
        [ParamAttribute(false, "User name")]
        public string? Name { get; set; }
        [ParamAttribute(false, "User age")]
        public string? Age { get; set; }
        public int SomeFoo { get; set;}
        [ParamAttribute(false, "User ID")]
        public string? ID { get; set; }
        [ParamAttribute(true, "User surname")]
        public string? Surname { get; set;}
    }

    static void Main(string[] args)
    {
        var options = new MyOptions();
        var parser = new Parser<MyOptions>(options, "mycommand");

        options = parser.Parse(args);
        if(options is not null)
        {
            System.Console.WriteLine(options.Name);
            System.Console.WriteLine(options.Age);
            System.Console.WriteLine(options.Surname);
            System.Console.WriteLine(options.ID);
        }
    }
}