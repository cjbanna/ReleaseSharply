namespace ReleaseSharply.Server
{
    public class PublishRequest
    {
        public string FeatureGroup { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }
}
