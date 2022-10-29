//wrong format of argument passed to parser
namespace ConsoleOptions
{
    public class FormatException : ApplicationException
    {
        public FormatException(string message) 
            :base(message)
        {}
    }
}