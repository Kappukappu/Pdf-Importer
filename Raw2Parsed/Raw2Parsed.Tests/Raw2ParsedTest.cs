using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xunit;
using Newtonsoft.Json.Linq;
using Xbehave;
using FluentAssertions;

namespace Raw2Parsed.Tests
{
    public class Raw2ParsedTest
    {
        private Parser Parser;
        private List<Entry> ListOfEntries;
        private List<Entry> ListOfCombinedEntries;

        [Background]
        public void GetListOfEntries()
        {
            string RawOutputString = null;
            JObject RawOutputJSON = null;
            "Given raw output string by calling LocalRunner"
                .x(() => RawOutputString = new LocalRunner().FetchRawOutput());
            "And parse raw output string to raw output JSON"
                .x(() => RawOutputJSON = JObject.Parse(RawOutputString));
            "And select correct parser based on rawoutput"
                .x(() => Parser = new Functions().SelectCustomer(RawOutputString));
            "When I extract entires from raw output JSON"
                .x(() => this.ListOfEntries = new Functions().ExtractText(RawOutputJSON));
            "Then the list of entries should have more than one entry"
                .x(() => Assert.True(this.ListOfEntries.Count > 1));
        }

        [Background]
        public void CombineMultipleEntries()
        {
            List<Entry> ListOfEntries = null;
            "Given a list of entries"
                .x(() => ListOfEntries = this.ListOfEntries);
            "When I combine the set of entires who are in the same column and close enough"
                .x(() => this.ListOfCombinedEntries = this.Parser.CombineMultiLines(ListOfEntries));
            "Then the list of combined entries have more than one entry and less entries than original list"
                .x(() => Assert.True((this.ListOfCombinedEntries.Count > 1) && (this.ListOfCombinedEntries.Count <= ListOfEntries.Count)));
        }

        [Scenario]
        public void IndexOfFirstColumnLeaderTest(List<Entry> ListOfCombinedEntries, int IndexOfFirstColumnLeader)
        {
            "Given the list of combined entires"
                .x(() => ListOfCombinedEntries = this.ListOfCombinedEntries);
            "When I get the index of first column header"
                .x(() => IndexOfFirstColumnLeader = this.Parser.IndexOfFirstColumnLeader(ListOfCombinedEntries));
            "Then the index should be between 1 and the length of the list of entires"
                .x(() => Assert.True((IndexOfFirstColumnLeader > 0) && (IndexOfFirstColumnLeader < ListOfCombinedEntries.Count)));
        }

        [Scenario]
        public void IndexOfFirstRowLeaderTest(List<Entry> ListOfCombinedEntries, int IndexOfFirstRowLeader)
        {
            "Given the list of combined entires"
                .x(() => ListOfCombinedEntries = this.ListOfCombinedEntries);
            "When I get the index of first row header"
                .x(() => IndexOfFirstRowLeader = this.Parser.IndexOfFirstRowLeader(ListOfCombinedEntries));
            "Then the index should be between 1 and the length of the list of entires"
                .x(() => Assert.True((IndexOfFirstRowLeader > 0) && (IndexOfFirstRowLeader < ListOfCombinedEntries.Count)));
        }

        [Scenario]
        public void GetColumnTitleListTest(List<Entry> ListOfCombinedEntries, List<Entry> ListOfColumnTitles)
        {
            "Given the list of combined entires"
                .x(() => ListOfCombinedEntries = this.ListOfCombinedEntries);
            "When I get the titles of each column"
                .x(() => ListOfColumnTitles = this.Parser.GetColumnTitleList(ListOfCombinedEntries));
            "Then the length of list of column titles at least one and no more than half of the length of original list"
                .x(() => Assert.True((ListOfColumnTitles.Count > 0) && (ListOfColumnTitles.Count < ListOfCombinedEntries.Count / 2)));
        }

        [Scenario]
        [Example(0, "entry", 12F, 5F, 10F)]
        [Example(1, "name", 2F, 1.5F, 0.88F)]
        [Example(3, "reallyrandomname", 3F, 1.42F, 2.55555F)]
        public void GetColumnIndexTest(int ExpectedIndex, string text, float yAxis, float Width, float Size, int Index, 
            List<Entry> ListOfCombinedEntries, Entry EntryInExpectedColumn, List<Entry> ListOfColumnTitles)
        {
            "Given the list of combined entries"
                .x(() => ListOfCombinedEntries = this.ListOfCombinedEntries);
            "And the list of column titles"
                .x(() => ListOfColumnTitles = this.Parser.GetColumnTitleList(this.ListOfCombinedEntries));
            $"And the expected index {ExpectedIndex}"
                .x(() => { });
            "And the expected index does not exceed the length of column title"
                .x(() => Assert.True(ExpectedIndex < ListOfColumnTitles.Count));
            $"And the text {text}"
                .x(() => { });
            $"And the yAxis {yAxis}"
                .x(() => { });
            $"And the Width {Width}"
                .x(() => { });
            $"And the Size {Size}"
                .x(() => { });
            "And an entry which should be in the expected column"
                .x(() => EntryInExpectedColumn = new Entry(text, ListOfColumnTitles[ExpectedIndex].x + 0.1F, yAxis, Width, Size));
            "When I get the column index of this entry"
                .x(() => Index = this.Parser.GetColumnIndex(EntryInExpectedColumn, ListOfColumnTitles));
            "Then the column index should be expected index"
                .x(() => Assert.Equal(ExpectedIndex, Index));
        }
    }
}
