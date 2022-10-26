using ConsoleOptions;
class Program
{
    class MyOptions
    {
        [Param(false, "User name")]
        public string? Name { get; set; }
        [Param(false, "User age")]
        public string? Age { get; set; }
        public int SomeFoo { get; set;}
        [Param(false, "User ID")]
        public string? ID { get; set; }
        [Param(true, "User surname")]
        public string? Surname { get; set;}

        [Command("-h", "Says Hello")]
        public void SayHello()
        {
            System.Console.WriteLine("Hello!");
        }

        [Command("-hu", "Says Hello with user's name")]
        public void SayNamedHello()
        {
            System.Console.WriteLine($"Hello {Name}");
        }
    }

    static void Main(string[] args)
    {
        var options = new MyOptions();
        var parser = new Parser<MyOptions>(options, "mycommand");

        parser.Parse(args);
        if(options is not null)
        {
            System.Console.WriteLine(options.Name);
            System.Console.WriteLine(options.Age);
            System.Console.WriteLine(options.Surname);
            System.Console.WriteLine(options.ID);
        }
    }
}