using System;

namespace Dorbit.Framework.Extensions;

public class ConsoleExtensions
{
    public static string ReadPassword()
    {
        string password = "";

        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);

            // Ignore non-character keys (e.g., function keys)
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Remove(password.Length - 1);
                Console.Write("\b \b"); // Erase the last character from the console
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine(); // Move to the next line after pressing Enter

        return password;
    }
}