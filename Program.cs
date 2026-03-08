using System;
using System.Collections.Generic;
using System.Xml;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace youtube_chapters_generator
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Drag-and-Drop *.lss file and press Enter.");
            string lssPath = Console.ReadLine();

            XmlDocument lssDocument = new XmlDocument();
            lssDocument.Load(lssPath.Trim('"'));

            XmlNodeList offsetNode = lssDocument.SelectNodes("/Run/Offset");
            double startTimerOffset = TimeSpan.Parse(offsetNode[0].InnerText).TotalMilliseconds;
            Console.WriteLine("Start Timer Offset (ms): " + startTimerOffset);

            XmlNodeList lssNodes = lssDocument.SelectNodes("/Run/Segments/Segment/Name | /Run/Segments/Segment/SplitTimes/SplitTime/RealTime");

            List<string> names = new List<string>();

            List<string> splitsName = new List<string>();
            List<double> splitsRealTime = new List<double>();

            string pattern = @"^- |^{.*} ";

            foreach (XmlNode node in lssNodes)
            {
                if (node.Name == "Name")
                {
                    names.Add(Regex.Replace(node.InnerText, pattern, ""));
                }
                else
                {
                    splitsName.Add(string.Join(" + ", names));
                    names.Clear();
                    splitsRealTime.Add(TimeSpan.Parse(node.InnerText).TotalMilliseconds);
                }
            }

            Console.WriteLine("Start Video Offset (ms):");
            string offsetString = Console.ReadLine();

            double offset = double.Parse(offsetString);

            for (int i = 0; i < splitsRealTime.Count; i++)
            {
                splitsRealTime[i] += offset - startTimerOffset;
            }

            if (offset == 0)
            {
                splitsRealTime.Insert(0, 0);
                splitsRealTime.RemoveAt(splitsRealTime.Count - 1);
            }
            else if (offset > 0)
            {
                splitsRealTime.Insert(0, 0);
                splitsName.Insert(0, "Prep");
                splitsRealTime.Insert(1, offset);
                splitsRealTime.RemoveAt(splitsRealTime.Count - 1);
            }

            Console.WriteLine("Time format");
            Console.WriteLine("1) h:mm:ss");
            Console.WriteLine("2) mm:ss");

            string timeFormat = Console.ReadLine();
            string timeFormatString = timeFormat == "1" ? "h':'mm':'ss" : "mm':'ss";

            string clipboard = "";

            if (splitsName.Count == splitsRealTime.Count)
            {
                for (int i = 0; i < splitsRealTime.Count; i++)
                {
                    Console.WriteLine($"{TimeSpan.FromMilliseconds(splitsRealTime[i]).ToString(timeFormatString)} {splitsName[i]}");
                    clipboard += $"{TimeSpan.FromMilliseconds(splitsRealTime[i]).ToString(timeFormatString)} {splitsName[i]}\n";

                }
            }
            else
            {
                Console.WriteLine($"Oops different list sizes. splitsName.Count={splitsName.Count} splitsRealTime.Count={splitsRealTime.Count}");
            }

            Clipboard.SetData(DataFormats.Text, (Object)clipboard.Trim('\n'));
            Console.WriteLine("Copied to clipboard!");

            Console.ReadLine();
        }
    }
}