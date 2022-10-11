using System;

namespace Notepad.GUI.Example
{
    internal static class Program
    {
        private sealed class Logger : IProgress<string>
        {
            public void Report(string message)
            {
                Console.WriteLine($"Client: {message}");
            }
        }

        public static int Main(string[] args)
        {
            using var client = NotepadClient.Initialize(new ClientCreationOptions
            {
                CreationOptions = NotepadCreationOptions.IfNotFound,
                OwnershipOptions = NotepadOwnershipOptions.IfCreated,
                Logger = new Logger()
            });

            return 0;
        }
    }
}