namespace AvatarExpo.Services;

public class AvatarState
{
    public string ShirtStyle { get; set; } = "shirt-short";
    public string HatStyle { get; set; } = "none";
    public string GlassesStyle { get; set; } = "none";
    public string LeftHandItem { get; set; } = "none";
    public string RightHandItem { get; set; } = "none";
    public string HairStyle { get; set; } = "hair-style-1";
    public string HairColor { get; set; } = "#3e2723";
    public string Expression { get; set; } = "neutral";
    public double DemoTorsoY { get; set; }
    public double DemoHeadTilt { get; set; }
    public double DemoLeftShoulder { get; set; }
    public double DemoLeftElbow { get; set; }
    public double DemoRightShoulder { get; set; }
    public double DemoRightElbow { get; set; }

    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();
}
