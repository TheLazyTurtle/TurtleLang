namespace TurtleLang.Models.Exceptions;

class RedefinitionException: Exception
{
    public RedefinitionException(string functionName): base($"Redefinition of function: {functionName}")
    {
    }
}