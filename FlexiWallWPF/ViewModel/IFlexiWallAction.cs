using System.Windows;

namespace FlexiWallWPF.ViewModel
{
    public interface IFlexiWallAction
    {
        void StartPush(Point startPoint);
        void StartPull(Point startPoint);

        void ContinuePush(Point currPos);

        void ContinuePull(Point currPos);

        void StopPush(Point endPos);

        void StopPull(Point endPos);

        void Move(Vector vector);

        void Scale(int factor);
    }
}
