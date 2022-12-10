// See https://aka.ms/new-console-template for more information
bool quit = false;
do 
{
    Console.WriteLine("Please type your number, or type Q to exit; then press Enter.");
    var s = Console.ReadLine()?.Trim() ?? "";
    quit = "q".Equals(s.ToLower());
    if (!quit && s.Length > 0) 
    {
        var number = -1; 
        bool isNumberic = Int32.TryParse(s, out number);
        if(isNumberic)
        {
            Console.WriteLine("Outputs: ");
            PrintNumberToDigits(number);
            Console.WriteLine();
        }
        else
            Console.WriteLine("It is not a number.");
    }
} while (!quit);
return -1;

static void PrintNumberToDigits(int number)
{
    // the recursive function is being called itself repeatedly
    // until the number reached a single digit number ((number / 10) == 0)
    // then print out the the remainder of the number value from each recursive functions on stack
    if ((number / 10) > 0)
        PrintNumberToDigits(number / 10);
    
    Console.Write(" {0} ", (number % 10));
} 