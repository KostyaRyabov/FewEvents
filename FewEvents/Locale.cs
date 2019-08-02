namespace FewEvents
{

    public class LOCALE
    {
        public int total { get; set; }
        public LLocale[] locales { get; set; }
    }

    public class LLocale
    {
        public int _id { get; set; }
        public string name {get; set;}
    }

}
