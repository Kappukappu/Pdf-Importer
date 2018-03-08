using System;
using System.Collections.Generic;
using System.Net;

using Newtonsoft.Json.Linq;

namespace Raw2Parsed
{
    public class ESharesParser : Parser
    {
        public float sameLine_Tolerance = 1.0F;

        public Boolean InSameRow(Entry a, Entry b)
        {
            return Math.Abs(a.y - b.y) < sameLine_Tolerance;
        }

        public Boolean InSameColumn(Entry a, Entry b)
        {
            return Math.Abs(a.x - b.x) < Math.Max(a.width, b.width) / 2;
        }

        public List<Entry> CombineMultiLines(List<Entry> list)
        {
            //combine multi-line column titles
            List<Entry> result = new List<Entry>();
            int i;
            for (i = 0; i < list.Count - 1; i++)
            {
                Entry CombinedEntry = list[i];
                //combine multi-line column titles
                while (InSameRow(list[i], list[i + 1]) && InSameColumn(list[i], list[i + 1]))
                {
                    i++;
                    CombinedEntry.text += list[i].text;
                }
                result.Add(CombinedEntry);
            }
            //handling last element
            if (i == list.Count - 1)
            {
                result.Add(list[i]);
            }
            return result;
        }

        public int IndexOfFirstColumnLeader(List<Entry> text)
        {
            //The first instance of two blocks in the same row is expected to be the header row
            for (int i = 0; i < text.Count - 1; i++)
            {
                if (InSameRow(text[i], text[i + 1]))
                {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOfFirstRowLeader(List<Entry> text)
        {
            return IndexOfFirstColumnLeader(text) + GetColumnTitleList(text).Count;
        }

        public List<Entry> GetColumnTitleList(List<Entry> text)
        {
            List<Entry> columnTitles = new List<Entry>();
            for (int i = IndexOfFirstColumnLeader(text); i < text.Count - 1; i++)
            {
                columnTitles.Add(text[i]);
                //check if it is the end of the row for column titles
                if (!(InSameRow(text[i], text[i + 1])))
                {
                    break;
                }
            }
            return columnTitles;
        }

        public int GetColumnIndex(Entry entry, List<Entry> ColumnTitleList)
        {
            for (int i = 0; i < ColumnTitleList.Count - 1; i++)
            {
                if ((entry.x > ColumnTitleList[i].x) && (entry.x < ColumnTitleList[i + 1].x))
                {
                    return i;
                }
            }
            return ColumnTitleList.Count - 1;
        }

        public string Parse(string RawData)
        {
            //1. Get useful information (text, x&y value, width, size) from raw output
            List<Entry> text = new Functions().ExtractText(JObject.Parse(RawData));
            //2. Combine entries that in the same column and close enough.
            text = CombineMultiLines(text);
            //3. Get Column Titles
            List<Entry> ColumnTitleList = GetColumnTitleList(text);
            //4. Find the first entry after column titles
            int indexOfFirstValue = IndexOfFirstRowLeader(text);

            JObject parsedText = new JObject(); //to receive the final result
            int i = indexOfFirstValue;

            while (i < text.Count)
            {
                //each loop handles one row, wrapped into a JObject
                Entry rowLeader = text[i];
                i++;
                JObject row = new JObject();
                while ((i < text.Count) && (InSameRow(text[i], rowLeader)))
                {
                    int columnIndex = GetColumnIndex(text[i], ColumnTitleList);
                    row[ColumnTitleList[columnIndex].text] = text[i].text;
                    i++;
                }
                //the first entry in a row is the key name of JObject
                parsedText.Add(rowLeader.text, row);
            }
            return parsedText.ToString();
        }
    }
}
