using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;

namespace TestCS
{
    public static class Logger
    {
        public enum LogLevel
        {
            Verbose = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }

        [DllImport("RDONatives.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Logger_Init(string projectName, string settingsPath, bool attachConsole);

        [DllImport("RDONatives.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Logger_Destroy();

        [DllImport("RDONatives.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Logger_ToggleConsole(bool toggle);

        [DllImport("RDONatives.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Logger_Log(int level, string message);

        public static bool Init(string projectName, string settingsPath = "/settings.js", bool attachConsole = true)
        {
            Console.WriteLine($"Attempting to initialize logger. Project: {projectName}, Settings: {settingsPath}, AttachConsole: {attachConsole}");
            try
            {
                bool result = Logger_Init(projectName, settingsPath, attachConsole);
                if (!result)
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Logger_Init failed. Error code: {error}");
                }
                else
                {
                    Console.WriteLine("Logger_Init succeeded");
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Logger.Init: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public static void Destroy()
        {
            try
            {
                Logger_Destroy();
                Console.WriteLine("Logger_Destroy called successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Logger.Destroy: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        public static void ToggleConsole(bool enable)
        {
            try
            {
                Logger_ToggleConsole(enable);
                Console.WriteLine($"Logger_ToggleConsole called with parameter: {enable}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Logger.ToggleConsole: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        public static async Task<bool> InitAsync(string projectName, string settingsPath = "/settings.js", bool attachConsole = true)
        {
            Console.WriteLine($"Attempting to initialize logger. Project: {projectName}, Settings: {settingsPath}, AttachConsole: {attachConsole}");
            try
            {
                return await Task.Run(() => Logger_Init(projectName, settingsPath, attachConsole));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Logger.InitAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public static Task DestroyAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    Logger_Destroy();
                    Console.WriteLine("Logger_Destroy called successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in Logger.DestroyAsync: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                }
            });
        }

        public static Task ToggleConsoleAsync(bool enable)
        {
            return Task.Run(() =>
            {
                try
                {
                    Logger_ToggleConsole(enable);
                    Console.WriteLine($"Logger_ToggleConsole called with parameter: {enable}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in Logger.ToggleConsoleAsync: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                }
            });
        }


        public static class LOG
        {
            public static LogLevel MinimumLogLevel { get; set; } = LogLevel.Info;

            public static void VERBOSE(string message,
                [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0,
                [CallerMemberName] string member = "")
            {
                LogWithLevel(LogLevel.Verbose, message, file, line, member);
            }

            public static void INFO(string message,
                [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0,
                [CallerMemberName] string member = "")
            {
                LogWithLevel(LogLevel.Info, message, file, line, member);
            }

            public static void WARNING(string message,
                [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0,
                [CallerMemberName] string member = "")
            {
                LogWithLevel(LogLevel.Warning, message, file, line, member);
            }

            public static void ERROR(string message,
                [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0,
                [CallerMemberName] string member = "")
            {
                LogWithLevel(LogLevel.Error, message, file, line, member);
            }

            private static void LogWithLevel(LogLevel level, string message, string file, int line, string member)
            {
                if (level >= MinimumLogLevel)
                {
                    try
                    {
                        string formattedMessage = FormatMessage(message, file, line, member);
                        Logger_Log((int)level, formattedMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception in Logger.LOG.LogWithLevel: {ex.Message}");
                        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    }
                }
            }

            public static Task VERBOSEAsync(string message,
                [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0,
                [CallerMemberName] string member = "")
            {
                return LogWithLevelAsync(LogLevel.Verbose, message, file, line, member);
            }

            public static Task INFOAsync(string message,
                [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0,
                [CallerMemberName] string member = "")
            {
                return LogWithLevelAsync(LogLevel.Info, message, file, line, member);
            }

            public static Task WARNINGAsync(string message,
                [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0,
                [CallerMemberName] string member = "")
            {
                return LogWithLevelAsync(LogLevel.Warning, message, file, line, member);
            }

            public static Task ERRORAsync(string message,
                [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0,
                [CallerMemberName] string member = "")
            {
                return LogWithLevelAsync(LogLevel.Error, message, file, line, member);
            }

            private static Task LogWithLevelAsync(LogLevel level, string message, string file, int line, string member)
            {
                if (level >= MinimumLogLevel)
                {
                    return Task.Run(() =>
                    {
                        try
                        {
                            string formattedMessage = FormatMessage(message, file, line, member);
                            Logger_Log((int)level, formattedMessage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception in Logger.LOG.LogWithLevelAsync: {ex.Message}");
                            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                        }
                    });
                }
                return Task.CompletedTask;
            }

            private static string FormatMessage(string message, string file, int line, string member)
            {
                return $"[{Path.GetFileName(file)}:{line} - {member}] {message}";
            }
        }
    }
}