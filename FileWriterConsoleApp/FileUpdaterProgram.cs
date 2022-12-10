/// <summary>
/// This is is just a test code, not a main console program.
/// I only demonstrated the change in 1 method.
/// </summary>
class FileUpdaterProgram
{
    private static SemaphoreSlim _semaphore;
    // Change to Main() if you want to run to see the result
    static async Task Main(string[] args)
    {
        //await SimilateMultipleTasksExecuteFinaliseFile(10);
        await SimilateMultipleTasksWriteFile(10);
    }

    static async Task SimilateMultipleTasksExecuteFinaliseFile(int numberTasks = 5)
    {
        var allTasks = new List<Task<FinalisedStatus>>();
        var fileDir = "C:\\D\\Test\\";
        var fileDestinationDir = "C:\\D\\Test1\\";
        try
        {
            for (int i = 0; i < numberTasks; i++)
            {
                allTasks.Add(FinaliseWorkingAsync(fileDir + "workingFile.txt",
                                                            fileDestinationDir + "targetFile.txt",
                                                            fileDir + "semaphoreFile.txt"));
            }

            await Task.WhenAny(allTasks);
            allTasks.ForEach(task => Console.WriteLine($"Main Program ExecuteFinaliseFile Finished - Task {task.Id} - Result {task.Result.ToString()}"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Main Program - ExecuteFinaliseFile - Exception {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static async Task SimilateMultipleTasksWriteFile(int numberTasks = 5)
    {
        _semaphore = new SemaphoreSlim(1, numberTasks);
        var allTasks = new List<Task<WriteFileStatus>>();
        // use only 1 file to have the content updated - for testing.
        var filePath = "C:\\D\\Test\\";
        try
        {
            for (int i = 0; i < numberTasks; i++)
            {
                // test 1: use only 1 file to have the content updated - for testing.
                allTasks.Add(WriteAsync($"content {i}", filePath + "onlyFile.txt"));
                // test 2: write multiple file for each task
                //allTasks.Add(WriteAsync($"content {i}", filePath + $"file{i}.txt"));
            }

            await Task.WhenAny(allTasks);
            allTasks.ForEach(task => Console.WriteLine($"Main Program WriteFile Finished - Task {task.Id} - Result {task.Result.ToString()}"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Main Program - WriteFile - Exception {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }


    static async Task<FinalisedStatus> FinaliseWorkingAsync(string workingFilePath, string targetFilePath, string semaphoreFilePath)
    {
        try
        {
            //Log.Info("Beginning working file finalisation.");
            if (File.Exists(semaphoreFilePath))
            {
                //Log.Info("Semaphore found, skipping finalisation.");
                Console.WriteLine("Semaphore found, skipping finalisation.");
                return FinalisedStatus.Skip;
            }
            else if (File.Exists(targetFilePath))
            {
                Console.WriteLine($"Existing file found at {targetFilePath}. Unable to finalise working file despite no semaphore present.Skipping.");
                //Log.Error($"Existing file found at {targetFilePath}. Unable to finalise working file despite no semaphore present.Skipping.");
                //Auditor.AuditEvent("DestinationWriterWrite-Failed", () => targetFilePath);
                return FinalisedStatus.ExistingTargetFileFound;
            }

            if (File.Exists(workingFilePath))
            {
                Console.WriteLine("File found, publishing to target directory.");
                /*Log.Info("File found, publishing to target directory.");
                Log.Debug($"File found at: {workingFilePath}, moving to: { targetFilePath}");*/
                await File.WriteAllTextAsync(semaphoreFilePath, "");
                File.Move(workingFilePath, targetFilePath);

                //Log.Info("Finalisation completed.");
                Console.WriteLine("Finalisation completed.");
                return FinalisedStatus.Finalised;
            }
            return FinalisedStatus.ExistingWorkingFileNotFound;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occurred while publishing working file. Error:{ e.Message}");
            Console.WriteLine("Move failed.");
            /*Log.Error($"Error occurred while publishing working file. Error:{ e.Message}");
            Log.Error("Move failed", e);
            */
            //Auditor.AuditEvent("DestinationWriterWrite-Failed", () => targetFilePath);
            //throw new Exception("Error occurred publishing working file.", e);
            return FinalisedStatus.Error;
        }
       
    }


    static async Task<WriteFileStatus> WriteAsync(string content, string workingFilePath)
    {
        try
        {
            await _semaphore.WaitAsync();
            await File.AppendAllTextAsync(workingFilePath, content);
            //Log.Info($"Message processing complete!");
            Console.WriteLine("Message processing complete!");
            return WriteFileStatus.Completed;
        }
        catch (Exception e)
        {
            /*
            Log.Error($"Error occurred updating working file. Error: {e.Message}");
            Log.Error("Write Failed", e);*/
            //Auditor.AuditEvent("DestinationWriterWrite-Failed", () => model);
            //throw new Exception("Error occurred updating working file.", e);
            Console.WriteLine($"Error occurred updating working file. Error: {e.Message}");
            Console.WriteLine("Write failed.");
            return WriteFileStatus.Failed;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    enum FinalisedStatus : ushort
    {
        Skip = 0,
        ExistingTargetFileFound = 1,
        ExistingWorkingFileNotFound = 2,
        Finalised = 3,
        Error = 4
    }

    enum WriteFileStatus : ushort
    {
        Completed = 0,
        Failed = 1
    }
}

