namespace Nucleus.Gaming
{
    public interface IRadioControl
    {
        void RadioSelected();
        void RadioUnselected();

        // mouse enter basically
        void UserOver();
        // mouse leave
        void UserLeave();

        //void Highlight();
        //void SoftHighlight();
        //void Darken();
    }
}
