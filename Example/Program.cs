using InteractiveConsole;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Example
{
    class Program
    {
        private static Regex word = new Regex(@"[\w]+");
        private static Regex sector = new Regex("\\/[^\\/\\s]+");
        private static Regex url = new Regex(@"(^|\s)((ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?)\s");

        private static int curLeft;
        private static int curTop;

        static void Main(string[] args)
        {

            //Events of the same type for phrases will be triggered according to the specified order.
            ConsoleListener.IntaractivePhrases = new List<Phrase>()
            {
              new Phrase(word),
              new Phrase(url),
              new Phrase(sector),
            };

            foreach(var phrase in ConsoleListener.IntaractivePhrases)
            {
                phrase.ConsoleMouseUpEvent += OnPhraseMouseUp;
                phrase.ConsoleMouseDownEvent += OnPhraseMouseDown;
                phrase.ConsoleMouseClickEvent += OnPhraseMouseClick;
                phrase.ConsoleMouseEnterEvent += OnPhraseMouseEnter;
                phrase.ConsoleMouseLeaveEvent += OnPhraseMouseLeave;
            }
            ConsoleListener.Start();

            Console.WriteLine(@"Lorem /ipsum dolor sit amet, /consectetur/ adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
Ut enim ad minim veniam, quis nostrud /exercitation/ ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
Excepteur sint occaecat cupidatat non proident, sunt in culpa qui /officia/ deserunt mollit anim id est laborum.");
           
            Console.WriteLine();

            curLeft = Console.CursorLeft;
            curTop = Console.CursorTop + 1;

            Console.WriteLine("https://github.com/Vovanda");
            var tempUrl = new Phrase(url);
            Phrase.FindInPosition(0, curTop - 1, ref tempUrl);
            SetUrlPhraseColor(tempUrl, ConsoleColor.DarkCyan, 0, curTop++);

            Console.WriteLine("https://vk.com/vuvu_man");
            Phrase.FindInPosition(0, curTop - 1, ref tempUrl);
            SetUrlPhraseColor(tempUrl, ConsoleColor.DarkCyan, 0, curTop++);

            Console.ReadKey();

            ConsoleListener.Stop();
            foreach (var phrase in ConsoleListener.IntaractivePhrases)
            {
                phrase.ConsoleMouseUpEvent -= OnPhraseMouseUp;
                phrase.ConsoleMouseDownEvent -= OnPhraseMouseDown;
                phrase.ConsoleMouseClickEvent -= OnPhraseMouseClick;
                phrase.ConsoleMouseEnterEvent -= OnPhraseMouseEnter;
                phrase.ConsoleMouseLeaveEvent -= OnPhraseMouseLeave;
            }
        }

        private static void SetUrlPhraseColor(Phrase urlPhrase, ConsoleColor color, int nextCurLeft, int nextCurTop)
        {
            Console.SetCursorPosition(urlPhrase.PosLeft + urlPhrase.Match.Groups[2].Index, urlPhrase.PosTop);
            Console.ForegroundColor = color;
            Console.Write(urlPhrase.Match.Groups[2].Value);
            Console.SetCursorPosition(nextCurLeft, nextCurTop);
        }

        private static void SetPhraseColor(Phrase phrase, ConsoleColor color, int nextCurLeft, int nextCurTop)
        {
            Console.SetCursorPosition(phrase.PosLeft, phrase.PosTop);
            Console.ForegroundColor = color;
            Console.Write(phrase.Match.Value);
            Console.SetCursorPosition(nextCurLeft, nextCurTop);
        }
          
        private static void OnPhraseMouseUp(Phrase sender, ConsoleMouseEventArgs e)
        {
            try
            {
                if (sender.Pattern == url) SetUrlPhraseColor(sender, ConsoleColor.DarkCyan, curLeft, curTop);                
            }
            catch 
            { }
        }

        private static void OnPhraseMouseDown(Phrase sender, ConsoleMouseEventArgs e)
        {
            try
            {
                if (sender.Pattern == url) SetUrlPhraseColor(sender, ConsoleColor.Blue, curLeft, curTop);
            }
            catch { }
        }

        private static void OnPhraseMouseClick(Phrase sender, ConsoleMouseEventArgs e)
        {
            try
            {
                if (sender.Pattern == url) Process.Start(sender.Match.Groups[2].Value);
            }
            catch { }
        }

        private static void OnPhraseMouseEnter(Phrase sender, ConsoleMouseEventArgs e)
        {
            try
            {
                if (sender.Pattern == word) SetPhraseColor(sender, ConsoleColor.DarkMagenta, curLeft, curTop);
                else if (sender.Pattern == sector) SetPhraseColor(sender, ConsoleColor.Green, curLeft, curTop);
                else if (sender.Pattern == url) SetUrlPhraseColor(sender, ConsoleColor.DarkCyan, curLeft, curTop);
            }
            catch { }
        }

        private static void OnPhraseMouseLeave(Phrase sender, ConsoleMouseEventArgs e)
        {
            try
            {
                if (sender.Pattern == url) SetUrlPhraseColor(sender, ConsoleColor.DarkCyan, curLeft, curTop);
                else if (sender.Pattern == word) SetPhraseColor(sender, ConsoleColor.DarkYellow, curLeft, curTop);
            }
            catch { }
        }
    }
}
