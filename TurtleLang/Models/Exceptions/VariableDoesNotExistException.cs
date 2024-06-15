namespace TurtleLang.Models.Exceptions;

class VariableDoesNotExistException: Exception
{
    public VariableDoesNotExistException(string variableName) : base($"Variable {variableName} does not exist in current scope")
    {
    }
}