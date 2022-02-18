using System;
using System.Runtime.InteropServices;

namespace AutoHotkey.Interop
{
    /// <summary>
    /// These functions serve as a flat wrapper for AutoHotkey.dll.
    /// They assume AutoHotkey.dll is in the same directory as your
    /// executable.
    /// </summary>
    internal class AutoHotkeyDll
    {
        private const string DLLPATH = "AutoHotkey.dll";

        #region Create Thread

        /// <summary>
        /// Start new thread from ahk file.
        /// </summary>
        /// <param name="Path">This parameter must be a path to existing ahk file.</param>
        /// <param name="Options">Additional parameter passed to AutoHotkey.dll (not available in Version 2 alpha).</param>
        /// <param name="Parameters">Parameters passed to dll.</param>
        /// <returns>	ahkdll returns a thread handle.</returns>
        /// <remarks>ahktextdll is available in AutoHotkey[Mini].dll only, not in AutoHotkey.exe.</remarks>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ahkdll(
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            [MarshalAs(UnmanagedType.LPWStr)] string Options,
            [MarshalAs(UnmanagedType.LPWStr)] string Parameters);

        /// <summary>
        /// ahktextdll is used to launch a script in a separate thread from text/variable.
        /// </summary>
        /// <param name="Code">This parameter must be a string with ahk script.</param>
        /// <param name="Options">Additional parameter passed to AutoHotkey.dll (not available in Version 2 alpha).</param>
        /// <param name="Parameters">Parameters passed to dll.</param>
        /// <returns>ahkdll returns a thread handle.</returns>
        /// <remarks>ahktextdll is available in AutoHotkey[Mini].dll only, not in AutoHotkey.exe.</remarks>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ahktextdll(
            [MarshalAs(UnmanagedType.LPWStr)] string Code,
            [MarshalAs(UnmanagedType.LPWStr)] string Options,
            [MarshalAs(UnmanagedType.LPWStr)] string Parameters);

        #endregion

        #region Determine Thread's State

        /// <summary>
        /// ahkReady is used to check if a dll script is running or not.
        /// </summary>
        /// <returns>1 if a thread is running or 0 otherwise.</returns>
        /// <remarks>Available in AutoHotkey[Mini].dll only, not in AutoHotkey.exe.</remarks>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ahkReady();

        #endregion

        #region Control Thread

        /// <summary>
        /// ahkTerminate is used to stop and exit a running script.
        /// </summary>
        /// <param name="timeout">Time in milliseconds to wait until thread exits.</param>
        /// <returns>Returns always 0.</returns>
        /// <remarks>Available in AutoHotkey[Mini].dll only, not in AutoHotkey.exe.</remarks>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ahkTerminate(uint timeout);

        /// <summary>
        /// ahkReload is used to terminate and start a running script again.
        /// </summary>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ahkReload();

        /// <summary>
        /// ahkPause will pause/un-pause a thread and run traditional AutoHotkey Sleep internally.
        /// </summary>
        /// <param name="strState">Should be "On" or "Off" as a string</param>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ahkPause(
            [MarshalAs(UnmanagedType.LPWStr)] string strState);

        #endregion

        #region Add New Code

        /// <summary>
        /// addFile includes additional script from a file to the running script.
        /// </summary>
        /// <param name="FilePath">Path to a file that will be added to a running script.</param>
        /// <param name="AllowDuplicateInclude">Allow duplicate includes.</param>
        /// <param name="IgnoreLoadFailure">Ignore if loading a file failed.</param>
        /// <returns>addFile returns a pointer to the first line of new created code.</returns>
        /// <remarks>pointerLine can be used in ahkExecuteLine to execute one line only or until a return is encountered.</remarks>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint addFile(
            [MarshalAs(UnmanagedType.LPWStr)] string FilePath,
            byte AllowDuplicateInclude,
            byte IgnoreLoadFailure);

        // Constant values for the execute parameter of addScript
        public struct Execute
        {
            public const byte Add = 0, Run = 1, RunWait = 2;
        }

        /// <summary>
        /// addScript includes additional script from a string to the running script.
        /// </summary>
        /// <param name="code">cript that will be added to a running script.</param>
        /// <param name="execute">Determines whether the added script should be executed.</param>
        /// <returns>addScript returns a pointer to the first line of new created code.</returns>
        /// <remarks>pointerLine can be used in ahkExecuteLine to execute one line only or until a return is encountered.</remarks>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint addScript(
            [MarshalAs(UnmanagedType.LPWStr)] string code,
            byte execute);

        #endregion

        #region Execute Some Code

        /// <summary>
        /// Execute a script from a string that contains ahk script.
        /// </summary>
        /// <param name="code">Script as string/text or variable containing script that will be executed.</param>
        /// <returns>Returns true if script was executed and false if there was an error.</returns>
        /// <remarks>ahkExec will execute the code and delete it before it returns.</remarks>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ahkExec(
            [MarshalAs(UnmanagedType.LPWStr)] string code);

        //TODO: ahkExecuteLine

        /// <summary>
        /// ahkLabel is used to launch a Goto/GoSub routine in script.
        /// </summary>
        /// <param name="labelName">Name of label to execute.</param>
        /// <param name="noWait">Do not to wait until execution finished. </param>
        /// <returns>	1 if label exists 0 otherwise.</returns>
        /// <remarks>Default is 0 = wait for code to finish execution.</remarks>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ahkLabel(
            [MarshalAs(UnmanagedType.LPWStr)] string labelName,
            bool noWait);


        /// <summary>
        /// ahkFunction is used to launch a function in script.
        /// </summary>
        /// <param name="functionName">Name of function to call.</param>
        /// <param name="parameter1">The 1st parameter, or null</param>
        /// <param name="parameter2">The 2nd parameter, or null</param>
        /// <param name="parameter3">The 3rd parameter, or null</param>
        /// <param name="parameter4">The 4th parameter, or null</param>
        /// <param name="parameter5">The 5th parameter, or null</param>
        /// <param name="parameter6">The 6th parameter, or null</param>
        /// <param name="parameter7">The 7th parameter, or null</param>
        /// <param name="parameter8">The 8th parameter, or null</param>
        /// <param name="parameter9">The 9th parameter, or null</param>
        /// <param name="parameter10">The 10th parameter, or null</param>
        /// <returns>	Return value is always a string/text, add 0 to make sure it resolves to digit if necessary.</returns>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ahkFunction(
            [MarshalAs(UnmanagedType.LPWStr)] string functionName,
            [MarshalAs(UnmanagedType.LPWStr)] string parameter1,
            [MarshalAs(UnmanagedType.LPWStr)] string parameter2,
            [MarshalAs(UnmanagedType.LPWStr)] string parameter3,
            [MarshalAs(UnmanagedType.LPWStr)] string parameter4,
            [MarshalAs(UnmanagedType.LPWStr)] string parameter5,
            [MarshalAs(UnmanagedType.LPWStr)] string parameter6,
            [MarshalAs(UnmanagedType.LPWStr)] string parameter7,
            [MarshalAs(UnmanagedType.LPWStr)] string parameter8,
            [MarshalAs(UnmanagedType.LPWStr)] string parameter9,
            [MarshalAs(UnmanagedType.LPWStr)] string parameter10);


        /// <summary>
        /// ahkFunction is used to launch a function in script.
        /// </summary>
        /// <param name="functionName">Name of function to call.</param>
        /// <param name="Parameters">Parameters to pass to function.</param>
        /// <returns>0 if function exists else -1.</returns>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ahkPostFunction(
            [MarshalAs(UnmanagedType.LPWStr)] string functionName,
            [MarshalAs(UnmanagedType.LPWStr)] string Parameters);

        #endregion

        #region Working With Variables

        /// <summary>
        /// ahkassign is used to assign a string to a variable in script.
        /// </summary>
        /// <param name="VariableName">Name of a variable.</param>
        /// <param name="NewValue">Value to assign to variable.</param>
        /// <returns>Returns value is 0 on success and -1 on failure.</returns>
        /// <remarks>ahkassign will create the variable if it does not exist.</remarks>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ahkassign(
            [MarshalAs(UnmanagedType.LPWStr)] string VariableName,
            [MarshalAs(UnmanagedType.LPWStr)] string NewValue);

        /// <summary>
        /// ahkgetvar is used to get a value from a variable in script. 
        /// </summary>
        /// <param name="VariableName">Name of variable to get value from.</param>
        /// <param name="GetPointer">Get value or pointer.</param>
        /// <returns>Returned value is always a string, add 0 to convert to integer if necessary, especially when using getPointer.</returns>
        /// <remarks>ahkgetvar returns empty string if variable does not exist or is empty.</remarks>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ahkgetvar(
            [MarshalAs(UnmanagedType.LPWStr)] string VariableName,
            [MarshalAs(UnmanagedType.I4)] int GetPointer);

        #endregion

        #region Advanced

        /// <summary>
        /// ahkFundFunc is used to get function its pointer
        /// </summary>
        /// <param name="FuncName">Name of function to call.</param>
        /// <returns>Function pointer.</returns>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ahkFindFunc(
            [MarshalAs(UnmanagedType.LPWStr)] string FuncName);

        /// <summary>
        /// ahkFindLabel is used to get a pointer to the label.
        /// </summary>
        /// <param name="LabelName">Name of label.</param>
        /// <returns>ahkFindLabel returns a pointer to a line where label points to.</returns>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ahkFindLabel(
            [MarshalAs(UnmanagedType.LPWStr)] string LabelName);

        /// <summary>
        /// Build in function to get a pointer to the structure of a user-defined variable. 
        /// </summary>
        /// <param name="Variable">the name of the variable</param>
        /// <returns>The pointer to the variable.</returns>
        [DllImport(DLLPATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getVar")]
        public static extern IntPtr GetVar(
            [MarshalAs(UnmanagedType.LPWStr)] string Variable);

        #endregion
    }

}
