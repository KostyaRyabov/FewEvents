using System;

namespace FewEvents
{
    public class ObjList
    {
        public Obj[] tags { get; set; }
        public int total { get; set; }
    }

    public class Obj
    {
        public int _id { get; set; }
        public string name { get; set; }
        public string sysName { get; set; }
        public Boolean Check_Status { get; set; }
    }
}
