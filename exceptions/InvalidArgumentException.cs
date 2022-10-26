//exception occurs when a console argument that is passed to the application 
//is invalid, unrecognized.
namespace ConsoleOptions
{
    class InvalidArgumentException : ApplicationException
    {
        public InvalidArgumentException(string message)
            :base(message)
        {}
    }
}