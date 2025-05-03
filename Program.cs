using System;
using System.Collections.Generic;
using System.Xml;
using System.Windows.Forms;

namespace LssToYouTubeChapters
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Path to *.lss");
            Console.WriteLine("Drag-and-drop *lss. file and press Enter");
            string lssPath = Console.ReadLine();

            XmlDocument lssDocument = new XmlDocument();
            lssDocument.Load(lssPath.Trim('"'));

            XmlNodeList lssNodes = lssDocument.SelectNodes("/Run/Segments/Segment/Name | /Run/Segments/Segment/SplitTimes/SplitTime/RealTime");

            List<string> names = new List<string>();
            List<string> splitsName = new List<string>();
            List<double> splitsRealTime = new List<double>();

            foreach (XmlNode node in lssNodes)
            {
                if (node.Name == "Name")
                {
                    names.Add(node.InnerText);
                }
                else
                {
                    splitsName.Add(string.Join(" + ", names));
                    names.Clear();
                    splitsRealTime.Add(TimeSpan.Parse(node.InnerText).TotalMilliseconds);
                }
            }

            Console.WriteLine("Offset (in ms):");
            string offsetString = Console.ReadLine();

            double offset = double.Parse(offsetString);

            for (int i = 0; i < splitsRealTime.Count; i++)
            {
                splitsRealTime[i] += offset;
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
            Console.WriteLine("The text has been successfully copied to the clipboard!");

            Console.ReadLine();
        }
    }
}