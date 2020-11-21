using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InteractiveConsole
{
    // https://www.medo64.com/2013/05/console-mouse-input-in-c/
    public class ConsoleListener
    {
        public static event ConsoleKeyEvent KeyEvent;

        public static event ConsoleWindowBufferSizeEvent WindowBufferSizeEvent;

        public static List<Phrase> IntaractivePhrases = new List<Phrase>();

        private static bool Run = false;

        public static void Start()
        {
            if (!Run)
            {
                Run = true;

                Task.Factory.StartNew(() =>
                {
                    IntPtr handleIn = NativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE);

                    uint mode = 0;
                    NativeMethods.GetConsoleMode(handleIn, ref mode);

                    mode = ~NativeMethods.ENABLE_QUICK_EDIT_MODE; //disable
                    mode |= NativeMethods.ENABLE_MOUSE_INPUT; //enable

                    NativeMethods.SetConsoleMode(handleIn, mode);

                    while (true)
                    {
                        uint numRead = 0;
                        NativeMethods.INPUT_RECORD[] record = new NativeMethods.INPUT_RECORD[1];
                        record[0] = new NativeMethods.INPUT_RECORD();
                        NativeMethods.ReadConsoleInput(handleIn, record, 1, ref numRead);
                        if (Run)
                        {
                            switch (record[0].EventType)
                            {
                                case NativeMethods.INPUT_RECORD.MOUSE_EVENT:
                                    MouseEventInvoke(record[0].MouseEvent);
                                    break;
                                case NativeMethods.INPUT_RECORD.KEY_EVENT:
                                    KeyEvent?.Invoke(record[0].KeyEvent);
                                    break;
                                case NativeMethods.INPUT_RECORD.WINDOW_BUFFER_SIZE_EVENT:
                                    WindowBufferSizeEvent?.Invoke(record[0].WindowBufferSizeEvent);
                                    break;
                            }
                        }
                        else
                        {
                            uint numWritten = 0;
                            NativeMethods.WriteConsoleInput(handleIn, record, 1, ref numWritten);
                            return;
                        }
                    }
                });
            }
        }

        private static void MouseEventInvoke(NativeMethods.MOUSE_EVENT_RECORD record)
        {
            MButton mBtnCurrentState = MButton.None;
            if (record.dwButtonState >= 0 && record.dwButtonState <= 3)
            {
                mBtnCurrentState = (MButton)(2 * record.dwButtonState);
            }
            var phrases = new List<Phrase>();

            for(int i = 0; i < IntaractivePhrases.Count; i++)
            {
                var phrase = IntaractivePhrases[i];
                if (phrase != null)
                {
                    var tempPhrase = new Phrase(phrase.Pattern);

                    Phrase.FindInPosition(record.dwMousePosition.X, record.dwMousePosition.Y, ref tempPhrase);

                    if (tempPhrase.IsSuccess)
                    {
                        phrase.PosLeft = tempPhrase.PosLeft;
                        phrase.PosTop = tempPhrase.PosTop;
                        phrase.Match = tempPhrase.Match;
                        phrases.Add(phrase);
                    }
                    else
                    {
                        phrase.IsSuccess = false;
                    }
                }
            }

            //MouseMove
            if (!MPosition.Equals(record.dwMousePosition))
            {
                MPosition = record.dwMousePosition;
                PhraseEventSignal(PhraseEvents.MouseMove, new ConsoleMouseEventArgs(MPosition.X, MPosition.Y, mBtnCurrentState));
                #region Mouse Enter/Leave logic

                for (int i = 0; i < phrases.Count; i++)
                {
                    if(!phrases[i].IsMouseEnter)
                    {
                        phrases[i].MouseEventSignal(PhraseEvents.MouseEnter, new ConsoleMouseEventArgs(MPosition.X, MPosition.Y, mBtnCurrentState));
                    }
                }
                
                for (int i = 0; i < IntaractivePhrases.Count; i++)
                {
                    if(IntaractivePhrases[i] != null && !IntaractivePhrases[i].IsSuccess && IntaractivePhrases[i].IsMouseEnter)
                    {
                        IntaractivePhrases[i].MouseEventSignal(PhraseEvents.MouseLeave, new ConsoleMouseEventArgs(MPosition.X, MPosition.Y, mBtnCurrentState));
                    }
                }
                #endregion
            }

            //Left MouseDown
            if (MButtonPreState == MButton.None && (mBtnCurrentState & MButton.Left) == MButton.Left)
            {
                IsLeftMButtonDown = true;
                PhraseEventSignal(PhraseEvents.MouseDown, new ConsoleMouseEventArgs(MPosition.X, MPosition.Y, MButton.Left));
            }

            //Right MouseDown
            if (MButtonPreState == MButton.None && (mBtnCurrentState & MButton.Right) == MButton.Right)
            {
                IsRightMButtonDown = true;
                PhraseEventSignal(PhraseEvents.MouseDown, new ConsoleMouseEventArgs(MPosition.X, MPosition.Y, MButton.Right));
            }
            //Right MouseUp / MouseClick
            if ((MButtonPreState & MButton.Left) == MButton.Left && (mBtnCurrentState & MButton.Left) != MButton.Left)
            {
                PhraseEventSignal(PhraseEvents.MouseUp, new ConsoleMouseEventArgs(MPosition.X, MPosition.Y, MButton.Left));
                if (IsLeftMButtonDown)
                {
                    PhraseEventSignal(PhraseEvents.MouseClick, new ConsoleMouseEventArgs(MPosition.X, MPosition.Y, MButton.Left));
                    IsLeftMButtonDown = false;
                }
            }

            //Left MouseUp / MouseClick
            if ((MButtonPreState & MButton.Right) == MButton.Right && (mBtnCurrentState & MButton.Right) != MButton.Right)
            {
                PhraseEventSignal(PhraseEvents.MouseUp, new ConsoleMouseEventArgs(MPosition.X, MPosition.Y, MButton.Left));
                if (IsRightMButtonDown)
                {
                    PhraseEventSignal(PhraseEvents.MouseClick, new ConsoleMouseEventArgs(MPosition.X, MPosition.Y, MButton.Left));
                    IsRightMButtonDown = false;
                }
            }
            MButtonPreState = mBtnCurrentState;

            void PhraseEventSignal(PhraseEvents eventName, ConsoleMouseEventArgs e)
            {
                for(int i=0; i < phrases.Count; i++)
                {
                    phrases[i].MouseEventSignal(eventName, e);
                }
            }
        }
              
        private static MButton MButtonPreState;
        private static NativeMethods.COORD MPosition;

        private static bool IsLeftMButtonDown = false;
        private static bool IsRightMButtonDown = false;
        public static void Stop() => Run = false;

        public delegate void ConsoleMouseEvent(NativeMethods.MOUSE_EVENT_RECORD r);

        public delegate void ConsoleKeyEvent(NativeMethods.KEY_EVENT_RECORD r);

        public delegate void ConsoleWindowBufferSizeEvent(NativeMethods.WINDOW_BUFFER_SIZE_RECORD r);

    }

    [Flags]
    public enum MButton : int
    {
        None = 0,
        Left = 2,
        Right = 4
    }
}
