namespace Liangddyy.UnityKitModule.Common
{
    public class PlatformEditor
    {
        public static bool IsWinPlatform
        {
            get
            {
#if UNITY_EDITOR_WIN
                return true;
#endif
                return false;
            }
        }

        public static bool IsOSXPlatform
        {
            get
            {
#if UNITY_EDITOR_OSX
                return true;
#endif
                return false;
            }
        }
    }
}