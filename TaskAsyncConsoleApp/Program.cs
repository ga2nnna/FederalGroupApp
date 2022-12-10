/// <summary>
/// This is an updated solution for the original code.
/// </summary>
class Program
{
    static void Main()
    {
        // In original version, you'll get warning CS4014 (https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs4014?f1url=%3FappId%3Droslyn%26k%3Dk(CS4014))
        // which clearly says:
        // "Because this call is not awaited,
        // execution of the current method continues before the call is completed.
        // Consider applying the await operator to the result of the call."
        // Hence, this is workaround: follows as warning instructs.
        Task<string> task = GetResult();
        task.Wait();
        // after the await period for the task is done, it'll return the task with result,
        // which can be used for print out.
        Console.WriteLine($"{task.Result}"); 
        Console.ReadKey();
    }

    static async Task<string> GetResult()
    {
        await Task.Delay(1000);
        var result = "Hello world!";
        return result;
    }
}

