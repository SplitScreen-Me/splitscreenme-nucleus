#include "pch.h"
#include "Logging.h"
#include "FakeMouse.h"
#include "KeyStates.h"

namespace Piping
{
	std::wstring writePipeName;
	std::wstring readPipeName;
	std::wstring sharedMemName;

	HANDLE hPipeRead;
	HANDLE hPipeWrite;
	bool pipeClosed = false;

	HANDLE hMem;
	int* memBuf;
	const SIZE_T MEM_SIZE = 4 * 4;

	const int SEQUENTIAL_ERRORS_BEFORE_TERMINATE = 10;

	void startSharedMem()
	{
		//Open memory
		hMem = OpenFileMappingW(
			FILE_MAP_READ | FILE_MAP_WRITE,
			FALSE,
			sharedMemName.c_str());

		if (hMem == NULL)
		{
			DEBUGLOG("Failed to open shared memory. Error=" << GetLastError() << "\n");
			return;
		}

		//Get pointer
		memBuf = static_cast<int*>(
			MapViewOfFile(
				hMem,
		         FILE_MAP_READ | FILE_MAP_WRITE,
		         0,
		         0,
		         MEM_SIZE));

		if (memBuf == NULL)
		{
			DEBUGLOG("Failed to open shared memory buffer. Error=" << GetLastError() << "\n");
			CloseHandle(hMem);
			return;
		}

		DEBUGLOG("Successfully connected to shared memory\n");
	}
	
	void startPipeListen()
	{		
		//Read pipe
		char pipeNameChars[256];
		sprintf_s(pipeNameChars, R"(\\.\pipe\%s)", std::string(readPipeName.begin(), readPipeName.end()).c_str());

		hPipeRead = CreateFile(
			pipeNameChars,
			GENERIC_READ,
			FILE_SHARE_READ | FILE_SHARE_WRITE,
			nullptr,
			OPEN_EXISTING,
			FILE_ATTRIBUTE_NORMAL,
			nullptr
		);

		if (hPipeRead == INVALID_HANDLE_VALUE)
		{
			DEBUGLOG("Failed to connect to pipe (read)\n")
				return;
		}

		DEBUGLOG("Connected to pipe (read)\n")

		//Write pipe
		char pipeNameCharsWrite[256];
		sprintf_s(pipeNameCharsWrite, R"(\\.\pipe\%s)", std::string(writePipeName.begin(), writePipeName.end()).c_str());

		hPipeWrite = CreateFile(
			pipeNameCharsWrite,
			GENERIC_WRITE,
			FILE_SHARE_READ | FILE_SHARE_WRITE,
			nullptr,
			OPEN_EXISTING,
			FILE_ATTRIBUTE_NORMAL,
			nullptr
		);

		if (hPipeWrite == INVALID_HANDLE_VALUE)
		{
			DEBUGLOG("Failed to connect to pipe (write)\n")
		}
		else
		{
			DEBUGLOG("Connected to pipe (write)\n")
		}

		int sequentialErrors = 0;
		
		//Loop until pipe close message is received
		for (;;)
		{
			BYTE buffer[9]; //9 bytes are sent at a time (1st is message, next 8 for 2 ints)
			DWORD bytesRead = 0;

			const auto result = ReadFile(
				hPipeRead,
				buffer,
				9 * sizeof(BYTE),
				&bytesRead,
				nullptr
			);

			if (!result || bytesRead != 9)
			{
				//Failed to read
				
				sequentialErrors++;
				DEBUGLOG("Failed to read pipe (" << sequentialErrors << ")\n");
				if (sequentialErrors == SEQUENTIAL_ERRORS_BEFORE_TERMINATE)
				{
					DEBUGLOG("ENDING PIPE LISTEN");
					pipeClosed = true;
					return;
				}

				//Give it time to recover if necessary
				Sleep(100);
				continue;
			}

			//Read successful
			sequentialErrors = 0;
			
			const auto param1 = bytesToInt(&buffer[1]);
			const auto param2 = bytesToInt(&buffer[5]);

			switch (buffer[0])
			{
			case 0x01: //Add delta cursor pos
				{
					DEBUGLOG("Received set delta cursor pos message. This should not happen.")
					//EnterCriticalSection(&FakeMouse::fakeMouseCriticalSection);
					//FakeMouse::fakeX += param1;
					//FakeMouse::fakeY += param2;
					//LeaveCriticalSection(&FakeMouse::fakeMouseCriticalSection);
					break;
				}
			case 0x04: //Set absolute cursor pos
				{
					DEBUGLOG("Received set abs cursor pos message. This should not happen.")
					//EnterCriticalSection(&FakeMouse::fakeMouseCriticalSection);
					//FakeMouse::absoluteX = param1;
					//FakeMouse::absoluteY = param2;
					//LeaveCriticalSection(&FakeMouse::fakeMouseCriticalSection);
					break;
				}
			case 0x02: //Set VKey
				{
					KeyStates::setVkeyState(param1, param2 != 0);
					break;
				}
			case 0x03: //Close named pipe
				{
					DEBUGLOG("Received pipe closed message. Closing pipe..." << "\n")
					pipeClosed = true;
					return;
				}
			case 0x05: //Focus desktop
				{
					//If the game brings itself to the foreground, it is the only window that can set something else as foreground (so it's required to do this in Hooks)
					SetForegroundWindow(GetDesktopWindow());
					break;
				}
			default:
				{
					break;
				}
			}
		}
	}
}