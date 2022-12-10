/// <summary>
/// This is is just a test code that simulate code in FileUpdater, not a main console program.
/// To avoid complexity, lock is not used here.
/// </summary>
class ExceptionTestProgram
{
    #region "Main Thead Invokes async task"
    // Change to Main() if you want to run to see the result
    static async Task InvokerApproach1()
    { 
        try
        {
            await OriginalTaskAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("------- LOG SIMULATION ---");
            Console.WriteLine($"InvokerApproach1 Exception {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("------- LOG END ---");
        }
        finally
        {
            Console.WriteLine("InvokerApproach1 FINISHED");
        }
    }

    // Change to Main() if you want to run to see the result
    static async Task InvokerApproach2()
    {
        await OriginalTaskAsync();
        // This code below never reach.
        Console.WriteLine("InvokerApproach2 FINISHED");
    }

    // Change to Main() if you want to run to see the result
    static async Task InvokerApproach3()
    {
        try
        {
            await OriginalTaskAsync();
        }
        catch (AggregateException ae)
        {
            Console.WriteLine("------- LOG SIMULATION ---");
            foreach (var ex in ae.InnerExceptions)
            {
                Console.WriteLine($"InvokerApproach3 Exception {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            Console.WriteLine("------- LOG END ---");
        }
        finally
        {
            Console.WriteLine("InvokerApproach3 FINISHED");
        }
    }

    // Change to Main() if you want to run to see the result
    static async Task SolutionInvokerApproach1()
    {
        try
        {
            await SolutionTaskAsync();
        }
        catch (AggregateException ae)
        {
            Console.WriteLine("------- LOG SIMULATION ---");
            foreach (var ex in ae.InnerExceptions)
            {
                Console.WriteLine($"SolutionInvokerApproach1 Exception {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            Console.WriteLine("------- LOG END ---");
        }
        finally
        {
            Console.WriteLine("SolutionInvokerApproach1 FINISHED");
        }
    }

    // Change to Main() if you want to run to see the result
    static async Task SolutionInvokerApproachReturnResult()
    {
        try
        {
            Task<Boolean> task = SolutionTaskAsyncReturnResult();
            await task;
            Console.WriteLine($"SolutionInvokerApproachReturnResult FINISHED. Result: {task.Result}");
        }
        catch (AggregateException ae)
        {
            Console.WriteLine("------- LOG SIMULATION ---");
            foreach (var ex in ae.InnerExceptions)
            {
                Console.WriteLine($"SolutionInvokerApproachReturnResult Exception {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            Console.WriteLine("------- LOG END ---");
        }
    }
#endregion

    #region "Async Task Methods - For Answers Related to FileUpdater"
    static async Task OriginalTaskAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                Console.WriteLine("OriginalTaskAsync START");
                // simulate exception
                int.Parse("");
            }
            catch (Exception ex)
            {
                throw new Exception("OriginalTaskAsync unhandled", ex);
            }
        });
    }

    static async Task SolutionTaskAsync()
    {
        Exception? exception = await Task.Run(() =>
        {
            try
            {
                Console.WriteLine("SolutionTaskAsync START");
                // simulate exception
                int.Parse("");
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        });
        if (exception != null)
            throw exception;
    }


    static async Task<Boolean> SolutionTaskAsyncReturnResult()
    {
        Exception? exception = await Task.Run(() =>
        {
            try
            {
                Console.WriteLine("SolutionTaskAsyncReturnResult START");
                // simulate exception
                int.Parse("");
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        });
        if (exception != null)
        {
            // log exception. simulate by writing to console
            Console.WriteLine("------- LOG SIMULATION ---");
            Console.WriteLine($"SolutionTaskAsync2 {exception.Message}");
            Console.WriteLine($"{exception.StackTrace}");
            Console.WriteLine("----- LOG END ------");
            Console.WriteLine("");
            return false;
        }
        return true;
    }
    #endregion
}

