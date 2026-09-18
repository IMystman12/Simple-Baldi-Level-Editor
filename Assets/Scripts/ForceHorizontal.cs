using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class ForceHorizontal : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void Load()
    {
        Task.Run(() => SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate));
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
    void OnApplicationFocus(bool focus) => Screen.orientation = ScreenOrientation.LandscapeLeft;
}
