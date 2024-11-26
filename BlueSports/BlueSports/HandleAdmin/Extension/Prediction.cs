public class Prediction
{
    public string Label { get; set; }
    public float Confidence { get; set; }
    public List<int> Bbox { get; set; }
}

public class DetectionResult
{
    public List<Prediction> Predictions { get; set; }
}
