namespace TurtleLang.Models.Exceptions;

class UnexpectedTokenException: Exception
{
    public UnexpectedTokenException(string message): base(message)
    {
    }
}