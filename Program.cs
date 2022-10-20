
class Program
{
    class MyOptions
    {
        [ParamAttribute(false, "User name")]
        public string? Name { get; set; }
        [ParamAttribute(false, "User age")]
        public int Age { get; set; }
        public int SomeFoo { get; set; }
        [ParamAttribute(true, "User surname")]
        public string? Surname { get; set; }
    }

    static void Main(string[] args)
    {
        var options = new MyOptions();
        var parser = new Parser<MyOptions>(options, "mycommand");

        parser.ShowHelp();
    }
}