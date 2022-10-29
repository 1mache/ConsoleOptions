//this exception is for cases when attribute is assigned to a property/method 
//that cannot be a assigned with this type of attribute. (for example: no accessibility or wrong typing).
namespace ConsoleOptions
{
    public class InvalidAttributeException : ApplicationException
    {
        public InvalidAttributeException(string message)
            :base(message)
        {}
    }
}