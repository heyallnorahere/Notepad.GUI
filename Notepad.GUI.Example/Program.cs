﻿/*
   Copyright 2022 Nora Beda

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

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