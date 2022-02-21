namespace Nucleus.Gaming.Controls
{
    public interface ICanProceed
    {
        bool CanProceed { get; }
        bool AutoProceed { get; }
        void AutoProceeded();

        string StepTitle { get; }
        void Restart();
    }
}
