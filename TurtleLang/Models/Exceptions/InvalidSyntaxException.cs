namespace TurtleLang.Models.Exceptions;

class InvalidSyntaxException: Exception
{
    public InvalidSyntaxException(string error): base($"Invalid syntax: {error}")
    {
    }
}