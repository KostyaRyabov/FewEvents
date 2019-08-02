namespace FewEvents
{

    public class GEO
    {
        public Location location { get; set; }
    }

    public class Location
    {
        public LData data { get; set; }
    }

    public class LData
    {
        public string city { get; set; }
    }
}
