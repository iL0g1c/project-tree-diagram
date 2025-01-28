using System;

public static class ErrorHandler
{
    public static void LogError(int errorCode, Exception exception, string? additionalInfo = null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] Code: {errorCode}");
        Console.WriteLine($"Message: {exception.Message}");
        if (!string.IsNullOrWhiteSpace(additionalInfo))
        {
            Console.WriteLine($"Additional Info: {additionalInfo}");
        }
        Console.WriteLine($"Stack Trace: {exception.StackTrace}");
        Console.ResetColor();
    }

    public static bool HandleNonFatalError(int errorCode, Exception exception, Action? retryAction = null, string? additionalInfo = null)
    {
        LogError(errorCode, exception, additionalInfo);

        if (retryAction != null)
        {
            try
            {
                Console.WriteLine("Retrying...");
                retryAction.Invoke();
                Console.WriteLine("Retry successful.");
                return true;
            }
            catch (Exception retryException)
            {
                LogError(errorCode, retryException, "Retry failed.");
            }
        }

        return false;
    }

    public static void HandleFatalError(int errorCode, Exception exception)
    {
        LogError(errorCode, exception);
        Console.WriteLine("A fatal error occurred. The application will terminate.");
        Environment.Exit(errorCode);
    }
}