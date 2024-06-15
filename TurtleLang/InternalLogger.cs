namespace TurtleLang;

static class InternalLogger
{
    public static bool IsLoggingEnabled { get; set; } = true;
    
    public static void Log(string log)
    {
        if (!IsLoggingEnabled)
            return;

        Console.WriteLine(log);
    }
    
    public static void Log(object log)
    {
        if (!IsLoggingEnabled)
            return;

        Console.WriteLine(log.ToString());
    }
}