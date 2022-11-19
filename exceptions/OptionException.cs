//wrong format of argument passed to parser
namespace ConsoleOptions
{
    public class OptionException : ApplicationException
    {
        public OptionException(string message) 
            :base(message)
        {}
    }
}