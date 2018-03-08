Structure:

1. LocalRunner.cs: invokes AWS lambda function PDF2Raw (nodeJS) which processes a PDF file input (in form of Base64 string) and returns a Raw output string. The raw output string is the input for all Raw2Parsed tests.
2. Raw2ParsedTest.cs: Xunit tests on each method of Parser.cs in Raw2Parsed project.
3. RawOutput: the output from PDF2Raw
4. output: the output from Raw2Parsed

RawOutput and output are references to help debugging.

Usage:

In visual studio (2017), Test -> Run -> All Tests