# 1. How would you go about further investigating this issue?

My initial steps when reading the issue and scanning through the code were:

## a. Identifying the issue keywords: 

- crash, long run, random intervals 
- Write file action and read file action (n seconds) so its copy can be written to another place. 

=> (usually related to multiple threads access resources asynchronously)

## b. Analyse Potential crash:

There are potential causes for crashes here:
+ General Exception handling in Task async; Or how General Exception is handled?
+ Server is inaccessible for validation/writing/reading file: Was the exception for this case handled?
+ Read/Write rights exception handling was there?
+ File is locked by one process (usually write action) while another process try to access (usually read action)

# 2. What is the probable cause of the crashes?

All situations analysed above can be **general** potential causes, by looking into the code, here are the causes identified:

## a. Cause 1: Exception Handling In Catch Block:

All 3 methods of FileUpdater class has wrong Exception handling approach for Task async: 
Throw Exception inside catch block. 

This is called *unhandled exception*, and in general of coding, should be always avoided.

In Async method, exceptions like these would get lost with the Task - even the invoker method that calls the async method tries to catch Exception later. 

When this kind of code is presented, the invoker usually has 2 types of implementation when invoking the Task async method.
I created class '**ExceptionTestProgram.cs**' as an example of 2 implementation types to produce the issue:

- Have a try catch block surrounding the Task async method that is called to handle exception.
(Method '**InvokerApproach1()**')
The Exception handling in the **InvokerApproach1()** would never be able to catch out ALL the exceptions. In this case, only '*InvokerApproach1 Exception*' in the method is caught.
Microsoft explains here for further understanding: https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/exception-handling-task-parallel-library

- Do not have a try catch in the invoker method (method **InvokerApproach2()**) that calls the Task async.
This would result in the crash.

## b. Solutions:
*Refer to same ExceptionTestProgram.cs class*
- If go with **InvokerApproach1()**, shall need to implement the AggregateException as Microsoft suggested as improved approach
(implemented by method '**InvokerApproach3()**'). StackTrace is improved with a meaningful log, but the Original exception of (*int.Parse("")*) is still lost.

- A better approach is **never throw Exception intentionally** or **let Exception thrown uncaught** *INSIDE* the Task.Run. Instead, return the exception and throw it outside the Task.Run - if it wants the exception to be handled by the invoker method. I created the method **SolutionTaskAsync()** as an improved version for the async method, and the invoker method **SolutionInvokerApproach1()** is same code as method **InvokerApproach3()**, but you would see the meaningful Exception being thrown with message: "SolutionInvokerApproach1 Exception *Input string was not in a correct format.*".

- A even better approach is: Why not let the Async Task return a result? Exception that is occurred in the Async Task should be handled by itself, (unless there is a specific particular reason behind not to), and the Async Task returns a result to the invoker. The invoker could use the result returned to do some particular business logic to handle further. I created 2 methods for this solution: Async Task method **SolutionTaskAsyncReturnResult()** and the invoker method **SolutionInvokerApproachReturnResult()** to demonstrate this approach.

## b. Cause 2: File.Exists() validation:
Microsoft says this would only result in true/false with *no exception* throw when checking if a file with a given path exists.
However, it also warns: "*while trying to determine if the specified file exists. This can occur in situations that raise exceptions such as passing a file name with invalid characters or too many characters, a failing or missing disk, or if the caller does not have permission to read the file.*" (https://learn.microsoft.com/en-us/dotnet/api/system.io.file.exists?view=net-7.0).

Hence, File.Exists() implementations in the **FinaliseWorkingAsync()** method of **FileUpdater** class must be placed inside try catch block.

## c. Cause 3 - Most Important Cause: lock, async, Task.run() and await usages:

a. **lock and async**: In general, a *lock* keyword is for **synchronized mechanism**, hence it should be expected to be used for **a very short duration thread** and not for concurrent access to shared resources. Microsoft has Guidelines for using this lock very clearly (Under Guidelines - https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock). 
All 3 methods in **FileUpdater** class violated 2 principles: **lock(this)** for entire instance object and *for entire task*; and use this (lock) on **async task** that is for handling concurrent multi threads.

C# introduced the SemaphoreSlim (lighter version of Semaphore) as a lock for async task that supports handling concurrent multi threads: https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim?view=netcore-3.1

So, use this instead if **there's a need to use a lock**. However, similar to using a lock, there must be a release after usage - so must call **Release()** after *the need to lock the resource is done*.

Another tip to remember is: **Only lock for a particular job that is necessary to do so**, not the whole block of code or even worse the entire task. 

b. **Task.run()** and **await**: There is a difference between what Task.run() does and await does. While Task.run() moves the execution code to another thread on threadpool so the long-running CPU operations on UI thread would not be blocked; meanwhile, await does not block the calling threads on the long-running activities hence the current thread can do what it wants without being blocked. Both works well in supporting running async tasks, just depends on whether that's a desktop application (UI thread) or ASP.NET (IIS managed threads) that the usage of Task.run() or await should be better used.

Given I do not know the context of the FileUpdater, whether the service calls it is windows service or web service; so I cant recommend the change in whether using the Task.run() or await. 

For now, I would assume it is a web services or web application asp.net that would use this class, hence I'd choose to use await. 

There is a scenario in 1 async task where we need to run a task and then await for the task result to let the task continue doing something - in that scenario, we can use both but certainly at different places in code.


c. **Know when to use await/Task.run()**:

Both could give deadlocks, even use either separated await or Task.run() in async Task - DEPENDS on the context of code that implements the async task as well as the method that invokes that async task.

d. **mix async with sync**: For example, a UI thread method (e.g. button clicked) calls an async task (e.g. GetResultAsync()) as below:
Task<string> task = GetResultAsync();
// update textview with result: textview.Text = task.Result;

If GetResultAsync() has a kind of tasks inside to retrieve some other info from other threads that are unable to complete which results in a loop of threads, above code would have a chance of deadlocks as the update textview is **synchronously** waiting for the result.

**c and d** are extra points I'd like to highlight in general.

## 3. How could this be resolved?

I implemented a sample 'FileUpdaterProgram.cs' class - for 2 methods FinaliseWorkingAsync and WriteAsync - to demonstrate points noted above.
