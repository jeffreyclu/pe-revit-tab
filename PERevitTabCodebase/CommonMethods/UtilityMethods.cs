using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Autodesk.Revit.DB;

namespace PERevitTab.CommonMethods
{
    class UtilityMethods
    {
        /// <summary>
        /// Stores the error message, write date, and write time in a dictionary. Can be passed directly to WriteCsv() in order to log the error in a csv database
        /// </summary>
        /// <param name="e">The error</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, object> LogError(Exception e)
        {
            (string writeDate, string writeTime) = FormatDateTime();
            Dictionary<string, object> error = new Dictionary<string, object>();
            error.Add("Error", e.Message);
            error.Add("Write Date", writeDate);
            error.Add("Write Time", writeTime);
            return error; // this function logs the error message, write date, and write time to a CSV.
        }
        /// <summary>
        /// Writes a dictionary to a CSV file
        /// </summary>
        /// <param name="data">The data as a dictionary</param>
        /// <param name="fileName">The desired filename (without .csv)</param>
        public static void WriteCsv(Dictionary<string, object> data, string fileName)
        {
            (string writeDate, string writeTime) = FormatDateTime();
            string csvPath = $"\\\\d-peapcny.net\\enterprise\\G_Gen-Admin\\Committees\\Design-Technology_Digital-Practice\\Revit Model Health\\_Database\\{writeDate}_{fileName}.csv";
            StringBuilder output = new StringBuilder();

            if (!File.Exists(csvPath))
            {
                string header = "";
                foreach (string key in data.Keys)
                {
                    header += $"{key}, ";
                }
                output.AppendLine(header.TrimEnd(','));
            }

            string currentLine = "";
            foreach (object value in data.Values)
            {

                if (value is IList<Element>)
                {
                    IList<Element> valueList = value as IList<Element>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is IList<ElementId>)
                {
                    IList<ElementId> valueList = value as IList<ElementId>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is ICollection<Element>)
                {
                    ICollection<Element> valueCollection = value as ICollection<Element>;
                    currentLine += $"{valueCollection.Count}, ";
                }
                else if (value is IList<FailureMessage>)
                {
                    IList<FailureMessage> valueList = value as IList<FailureMessage>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is IList<Family>)
                {
                    IList<Family> valueList = value as IList<Family>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is IList<View>)
                {
                    IList<View> valueList = value as IList<View>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is IList<SpatialElement>)
                {
                    IList<SpatialElement> valueList = value as IList<SpatialElement>;
                    currentLine += $"{valueList.Count}, ";
                }
                else if (value is IList<Workset>)
                {
                    IList<Workset> valueList = value as IList<Workset>;
                    currentLine += $"{valueList.Count}, ";
                }
                else
                {
                    currentLine += $"{value}, ";
                }
            }
            output.AppendLine(currentLine.TrimEnd(','));
            File.AppendAllText(csvPath, output.ToString());
        }
        /// <summary>
        /// Gets and formats the current date to "yyyy-MM-dd" and the current time to "HH:mm:ss"
        /// </summary>
        /// <returns>Tuple</returns>
        public static (string date, string time) FormatDateTime()
        {
            DateTime localDate = DateTime.Now;
            string writeDate = localDate.ToString("yyyy-MM-dd");
            string writeTime = localDate.ToString("HH:mm:ss");
            return (writeDate, writeTime);
        }
    }
}
