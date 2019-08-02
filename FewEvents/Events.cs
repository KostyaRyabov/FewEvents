using System;
using Newtonsoft.Json;

namespace FewEvents
{
    public class Record
    {
        public Main[] places { get; set; }
        public int total { get; set; }
    }

    public class Main
    {
        public string status { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Category category { get; set; }
        public Tag[] tags { get; set; }

        public int ageRestriction { get; set; }
        public bool isFree { get; set; }
        public object price { get; set; }
        public object maxPrice { get; set; }
        public Place[] places { get; set; }
        public Organization organization { get; set; }
        public Seance[] seances { get; set; }

        public Workingschedule workingSchedule { get; set; }
        public string workingScheduleComment { get; set; }
        public Address address { get; set; }
        public Contacts contacts { get; set; }
    }

    public class Category
    {
        public string name { get; set; }
    }
    public class Workingschedule
    {
        [JsonProperty(PropertyName = "0")]
        public _0 _0 { get; set; }
        [JsonProperty(PropertyName = "1")]
        public _1 _1 { get; set; }
        [JsonProperty(PropertyName = "2")]
        public _2 _2 { get; set; }
        [JsonProperty(PropertyName = "3")]
        public _3 _3 { get; set; }
        [JsonProperty(PropertyName = "4")]
        public _4 _4 { get; set; }
        [JsonProperty(PropertyName = "5")]
        public _5 _5 { get; set; }
        [JsonProperty(PropertyName = "6")]
        public _6 _6 { get; set; }
    }

    public class _0
    {
        public int from { get; set; }
        public int to { get; set; }
    }
    public class _1
    {
        public int from { get; set; }
        public int to { get; set; }
    }
    public class _2
    {
        public int from { get; set; }
        public int to { get; set; }
    }
    public class _3
    {
        public int from { get; set; }
        public int to { get; set; }
    }
    public class _4
    {
        public int from { get; set; }
        public int to { get; set; }
    }
    public class _5
    {
        public int from { get; set; }
        public int to { get; set; }
    }
    public class _6
    {
        public int from { get; set; }
        public int to { get; set; }
    }

    public class Address
    {
        public string street { get; set; }
        public string comment { get; set; }
        public MapPosition mapPosition { get; set; }
    }
    public class Contacts
    {
        public string website { get; set; }
        public string email { get; set; }
        public string vkontakte { get; set; }
        public string facebook { get; set; }
        public string twitter { get; set; }
        public string instagram { get; set; }
        public Phone[] phones { get; set; }
    }
    public class Phone
    {
        public string value { get; set; }
        public string comment { get; set; }
    }
    public class Tag
    {
        public string name { get; set; }
    }



    public class Organization
    {
        public string name { get; set; }
    }
    public class Place
    {
        public Schedule schedule { get; set; }
        public Address address { get; set; }
        public Category category { get; set; }
        public string name { get; set; }
        public SeanceT[] seances { get; set; }
    }
    public class Schedule
    {
        public Workingschedule days { get; set; }
        public DateTime startLocal { get; set; }
        public DateTime endLocal { get; set; }
    }
    public class MapPosition
    {
        public float[] coordinates { get; set; }
    }
    public class Seance
    {
        public long start { get; set; }
        public long end { get; set; }
    }
    public class SeanceT
    {
        public DateTime startLocal { get; set; }
        public DateTime endLocal { get; set; }
    }
}
