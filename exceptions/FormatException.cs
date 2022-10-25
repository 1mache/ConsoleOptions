//wrong format of argument passed to parser
class FormatException : ApplicationException
{
    public FormatException(string message) 
        :base(message)
    {}
}