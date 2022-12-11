# 1. Project Directories
- DigitNumberConsoleApp: For Question: Write a C# Console Application that displays the individual digits of a given number in
order using recursion. (Q2)
- FileWriterConsoleApp: For Questions related to FileHelper. Answers to each question are listed in Answers.md file (Q4)
- NewStringConsoleApp: For first question. (Q1)
- TaskAsyncConsoleApp: For Question: What would be the result of running the following console program. (Q3)

# 2. SQL Questions
## a.Q1: Truncate Statement
- Top vs Max(): Top needs to do the sort data in the query plan - from 1st to last page - regardless of the number of row to get from the top. Meanwhile, Max() is an aggregate function to do the calculation of **potential** max value that is found. As a result, Max() performs better in terms of performance and it always retuns a single value (includes null value). Top, however, can result in worst performance if the involved table is too big - especially the main column performed in Top is non-index. Top can return 0 record found.
- So Statement1 is better than Statement 2 in terms of: Precise, there's always result return (which would lead to the entire statement not being executed depends on the result found), and performance wise.
- Both Statements may result in incorrect result expected, if the ModifiedDate is not a right datetime format supported.
- So as reasons above, C# program would be affected: hanging in waiting for a result that takes longer if it executes the Statement 2 (potentially database deadlocks / program deadlocks if not implemented well). Its result of course is affected by the correctness of both statements.

## b.Q2: TempDb

Temdb is used temporarily for data storage (table/table variables/hash plans) which resulted from operations create/drop of table/table variables/hash plans. 
Microsoft mentions of the operations that cause contention in tempdb (resulted in data blocking): https://learn.microsoft.com/en-us/troubleshoot/sql/performance/recommendations-reduce-allocation-contention

So based on Microsoft best practices: monitoring and troubleshooting of tempdb to allocate bottleneck (https://learn.microsoft.com/en-us/archive/blogs/sqlserverstorageengine/tempdb-monitoring-and-troubleshooting-allocation-bottleneck); and config the tempdb in SQL Server approriately (https://learn.microsoft.com/en-us/archive/blogs/sqlserverstorageengine/managing-tempdb-in-sql-server-tempdb-configuration)

I am aware of the concepts and the importance of TempDb; however I am not too familiar with the practices with it directly, unfortunately.

## c. Truncate vs Delete:
Truncate works best when deleting a table with large data set than Delete as it deals with **data pages** by deallocation; while Delete performs (removes) directly on data rows.

You can rollback only some initial data if you used Truncate as it only uses fewer transaction logs than Delete.

Both need to declare the transaction to allow the rollback to happen.

More details: https://learn.microsoft.com/en-us/sql/t-sql/statements/truncate-table-transact-sql?redirectedfrom=MSDN&view=sql-server-ver16

## d. Stored Procedure performance:
The not-so-good performance comes from many factors that could not be accessed or analysed because that query/procedure has parameters of date, category or classification.

I am not a DBA so my knowledge is much limited than them. However, implementing a stored procesure or query I always need to follow some standards or best practices as much as possible:
- Call store procedure name with Schema - to reduce db server perform entire searching for stored procedure
- SET NOCOUNT ON for stored procedure to stop sending updates on rows affected in the query executed.
- Index right columns
- Join: Join over sub-queries. Use the right join: left, right, inner, outer... 
- Query only the columns that are needed.
- Leave transactional commit to the program rather than doing it in stored procedure, if it would be called together with other stored procedure. If it is a standalone store procedure by busines logic, then it might do transaction commit.
- Splitting a large stored procedure to as smaller stored procedures that each fits its main task.