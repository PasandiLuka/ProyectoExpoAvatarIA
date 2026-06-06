using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvatarExpo.Services;

public class LandmarkData
{
    public Dictionary<string, List<double>>? P { get; set; }
    public FaceData? F { get; set; }
    public VisibilityData? V { get; set; }
    public SkeletonData? Skeleton { get; set; }
}

public class FaceData
{
    [JsonPropertyName("exp")]
    public string Exp { get; set; } = "neutral";

    [JsonPropertyName("brow")]
    public string Brow { get; set; } = "neutral";
}

public class VisibilityData
{
    [JsonPropertyName("lh")]
    public double LH { get; set; } = 1.0;

    [JsonPropertyName("rh")]
    public double RH { get; set; } = 1.0;
}

public class SkeletonData
{
    public List<List<double>>? Landmarks { get; set; }
    public List<List<int>>? Connections { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class ProcessedLandmarks
{
    public double LSx { get; set; }
    public double LSy { get; set; }
    public double LEx { get; set; }
    public double LEy { get; set; }
    public double LWx { get; set; }
    public double LWy { get; set; }
    public double RSx { get; set; }
    public double RSy { get; set; }
    public double REx { get; set; }
    public double REy { get; set; }
    public double RWx { get; set; }
    public double RWy { get; set; }
    public double RSz { get; set; }
    public double LSz { get; set; }
    public double NoseX { get; set; }
    public double NoseY { get; set; }
    public string Expression { get; set; } = "neutral";
    public double LeftHandVis { get; set; } = 1.0;
    public double RightHandVis { get; set; } = 1.0;
}

public static class LandmarkParser
{
    public static ProcessedLandmarks Parse(LandmarkData data)
    {
        var result = new ProcessedLandmarks();

        if (data.P != null)
        {
            if (data.P.TryGetValue("ls", out var ls) && ls.Count >= 2)
            {
                result.LSx = ls[0]; result.LSy = ls[1];
                result.LSz = ls.Count >= 3 ? ls[2] : 0;
            }
            if (data.P.TryGetValue("le", out var le) && le.Count >= 2)
            { result.LEx = le[0]; result.LEy = le[1]; }
            if (data.P.TryGetValue("lw", out var lw) && lw.Count >= 2)
            { result.LWx = lw[0]; result.LWy = lw[1]; }
            if (data.P.TryGetValue("rs", out var rs) && rs.Count >= 2)
            {
                result.RSx = rs[0]; result.RSy = rs[1];
                result.RSz = rs.Count >= 3 ? rs[2] : 0;
            }
            if (data.P.TryGetValue("re", out var re) && re.Count >= 2)
            { result.REx = re[0]; result.REy = re[1]; }
            if (data.P.TryGetValue("rw", out var rw) && rw.Count >= 2)
            { result.RWx = rw[0]; result.RWy = rw[1]; }
        }

        if (data.F != null)
            result.Expression = data.F.Exp;

        if (data.V != null)
        {
            result.LeftHandVis = data.V.LH;
            result.RightHandVis = data.V.RH;
        }

        if (data.Skeleton?.Landmarks != null && data.Skeleton.Landmarks.Count > 0)
        {
            var nose = data.Skeleton.Landmarks[0];
            if (nose.Count >= 2)
            {
                result.NoseX = nose[0];
                result.NoseY = nose[1];
            }
        }

        return result;
    }
}
