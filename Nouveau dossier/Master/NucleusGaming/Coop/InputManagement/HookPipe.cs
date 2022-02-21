using Nucleus.Gaming.Coop.InputManagement.Logging;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Nucleus.Gaming.Coop.InputManagement
{
    /*	Messages outgoing from NamedPipe.cs to Hook.cpp:

		* 0x01: add DELTA cursor pos. param1=x, param2=y

		* 0x02: Set VKEY (Virtual Key). For GetKeyState/GetAsyncKeyState/GetKeyboardState 
			param 1 = key. Mouse buttons: 1,2,4,5,6.
			param 2 = on off: 0 = off, 1 = on

		* 0x03: close named pipe

		* 0x04: set ABSOLUTE cursor pos. param1=x, param2=y

		* 0x05: set desktop as foreground window
	*/
    public class HookPipe
    {
        public readonly string pipeNameRead;//Read FROM Hooks
        public readonly string pipeNameWrite;//Written from Hooks

        private Thread serverThread;
        private Thread receiveThread;
        private NamedPipeServerStream pipeServerRead;//Read FROM Hooks
        private NamedPipeServerStream pipeServerWrite;//Written from Hooks

        private MemoryMappedFile sharedMem;
        public MemoryMappedViewAccessor sharedMemView;
        private readonly string sharedMemName;
        private const long sharedMemSize = 4 * 4;

        private IntPtr pipeServerReadHandle = (IntPtr)(-1);
        private bool clientConnected = false;
        private IntPtr hWnd;
        private Window window;
        private int toSendDeltaX, toSendDeltaY, toSendAbsX, toSendAbsY;
        private bool haveMousePosToSend;
        private bool loopThreadWaitingForMousePosToSend;
        private ManualResetEventSlim xyResetEvent = new ManualResetEventSlim(false);

        private Action onClosed;

        private GenericGameInfo gameInfo;

        private int sequentialErrorCounter = 0;
        private const int numSequentialErrorsBeforeAssumeDead = 10;

        public HookPipe(IntPtr hWnd, Window window, bool needWritePipe, Action onClosed, GenericGameInfo gameInfo)
        {
            pipeNameRead = GenerateName();
            if (needWritePipe)
            {
                pipeNameWrite = GenerateName();
            }

            sharedMemName = pipeNameRead + "_mem";

            this.hWnd = hWnd;
            this.window = window;
            this.onClosed = onClosed;
            this.gameInfo = gameInfo;

            serverThread = new Thread(Start)
            {
                IsBackground = true
            };
            serverThread.Start();

            if (needWritePipe)
            {
                receiveThread = new Thread(ReceiveMessages)
                {
                    IsBackground = true
                };
                receiveThread.Start();
            }

            StartSharedMemory();
        }

        private void Start()
        {
            Logger.WriteLine($"Starting read pipe {pipeNameRead}");
            pipeServerRead = new NamedPipeServerStream(pipeNameRead, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
            Logger.WriteLine($"Created read pipe {pipeNameRead}");

            try
            {
                pipeServerRead.WaitForConnection();
                pipeServerReadHandle = pipeServerRead.SafePipeHandle.DangerousGetHandle();
            }
            catch (Exception e)
            {
                Logger.WriteLine($"Exception while waiting for pipe client to connect (read): {e}");
                return;
            }

            clientConnected = true;
            Logger.WriteLine($"Client connected to (read) pipe {pipeNameRead}");



            // If legacy input is enabled, Hooks needs to know 
            // - Delta changes in mouse position (since last input). There is no bounding on this as it is used for first person camera movement.
            // - Absolute mouse position (this is bound from 0,0 to width,height). This is used in the menus.
            //If legacy input is disabled, Delta changes aren't used for first person camera movement (e.g. it uses raw input) so it doesn't need to be sent.

            /*bool sendDelta = gameInfo.HookUseLegacyInput;

			if (sendDelta || gameInfo.HookGetCursorPos)
			{
				//With this system, input messages are only sent as fast as one thread can manage.
				while (clientConnected)
				{
					//SpinWait.SpinUntil(() => haveMousePosToSend);
					if (!haveMousePosToSend)
					{
						loopThreadWaitingForMousePosToSend = true;
						xyResetEvent.Wait(); //Wait for an mouse input message to have altered toSendDeltaX/Y
					}

					if (sendDelta)
					{
						WriteMessageNow(0x01, toSendDeltaX, toSendDeltaY);
						toSendDeltaX = 0;
						toSendDeltaY = 0;
					}
					WriteMessageNow(0x04, toSendAbsX, toSendAbsY);
					//TODO: can error when closed

					haveMousePosToSend = false;

					if (loopThreadWaitingForMousePosToSend)
					{
						loopThreadWaitingForMousePosToSend = false;
						xyResetEvent.Reset(); //Reset the event (or WaitOne passes immediately)
					}
				}
			}*/
        }

        private void ReceiveMessages()
        {
            Logger.WriteLine($"Starting write pipe {pipeNameWrite}");
            pipeServerWrite = new NamedPipeServerStream(pipeNameWrite, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
            Logger.WriteLine($"Created write pipe {pipeNameWrite}");

            try
            {
                pipeServerWrite.WaitForConnection();
            }
            catch (Exception e)
            {
                Logger.WriteLine($"Exception while waiting for pipe client to connect (write): {e}");
                return;
            }

            Logger.WriteLine($"Client connected to (write) pipe {pipeNameWrite}");

            while (clientConnected)
            {
                try
                {
                    byte[] buffer = new byte[9];
                    pipeServerWrite.Read(buffer, 0, 9);
                    int msg = buffer[0];

                    if (msg == 0x06)
                    {
                        window.CursorVisibility = (buffer[4] == 1);
                    }

                    sequentialErrorCounter = 0;
                }
                catch
                {
                    CountError();
                }
            }
            Console.WriteLine("ReceiveMessages END");
        }

        private void StartSharedMemory()
        {
            sharedMem = MemoryMappedFile.CreateNew(
                sharedMemName,
                sharedMemSize,
                MemoryMappedFileAccess.ReadWrite,
                MemoryMappedFileOptions.None,
                HandleInheritability.None);

            Logger.WriteLine($"Created shared memory, name=\"{sharedMemName}\", size={sharedMemSize}");

            sharedMemView = sharedMem.CreateViewAccessor();
        }

        /// <summary>
        /// Updates the mouse position to send, and makes the loop know there is data to be sent.
        /// (Doesn't immediately send a message)
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <param name="absoluteX"></param>
        /// <param name="absoluteY"></param>
        public void SendMousePosition(int deltaX, int deltaY, int absoluteX, int absoluteY)
        {
            /*toSendDeltaX += deltaX;
			toSendDeltaY += deltaY;
			toSendAbsX = absoluteX;
			toSendAbsY = absoluteY;

			haveMousePosToSend = true;

			if (loopThreadWaitingForMousePosToSend)
				xyResetEvent.Set();*/

            //Data packed as absX; absY; deltaX; deltaY
            //CPP side will set deltas to zero when read

            //TODO: use underlying calls for optimisation
            int prevDeltaX = sharedMemView.ReadInt32(8);
            int prevDeltaY = sharedMemView.ReadInt32(12);

            int[] newData = { absoluteX, absoluteY, prevDeltaX + deltaX, prevDeltaY + deltaY };
            sharedMemView.WriteArray(0, newData, 0, 4);
        }

        /// <summary>
        /// When an process has the focus, another process (Nucleus) cannot set the foreground window.
        /// This sends a message to the hooks to set the focus to the desktop.
        /// </summary>
        public void SendPreventForegroundWindow()
        {
            WriteMessage(0x05, 0, 0);
        }

        /// <summary>
        /// Queues a send message via the ThreadPool.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        public void WriteMessage(byte message, int param1, int param2)
        {
            if (clientConnected)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    byte[] bytes = {
                        message,
                        (byte)(param1 >> 24), (byte)(param1 >> 16), (byte)(param1 >> 8), (byte)param1,
                        (byte)(param2 >> 24), (byte)(param2 >> 16), (byte)(param2 >> 8), (byte)param2
                    };

                    try
                    {
                        pipeServerRead?.Write(bytes, 0, 9);
                        sequentialErrorCounter = 0;
                    }
                    catch (Exception)
                    {
                        CountError();
                    }
                });
            }
        }

        /// <summary>
        /// Immediately sends a message via the named pipe on the current thread.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        private void WriteMessageNow(byte message, int param1, int param2)
        {
            byte[] bytes = {
                message,
                (byte)(param1 >> 24), (byte)(param1 >> 16), (byte)(param1 >> 8), (byte)param1,
                (byte)(param2 >> 24), (byte)(param2 >> 16), (byte)(param2 >> 8), (byte)param2
            };

            try
            {
                //pipeServerRead.Write(bytes, 0, 9);
                unsafe
                {
                    fixed (byte* numPtr = bytes)
                    {
                        WinApi.WriteFile(pipeServerReadHandle, numPtr, 9, out int numBytesWritten, IntPtr.Zero);
                    }
                }

                sequentialErrorCounter = 0;
            }
            catch (Exception)
            {
                CountError();
                ;
            }
        }

        private void CountError()
        {
            sequentialErrorCounter++;
            if (sequentialErrorCounter == numSequentialErrorsBeforeAssumeDead)
            {
                sequentialErrorCounter = 0;//Avoid stack overflow
                Close();
            }
        }

        public void Close()
        {
            Logger.WriteLine($"Closing read pipe {pipeNameRead} and write pipe {pipeNameWrite}");
            WriteMessageNow(0x03, 0, 0);//Close pipe message.

            pipeServerRead?.Dispose();
            pipeServerRead = null;

            receiveThread?.Abort();

            pipeServerWrite?.Dispose();
            pipeServerWrite = null;

            clientConnected = false;
            xyResetEvent.Dispose();

            onClosed();
        }

        //https://github.com/EasyHook/EasyHook/blob/master/EasyHook/RemoteHook.cs
        /// <summary>
        /// Generates a random 30 character long string of numbers and upper/lower case letters.
        /// </summary>
        /// <returns></returns>
        private string GenerateName()
        {
            byte[] data = new byte[30];
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            random.GetBytes(data);

            StringBuilder sb = new StringBuilder();

            sb.Append("NUCLEUS");

            for (int i = 7; i < 30; i++)
            {
                byte b = (byte)(data[i] % 62);

                if (b <= 9)
                {
                    sb.Append((char)('0' + b));
                }
                else if (b <= 35)
                {
                    sb.Append((char)('A' + (b - 10)));
                }
                else
                {
                    sb.Append((char)('a' + (b - 36)));
                }
            }

            return sb.ToString();
        }
    }
}
