using System;
using System.Text;

public static class Logger
{
    static readonly object lockObj = new object();
    public static void red(params object[] args)
    {
        _log(ConsoleColor.Red, Console.BackgroundColor, args);
    }
    public static void Red(params object[] args)
    {
        _log(ConsoleColor.Black, ConsoleColor.Red, args);
    }
    public static void magenta(params object[] args)
    {
        _log(ConsoleColor.Magenta, Console.BackgroundColor, args);
    }
    public static void Magenta(params object[] args)
    {
        _log(ConsoleColor.Black, ConsoleColor.Magenta, args);
    }
    public static void green(params object[] args)
    {
        _log(ConsoleColor.Green, Console.BackgroundColor, args);
    }
    public static void Green(params object[] args)
    {
        _log(ConsoleColor.Black, ConsoleColor.Green, args);
    }
    public static void white(params object[] args)
    {
        _log(ConsoleColor.White, Console.BackgroundColor, args);
    }
    public static void White(params object[] args)
    {
        _log(ConsoleColor.Black, ConsoleColor.White, args);
    }
    public static void blue(params object[] args)
    {
        _log(ConsoleColor.Blue, Console.BackgroundColor, args);
    }
    public static void Blue(params object[] args)
    {
        _log(ConsoleColor.Black, ConsoleColor.Blue, args);
    }
    public static void cyan(params object[] args)
    {
        _log(ConsoleColor.Cyan, Console.BackgroundColor, args);
    }
    public static void Cyan(params object[] args)
    {
        _log(ConsoleColor.Black, ConsoleColor.Cyan, args);
    }
    public static void Log(params object[] args)
    {
        _log(Console.ForegroundColor, Console.BackgroundColor, args);
    }
    static void _log(ConsoleColor fc, ConsoleColor bc, params object[] args)
    {
        lock (lockObj)
        {
            var str = new StringBuilder();
            str.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff "));
            str.Append(string.Join(" ", args));
            Console.ForegroundColor = fc;
            Console.BackgroundColor = bc;
            Console.Write(str.ToString());
            Console.ResetColor();
            Console.Write(Environment.NewLine);
        }
    }
}
