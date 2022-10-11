using Notepad.GUI.Implementations;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Notepad.GUI
{
    public enum NotepadCreationOptions
    {
        Always,
        IfNotFound,
        Never
    }

    public enum NotepadOwnershipOptions
    {
        Always,
        IfCreated,
        Never
    }

    public sealed class ClientCreationOptions
    {
        public ClientCreationOptions()
        {
            CreationOptions = NotepadCreationOptions.IfNotFound;
            OwnershipOptions = NotepadOwnershipOptions.IfCreated;
            Logger = null;
        }

        /// <summary>
        /// Configure how the client should decide to start up a new notepad process, or use an existing one
        /// </summary>
        public NotepadCreationOptions CreationOptions { get; set; }

        /// <summary>
        /// Configure in what circumstances the client should shut down the attached process, when it's disposed
        /// </summary>
        public NotepadOwnershipOptions OwnershipOptions { get; set; }

        /// <summary>
        /// An optional way to attach a logger-like object
        /// </summary>
        public IProgress<string>? Logger { get; set; }
    }

    /// <summary>
    /// An object used to interface with a Notepad (or a basic text editor for any other supported platforms) window.
    /// </summary>
    public abstract class NotepadClient : IDisposable
    {
        private static NotepadClient CreateInstance()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsClient();
            }

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Creates a new <see cref="NotepadClient"/> to interface with Notepad.
        /// </summary>
        /// <param name="options">Options to specify how this client connects.</param>
        /// <returns>The created client. Make sure to dispose the object once you're done with it.</returns>
        /// <exception cref="PlatformNotSupportedException" />
        public static NotepadClient Initialize(ClientCreationOptions? options = null)
        {
            var instance = CreateInstance();
            instance.mOptions = options ?? new ClientCreationOptions();

            instance.Initialize();
            return instance;
        }

        internal NotepadClient()
        {
            mDisposed = false;
            mOptions = new ClientCreationOptions();

            mProcessCreated = false;
            mProcess = null;
        }


        ~NotepadClient() => Dispose(false);
        public void Dispose()
        {
            if (mDisposed)
            {
                return;
            }

            Dispose(true);
            mDisposed = true;

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (mProcess != null)
            {
                mProcess.Exited -= ProcessExited;

                bool killProcess = mOptions.OwnershipOptions switch
                {
                    NotepadOwnershipOptions.Always => true,
                    NotepadOwnershipOptions.IfCreated => mProcessCreated,
                    NotepadOwnershipOptions.Never => false,
                    _ => throw new ArgumentException("Invalid ownership type!")
                };

                if (killProcess)
                {
                    mProcess.Kill();
                }

                if (disposing)
                {
                    mProcess.Dispose();
                }
            }
        }

        private void ProcessExited(object? sender, EventArgs args) => OnExit?.Invoke();
        private void Initialize()
        {
            bool createProcess = false;
            if (mOptions.CreationOptions == NotepadCreationOptions.Always)
            {
                createProcess = true;
            }
            else
            {
                var process = FindProcess();
                if (process != null)
                {
                    mProcess = process;
                    Logger?.Report($"Found process: {mProcess.ProcessName} (PID: {mProcess.Id})");
                }
                else if (mOptions.CreationOptions == NotepadCreationOptions.IfNotFound)
                {
                    createProcess = true;
                }
                else
                {
                    throw new Exception("No Notepad process found!");
                }
            }

            if (createProcess)
            {
                mProcess = CreateProcess();
                mProcessCreated = true;

                Logger?.Report($"Started process: {mProcess.ProcessName} (PID: {mProcess.Id})");
            }
            else
            {
                mProcessCreated = false;
            }

            if (mProcess == null)
            {
                throw new Exception("Something went wrong here");
            }

            mProcess.EnableRaisingEvents = true;
            mProcess.Exited += ProcessExited;

            Connect();
        }

        /// <summary>
        /// Writes a block of text to the buffer.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="offset">The buffer offset to write at. Use <see cref="GetBufferOffset(int, int)"/> or <see cref="GetBufferOffset(Vector2)"/> to compute.</param>
        public abstract void Write(string data, int offset);

        /// <summary>
        /// Compute a buffer offset to use with <see cref="Write(string, int)"/>.
        /// </summary>
        /// <param name="position">The position at which to write.</param>
        /// <returns>The buffer offset.</returns>
        public int GetBufferOffset(Vector2 position) => GetBufferOffset(position.X, position.Y);

        /// <summary>
        /// Compute a buffer offset to use with <see cref="Write(string, int)"/>.
        /// </summary>
        /// <param name="x">The x coordinate at which to write.</param>
        /// <param name="y">The y coordinate at which to write.</param>
        /// <returns>The buffer offset.</returns>
        public virtual int GetBufferOffset(int x, int y) => (y * BufferSize.X) + x;

        protected abstract Process CreateProcess();
        protected abstract Process? FindProcess();
        protected abstract void Connect();

        /// <summary>
        /// The object that this client should funnel debug messages to.
        /// </summary>
        public IProgress<string>? Logger => mOptions.Logger;

        /// <summary>
        /// The process that this client is connected to.
        /// </summary>
        public Process Process => mProcess!;

        /// <summary>
        /// This event is raised when the Notepad process exits.
        /// </summary>
        public event Action? OnExit;

        /// <summary>
        /// The available drawing space.
        /// </summary>
        public abstract Vector2 BufferSize { get; }

        private bool mDisposed;
        private ClientCreationOptions mOptions;

        private bool mProcessCreated;
        private Process? mProcess;
    }
}
