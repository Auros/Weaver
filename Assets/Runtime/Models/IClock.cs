namespace Weaver.Models
{
    public interface IClock
    {
        float GetCurrentTime();
        void SetCurrentTime(float value);
    }
}