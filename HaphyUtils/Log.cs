using Godot;
using System.Runtime.CompilerServices;
using System.Text;

namespace HaphyUtils
{
    public static class Log
    {
        public static LogLevel ConsoleLevel { get; set; } = LogLevel.Debug;
        public static LogLevel FileLevel { get; set; } = LogLevel.Debug;
        public static bool PushErrors { get; set; } = true;
        private static string Timestamp { get { return "[" + DateTime.Now.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "]"; } }
        private static string ConsoleTimestamp { get { return "[" + DateTime.Now.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + "]"; } }

        private static Stream? _stream;
        private static StreamWriter? _streamWriter;
        private static bool _useLogs = true;

        public static void Configure(LogLevel consoleLevel = LogLevel.Debug, LogLevel fileLevel = LogLevel.Debug, bool pushErrors = true)
        {
            ConsoleLevel = consoleLevel;
            FileLevel = fileLevel;
            PushErrors = pushErrors;

            Stream? logstream = GetLogStream();
            if (logstream == null)
            {
                Warn("Could not aquire logging stream.");
                _useLogs = false;
            } else
            {
                _stream = logstream;
                _streamWriter = new StreamWriter(_stream, Encoding.UTF8);
                _streamWriter.AutoFlush = true;
            }

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Fatal(@"Fatal exception occurred: " + e.ExceptionObject.ToString());
        }

        public static void Dispose()
        {
            if (_useLogs)
            {
                _streamWriter?.Dispose();
                _stream?.Dispose();
            }
        }

        public static void Debug(string message, [CallerFilePath] string sourcePath = @"", [CallerLineNumber] int sourceLine = 0)
        {
#if DEBUG
            Write(sourcePath, sourceLine, LogLevel.Debug, message);
#endif
        }

        public static void Info(string message, [CallerFilePath] string sourcePath = @"", [CallerLineNumber] int sourceLine = 0) => 
            Write(sourcePath, sourceLine, LogLevel.Info, message);

        public static void Warn(string message, [CallerFilePath] string sourcePath = @"", [CallerLineNumber] int sourceLine = 0) => 
            Write(sourcePath, sourceLine, LogLevel.Warn, message);

        public static void Error(string message, [CallerFilePath] string sourcePath = @"", [CallerLineNumber] int sourceLine = 0) => 
            Write(sourcePath, sourceLine, LogLevel.Error, message);

        public static void Alert(string message, [CallerFilePath] string sourcePath = @"", [CallerLineNumber] int sourceLine = 0) => 
            Write(sourcePath, sourceLine, LogLevel.Alert, message);

        public static void Fatal(string message, [CallerFilePath] string sourcePath = @"", [CallerLineNumber] int sourceLine = 0) => 
            Write(sourcePath, sourceLine, LogLevel.Fatal, message);

        private static Stream? GetLogStream()
        {
            string dir = OS.GetUserDataDir();
            string dir2 = Path.Combine(dir, "logs");
            Directory.CreateDirectory(dir2);

            //keep 4 logs on hand
            string lp3 = Path.Combine(dir2, "3.oldest.txt");
            string lp2 = Path.Combine(dir2, "2.older.txt");
            string lp1 = Path.Combine(dir2, "1.previous.txt");
            string lp0 = Path.Combine(dir2, "0.current.txt");
            if (File.Exists(lp3)) File.Delete(lp3);
            if (File.Exists(lp2)) File.Move(lp2, lp3);
            if (File.Exists(lp1)) File.Move(lp1, lp2);
            if (File.Exists(lp0)) File.Move(lp0, lp1);

            try
            {
                Stream logStream = new FileStream(lp0, FileMode.Create);
                return logStream;
            } catch (Exception)
            {
                //do nothing, its probably just in use by another instance of the program
            }
            return null;
        }

        private static void Write(string sourcePath, int sourceLine, LogLevel level, string message)
        {
            StringBuilder sb = new();
            sb.Append(GetLabelForLevel(level));
            if (!string.IsNullOrWhiteSpace(sourcePath))
            {
                sb.Append("[");
                sb.Append(Path.GetFileNameWithoutExtension(sourcePath));
                sb.Append(":").Append(sourceLine).Append("] ");
            } else
            {
                sb.Append(' ');
            }
            sb.Append(message);
            string output = sb.ToString();
            if(ConsoleLevel <= level)
            {
                if (PushErrors && (level == LogLevel.Warn || level == LogLevel.Alert))
                {
                    GD.PushWarning(ConsoleTimestamp + output);
                }
                else if (PushErrors && (level == LogLevel.Error || level == LogLevel.Fatal))
                {
                    GD.PushError(ConsoleTimestamp + output);
                }
                else
                {
                    GD.PrintRich(GetPrefixForLevel(level) + ConsoleTimestamp + output);
                }
            }

            if (_useLogs)
            {
                if(FileLevel <= level)
                {
                    _streamWriter?.WriteLine(Timestamp + output);
                }
            }
        }

        private static string GetLabelForLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => "[DEBUG]",
                LogLevel.Warn => "[WARN]",
                LogLevel.Error => "[ERROR]",
                LogLevel.Alert => "[ALERT]",
                LogLevel.Fatal => "[FATAL]",
                _ => string.Empty
            };
        }

        private static string GetPrefixForLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => "[color=darkgray]",
                LogLevel.Info => "[color=white]",
                LogLevel.Warn => "[color=yellow]",
                LogLevel.Error => "[color=darkred]",
                LogLevel.Alert => "[color=magenta]",
                LogLevel.Fatal => "[color=red]",
                _ => string.Empty
            };
        }
    }
}
