using System;
using System.Collections.Generic;
using System.Net;

using Newtonsoft.Json.Linq;

namespace Raw2Parsed
{
    public class BreatherParser : Parser
    {
        public float sameLine_Tolerance = 0.41F;

        public Boolean InSameRow(Entry a, Entry b)
        {
            return Math.Abs(a.y - b.y) < sameLine_Tolerance;
        }

        public Boolean InSameColumn(Entry a, Entry b)
        {
            return Math.Abs(a.x - b.x) < Math.Max(a.width, b.width) / 6;
        }

        public List<Entry> CombineMultiLines(List<Entry> list)
        {
            List<Entry> result = new List<Entry>();
            for (int i=0; i<list.Count; i++)
            {
                if ((list[i].text.Contains("Stock")) && !(list[i].text.Contains("Stockholder")))
                {
                    int count = 0;
                    int j = i;
                    Entry CombinedEntry = list[i];
                    while ((count<3) && (j > 0))
                    {
                        j--;
                        if (InSameColumn(list[i], result[j])){
                            CombinedEntry.text = result[j].text + " " + CombinedEntry.text;
                            CombinedEntry.x = Math.Min(list[i].x, result[j].x);
                            count++;
                        }
                    }
                    result.Add(CombinedEntry);
                }
                else if ((list[i].text.Contains("Fully Diluted")) && !(list[i].text.Contains("Total")))
                {
                    Entry CombinedEntry = list[i];
                    CombinedEntry.text = "Percentage " + CombinedEntry.text;
                    result.Add(CombinedEntry);
                }
                else if (list[i].text.Contains("Total"))
                {
                    Entry CombinedEntry = list[i];
                    CombinedEntry.x -= 0.3F;
                    result.Add(CombinedEntry);
                }
                else
                {
                    result.Add(list[i]);
                }
            }
            return result;
        }

        public int IndexOfFirstColumnLeader(List<Entry> text)
        {
            for (int i = 0; i < text.Count; i++)
            {
                if (text[i].text.Contains("Outstanding Common Stock")) return i;
            }
            return -1;
        }

        public int IndexOfFirstRowLeader(List<Entry> text)
        {
            for (int i=0; i<text.Count; i++)
            {
                if (text[i].text.Contains("Preferred Stockholders")) return i;
            }
            return -1;
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
                parsedText[rowLeader.text] = row;
                
            }
            return parsedText.ToString();
        }
    }
}
