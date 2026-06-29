public static class DepthPhaseSignal
{
    public static bool IsActive { get; private set; }

    public static void Enter() => IsActive = true;
    public static void Exit() => IsActive = false;
}