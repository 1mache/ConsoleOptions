//exception occurs when a console argument that is passed to the application 
//is invalid, unrecognized.
class InvalidArgumentException : ApplicationException
{
    public InvalidArgumentException(string message)
        :base(message)
    {}
}