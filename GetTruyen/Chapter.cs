using System;
using System.Collections.Generic;
using System.Text;

namespace GetTruyen
{
    public class Chapter
    {
        public int Id;
        public string Name;
        public string Url;
        public string Contents;
        public Chapter(int id=0, string name="", string url="", string contents = "")
        {
            Id = id;
            Name = name;
            Url = url;
            Contents = contents;
        }
    }
}
