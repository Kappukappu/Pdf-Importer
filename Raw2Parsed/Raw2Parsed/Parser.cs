using System;
using System.Collections.Generic;
using System.Net;

using Newtonsoft.Json.Linq;

namespace Raw2Parsed
{
    public class Entry
    {
        public string text;
        public float x;
        public float y;
        public float width;
        public float size;
        public Entry(string content, float xAxis, float yAxis, float w, float s)
        {
            text = content;
            x = xAxis;
            y = yAxis;
            width = w;
            size = s;
        }
        public override string ToString()
        {
            return "Text: " + text + "\nx: " + x + "\ny: " + y + "\nsize: " + size;
        }
    }

    public interface Parser
    {
        Boolean InSameRow(Entry a, Entry b);

        Boolean InSameColumn(Entry a, Entry b);

        List<Entry> CombineMultiLines(List<Entry> list);

        int IndexOfFirstColumnLeader(List<Entry> text);

        int IndexOfFirstRowLeader(List<Entry> text);

        List<Entry> GetColumnTitleList(List<Entry> text);

        int GetColumnIndex(Entry entry, List<Entry> ColumnTitleList);

        string Parse(string RawData);
    }
}
