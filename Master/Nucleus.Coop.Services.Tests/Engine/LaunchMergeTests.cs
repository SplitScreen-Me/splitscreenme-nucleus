using System.Collections;
using System.Collections.Specialized;
using System.IO;
using Nucleus.Gaming.Tools.NucleusUsers;
using Xunit;
using Gwm = Nucleus.Gaming.Tools.GlobalWindowMethods.GlobalWindowMethods;
using QuitAction = Nucleus.Gaming.Tools.GlobalWindowMethods.GlobalWindowMethods.InstanceQuitAction;
using Sdl = Nucleus.Gaming.Tools.SDL2Dll.SDL2Dll;
using XInputPlus = Nucleus.Gaming.Tools.XInputPlusDll.XInputPlusDll;

namespace Nucleus.Coop.Services.Tests.Engine;

/// <summary>Launch-side parts of the Nucleus 2.5 merge (Test-Update): pure helpers of the engine.</summary>
public class LaunchMergeTests
{
    [Fact]
    public void Player_temp_folder_is_inside_the_player_environment()
    {
        Assert.Equal(@"C:\Users\Bob\NucleusCoop\Player1", NucleusEnvironmentPaths.PlayerProfile(@"C:\Users\Bob", "Player1"));
        Assert.Equal(@"C:\Users\Bob\NucleusCoop\Player1\AppData\Local", NucleusEnvironmentPaths.PlayerLocalAppData(@"C:\Users\Bob", "Player1"));
        Assert.Equal(@"C:\Users\Bob\NucleusCoop\Player1\AppData\Local\Temp", NucleusEnvironmentPaths.PlayerTemp(@"C:\Users\Bob", "Player1"));
    }

    [Fact]
    public void User_temp_folder_is_under_local_app_data()
    {
        Assert.Equal(@"C:\Users\nucleusplayer2\AppData\Local\Temp", NucleusEnvironmentPaths.UserTemp(@"C:\Users\nucleusplayer2\AppData\Local"));
    }

    [Fact]
    public void Temp_and_tmp_are_both_set_in_an_environment_block()
    {
        IDictionary block = new Hashtable { ["TEMP"] = @"C:\Windows\Temp", ["PATH"] = @"C:\Windows" };
        NucleusEnvironmentPaths.SetTemp(block, @"D:\p1\Temp");

        Assert.Equal(@"D:\p1\Temp", block["TEMP"]);
        Assert.Equal(@"D:\p1\Temp", block["TMP"]);
        Assert.Equal(@"C:\Windows", block["PATH"]);

        StringDictionary startInfo = new StringDictionary();
        NucleusEnvironmentPaths.SetTemp(startInfo, @"D:\p2\Temp");

        Assert.Equal(@"D:\p2\Temp", startInfo["TEMP"]);
        Assert.Equal(@"D:\p2\Temp", startInfo["TMP"]);
    }

    [Theory]
    // Game.GameReLaunchByItself: wait for the game's new process and hook it, whatever the exit code.
    [InlineData(false, true, 0, 0, QuitAction.Rehook)]
    [InlineData(true, true, null, 2, QuitAction.Rehook)]
    // Game.CantRestartInstances: closed.
    [InlineData(true, false, -1073741819, 0, QuitAction.CountAsExited)]
    // Default: a crash is restarted, a normal close (or an unknown exit code) ends the instance.
    [InlineData(false, false, -1073741819, 0, QuitAction.Restart)]
    [InlineData(false, false, 1, 2, QuitAction.Restart)]
    [InlineData(false, false, 0, 0, QuitAction.CountAsExited)]
    [InlineData(false, false, null, 0, QuitAction.CountAsExited)]
    // Bounded: no more automatic restarts after MaxAutoRestarts within the window.
    [InlineData(false, false, -1, 3, QuitAction.CountAsExited)]
    [InlineData(false, true, 0, 3, QuitAction.CountAsExited)]
    public void Quit_action_follows_the_handler_options_and_the_exit_code(bool cantRestart, bool relaunchByItself, int? exitCode, int recentRestarts, QuitAction expected)
    {
        Assert.Equal(expected, Gwm.DecideQuitAction(cantRestart, relaunchByItself, exitCode, recentRestarts));
    }

    [Fact]
    public void Restarts_are_bounded_to_three_in_two_minutes()
    {
        Assert.Equal(3, Gwm.MaxAutoRestarts);
        Assert.Equal(120, Gwm.AutoRestartWindow.TotalSeconds);
    }

    [Theory]
    [InlineData(true, "x86", @"utils\SDL2\x86\SDL2.dll")]
    [InlineData(true, "x64", @"utils\SDL2\x64\SDL2.dll")]
    [InlineData(false, "x64", @"utils\SDL3\x64\SDL3.dll")]
    public void Sdl_dll_comes_from_the_utils_folder_of_its_version(bool sdl2, string arch, string relative)
    {
        Assert.Equal(Path.Combine(@"C:\Nucleus", relative), Sdl.SourceDllPath(@"C:\Nucleus", sdl2, arch));
    }

    [Fact]
    public void XInputPlus_proxy_is_under_the_XInputPlus_folder()
    {
        Assert.Equal(@"C:\Nucleus\utils\XInputPlus\proxy\x64\xinput.dll", XInputPlus.ProxyDllPath(@"C:\Nucleus\utils\XInputPlus", "x64"));
        Assert.Equal(@"C:\Nucleus\utils\XInputPlus\old\proxy\x86\xinput.dll", XInputPlus.ProxyDllPath(@"C:\Nucleus\utils\XInputPlus\old", "x86"));
    }
}
