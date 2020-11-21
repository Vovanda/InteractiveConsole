using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace InteractiveConsole
{
    public class Phrase
    {
        public static Phrase FindInPosition(int cursorLeft, int cursorTop, ref Phrase phrase)
        {
            Phrase result = null;
            try
            {
                phrase.Match = Match.Empty;
                string line = "";
                int top_shiht = 0, bottom_shiht = 0;
                int buf_width = Console.WindowWidth;

                bool isStop = false;
                while (!isStop)
                {
                    line = ConsoleBuffReader.ReadLine((short)(cursorTop - top_shiht)) + line;
                    if (bottom_shiht > 0)
                    {
                        line += ConsoleBuffReader.ReadLine((short)(cursorTop + bottom_shiht));
                    }

                    var mainPattern = new Regex($@".*\s{{{Console.WindowHeight}}}");
                    var parentMatches = mainPattern.IsMatch(line);
                    isStop = mainPattern.IsMatch(line);
                    bool success = false;
                    foreach (Match childMatch in phrase.Pattern.Matches(line))
                    {
                        int clickIndex = cursorLeft + top_shiht * buf_width;
                        success = childMatch.Index <= clickIndex && clickIndex < childMatch.Index + childMatch.Length;
                        if (success)
                        {
                            phrase.PosLeft = childMatch.Index - top_shiht * buf_width;
                            phrase.PosTop = cursorTop - top_shiht;
                            phrase.Match = childMatch;
                            break;
                        }
                    }

                    if (success)
                    {
                        isStop = true;
                        break;
                    }

                    if (top_shiht < cursorTop) top_shiht++;
                    bottom_shiht++;
                    if (top_shiht + bottom_shiht++ > Math.Min(1000, Console.BufferHeight)) isStop = true;
                }
            }
            catch (Exception e)
            {
                result = null;
                Debug.WriteLine(e.Message);
            }

            return result;
        }

        public Phrase(Regex pattern) : this(pattern, 0, 0, Match.Empty) { }
        public Phrase(Regex pattern, int posLeft, int posTop, Match match)
        {
            eventsDict = new Dictionary<PhraseEvents, ConsoleMouseEvent>();
            PosLeft = posLeft;
            PosTop = posTop;
            Pattern = pattern;
            Match = match;
            IsSuccess = match.Success;
        }


        public event ConsoleMouseEvent ConsoleMouseUpEvent
        {
            add => AddMouseEvent(PhraseEvents.MouseUp, value);
            remove => RemoveMouseEvent(PhraseEvents.MouseUp, value);
        }

        public event ConsoleMouseEvent ConsoleMouseDownEvent
        {
            add => AddMouseEvent(PhraseEvents.MouseDown, value);
            remove => RemoveMouseEvent(PhraseEvents.MouseDown, value);
        }

        public event ConsoleMouseEvent ConsoleMouseClickEvent
        {
            add => AddMouseEvent(PhraseEvents.MouseClick, value);
            remove => RemoveMouseEvent(PhraseEvents.MouseClick, value);
        }

        public event ConsoleMouseEvent ConsoleMouseMoveEvent
        {
            add => AddMouseEvent(PhraseEvents.MouseMove, value);
            remove => RemoveMouseEvent(PhraseEvents.MouseMove, value);
        }

        public event ConsoleMouseEvent ConsoleMouseEnterEvent
        {
            add => AddMouseEvent(PhraseEvents.MouseEnter, value);
            remove => AddMouseEvent(PhraseEvents.MouseLeave, value);
        }

        public event ConsoleMouseEvent ConsoleMouseLeaveEvent
        {
            add => AddMouseEvent(PhraseEvents.MouseLeave, value);
            remove => RemoveMouseEvent(PhraseEvents.MouseLeave, value);
        }

        internal void MouseEventSignal(PhraseEvents phraseEvent, ConsoleMouseEventArgs e)
        {
            if (eventsDict.ContainsKey(phraseEvent))
                eventsDict[phraseEvent]?.Invoke(this, e);

            // Mouse Enter/Leave logic
            if (phraseEvent == PhraseEvents.MouseEnter && !IsMouseEnter) IsMouseEnter = true;  
            if (phraseEvent == PhraseEvents.MouseLeave && IsMouseEnter) IsMouseEnter = false;            
        }

        private void AddMouseEvent(PhraseEvents eventName, ConsoleMouseEvent value)
        {
            if (eventsDict.ContainsKey(eventName))
                eventsDict[eventName] += value;
            else
                eventsDict.Add(eventName, value);
        }
         private void RemoveMouseEvent(PhraseEvents eventName, ConsoleMouseEvent value)
         {
            if (eventsDict.ContainsKey(eventName))
                eventsDict[eventName] -= value;
         }
        

        public bool IsSuccess { get; set; }
        public string Value => Match?.Value;
        public int PosLeft;
        public int PosTop;
        public Regex Pattern;
        public Match Match 
        {
            get => match;
            set
            {
                match = value;
                IsSuccess = match.Success;
            } 
        }

        private Match match;
        internal bool IsMouseEnter { get; private set; }

        private readonly Dictionary<PhraseEvents, ConsoleMouseEvent> eventsDict;

        public delegate void ConsoleMouseEvent(Phrase sender, ConsoleMouseEventArgs e);
    }

    public struct ConsoleMouseEventArgs
    {
        public ConsoleMouseEventArgs(int cursorLeft, int cursorTop, MButton button)
        {
            CursorLeft = cursorLeft;
            CursorTop = cursorTop;
            Button = button;
        }

        public int CursorLeft;
        public int CursorTop;
        public MButton Button;
    }

    public enum PhraseEvents
    {
        MouseUp,
        MouseDown,
        MouseClick,
        MouseMove,
        MouseEnter,
        MouseLeave
    }
}
