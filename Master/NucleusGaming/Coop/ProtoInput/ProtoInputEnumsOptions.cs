namespace Nucleus.Gaming
{
    public enum SetWindowPosHook
    {
        True = 1,
        DontResize,
        DontReposition
    }

    public enum MoveWindowHook
    {
        True = 1,
        DontResize,
        DontReposition
    }

    public enum DrawFakeCursor
    {
        True = 1,
        Fix
    }

    public enum PutMouseInsideWindow
    {
        True = 1,
        IgnoreTopLeft,
        IgnoreBottomRight
    }

    public enum SetRemoveBorderHook
    {
        True = 1,
        DontWait
    }
}
