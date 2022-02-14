namespace Nucleus.Gaming.Coop.InputManagement.Enums
{
    public enum HeaderDwType : uint
    {
        /// <summary>
        /// Raw input comes from some device that is not a keyboard or a mouse.
        /// </summary>
        RIM_TYPEHID = 2,

        /// <summary>
        /// Raw input comes from the keyboard.
        /// </summary>
        RIM_TYPEKEYBOARD = 1,

        /// <summary>
        /// Raw input comes from the mouse. 
        /// </summary>
        RIM_TYPEMOUSE = 0
    }
}
