using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GetTruyen
{
    public class ChapterCollection : IEnumerable<Chapter>
    {
        private List<Chapter> chapters;
        public Chapter this[int index]
        {
            get => chapters[index];
            set => chapters[index] = value;
        }
        public int Count{ get => chapters.Count; }
        public ChapterCollection()
        {
            chapters = new List<Chapter>();
        }

        public void Add(Chapter chapter)
        {
            chapters.Add(chapter);
        }
        public void Remove(Chapter chapter)
        {
            chapters.Remove(chapter);
        }
        public void RemoveAt(int index)
        {
            chapters.RemoveAt(index);
        }

        public IEnumerator<Chapter> GetEnumerator()
        {
            return chapters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
