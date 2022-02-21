using System;
using System.Runtime.InteropServices;

namespace AutoHotkey.Interop
{
    /// <summary>
    /// This class expects an AutoHotkey.dll to be available on the machine. (UNICODE) version.
    /// </summary>
    public class AutoHotkeyEngine
    {
        public AutoHotkeyEngine()
        {
            Util.EnsureAutoHotkeyLoaded();

            //ensure that a thread is started
            AutoHotkeyDll.ahktextdll("", "", "");
        }

        /// <summary>
        /// Gets the value for a varible or an empty string if the variable does not exist.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>Returns the value of the variable, or an empty string if the variable does not exist.</returns>
        public string GetVar(string variableName)
        {
            IntPtr p = AutoHotkeyDll.ahkgetvar(variableName, 0);
            return Marshal.PtrToStringUni(p);
        }

        /// <summary>
        /// Sets the value of a variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="value">The value to set.</param>
        public void SetVar(string variableName, string value)
        {
            if (value == null)
            {
                value = "";
            }

            AutoHotkeyDll.ahkassign(variableName, value);
        }

        /// <summary>
        /// Evaulates an expression or function and returns the results
        /// </summary>
        /// <param name="code">The code to execute</param>
        /// <returns>Returns the result of an expression</returns>
        public string Eval(string code)
        {
            string codeToRun = "A__EVAL:=" + code;
            AutoHotkeyDll.ahkExec(codeToRun);
            return GetVar("A__EVAL");
        }

        /// <summary>
        /// Loads a file into the running script
        /// </summary>
        /// <param name="filePath">The filepath of the script</param>
        public void Load(string filePath)
        {
            AutoHotkeyDll.addFile(filePath, 1, 1);
        }

        /// <summary>
        /// Executes raw ahk code.
        /// </summary>
        /// <param name="code">The code to execute</param>
        public void ExecRaw(string code)
        {
            AutoHotkeyDll.ahkExec(code);
        }

        /// <summary>
        /// Terminates the running scripts
        /// </summary>
        public void Terminate()
        {
            AutoHotkeyDll.ahkTerminate(1000);
        }

        /// <summary>
        /// Suspends the scripts
        /// </summary>
        public void Suspend()
        {
            ExecRaw("Suspend, On");
        }

        /// <summary>
        /// Unsuspends the scripts
        /// </summary>
        public void UnSuspend()
        {
            ExecRaw("Suspend, Off");
        }

        /// <summary>
        /// Executes an already defined function.
        /// </summary>
        /// <param name="functionName">The name of the function to execute.</param>
        /// <param name="param1">The 1st parameter</param>
        /// <param name="param2">The 2nd parameter</param>
        /// <param name="param3">The 3rd parameter</param>
        /// <param name="param4">The 4th parameter</param>
        /// <param name="param5">The 5th parameter</param>
        /// <param name="param6">The 6th parameter</param>
        /// <param name="param7">The 7th parameter</param>
        /// <param name="param8">The 8th parameter</param>
        /// <param name="param9">The 9th parameter</param>
        /// <param name="param10">The 10 parameter</param>
        public string ExecFunction(string functionName,
            string param1 = null,
            string param2 = null,
            string param3 = null,
            string param4 = null,
            string param5 = null,
            string param6 = null,
            string param7 = null,
            string param8 = null,
            string param9 = null,
            string param10 = null)
        {
            IntPtr ret = AutoHotkeyDll.ahkFunction(functionName, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);

            if (ret == IntPtr.Zero)
            {
                return null;
            }
            else
            {
                return Marshal.PtrToStringUni(ret);
            }
        }


        /// <summary>
        /// Determines if the function exists.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>Returns true if the function exists, otherwise false.</returns>
        public bool FunctionExists(string functionName)
        {
            IntPtr funcptr = AutoHotkeyDll.ahkFindFunc(functionName);
            return funcptr != IntPtr.Zero;
        }

        /// <summary>
        /// Executes a label
        /// </summary>
        /// <param name="labelName">Name of the label.</param>
        public void ExecLabel(string labelName)
        {
            AutoHotkeyDll.ahkLabel(labelName, false);
        }

        /// <summary>
        /// Determines if the label exists.
        /// </summary>
        /// <param name="labelName">Name of the label.</param>
        /// <returns>Returns true if the label exists, otherwise false</returns>
        public bool LabelExists(string labelName)
        {
            IntPtr labelptr = AutoHotkeyDll.ahkFindLabel(labelName);
            return labelptr != IntPtr.Zero;
        }
    }
}
