//exception occcurs when the params marked as required for application
//to function are somehow missused or werent passed at all 
namespace ConsoleOptions
{
    public class RequiredParamsException : ApplicationException
    {
        public RequiredParamsException(string message)
            :base(message)
        {}
    }
}