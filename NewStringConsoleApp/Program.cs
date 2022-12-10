// See https://aka.ms/new-console-template for more information
bool quit = false;
do 
{
    Console.WriteLine("Please type your text, or type Q to exit; then press Enter.");
    var s = Console.ReadLine()?.Trim() ?? "";
    quit = "q".Equals(s.ToLower());
    if (!quit && s.Length > 0) 
    {
        var appendChars = s.Length > 3 ? s.Substring(0, 3) : "";
        Console.WriteLine($"Your new text is: {appendChars}{s}{appendChars}.");
    }
} while (!quit);
return -1;

