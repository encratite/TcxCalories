using System;
using System.Xml;

namespace TcxCalories
{
    class Program
    {
        static void Main(string[] arguments)
        {
            if (arguments.Length != 1)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("<path to .tcx file to analyze>");
                return;
            }
            string tcxPath = arguments[0];
            AnalyzeTcxFile(tcxPath);
        }

        private static void AnalyzeTcxFile(string tcxPath)
        {
            var document = new XmlDocument();
            document.Load(tcxPath);
            var namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace("ActivityExtension", "http://www.garmin.com/xmlschemas/ActivityExtension/v2");
            double externalEnergy = 0.0;
            double metabolicEnergy = 0.0;
            var powerNodes = document.SelectNodes("//ActivityExtension:Watts", namespaceManager);
            foreach (XmlNode node in powerNodes)
            {
                int externalPower = int.Parse(node.InnerText);
                double metabolicPower = GetMetabolicPower(externalPower);
                // One second.
                const double interval = 1.0;
                externalEnergy += externalPower * interval;
                metabolicEnergy += metabolicPower * interval;
            }
            double metabolicEnergyKcal = metabolicEnergy / 1000.0 / 4.186;
            Console.WriteLine($"{metabolicEnergyKcal:F0} kcal");
        }

        private static double GetMetabolicPower(int externalPower)
        {
            const double metabolicSlope = 481.0 / 773.0;
            const double metabolicIntercept = 68.0;
            const double externalScale = 500.0 / 890.0;
            const double metabolicScale = 2500 / 712.0;

            double offset = externalPower / externalScale;
            double metabolicOffset = metabolicSlope * offset + metabolicIntercept;
            double metabolicPower = metabolicScale * metabolicOffset;
            return metabolicPower;
        }
    }
}
