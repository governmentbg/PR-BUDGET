using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;

using System;
using System.IO;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
using Microsoft.VisualBasic.FileIO;
using CielaDocs.Shared.ExpressionEngine;
using Microsoft.AspNetCore.Http;
using CielaDocs.SjcWeb.Extensions;

namespace CielaDocs.SjcWeb.Models
{
    public class Toolbox
    {
        public static string[] MonthStr = { " ", "Януари", "Февруари", "Март", "Април", "Май", "Юни", "Юли", "Август", "Септември", "Октомври", "Ноември", "Декември" };
        private static Random random = new Random();
        public const string EmailStandard = @"^[a-zA-Z0-9._-]+@([a-zA-Z0-9.-]+\.)+[a-zA-Z0-9.-]{2,4}$";
        public static bool ValidateEmail(string email)
        {
            if (email != null)
                return System.Text.RegularExpressions.Regex.IsMatch(email, EmailStandard);
            else
                return false;
        }
        public static string GetBGDateTime(DateTime? aDateTime, int aReturnType)
        {
            if(aDateTime==null) return string.Empty;
            if (aReturnType == 0) { return String.Format("{0:dd.MM.yyyy HH:mm:ss}", aDateTime); }
            else
                if (aReturnType == 1) { return String.Format("{0:dd.MM.yyyy}", aDateTime); }
            else
                    if (aReturnType == 2) { return String.Format("{0:dd.MM.yyyy HH:mm}", aDateTime); }
            else return "";
        }
        public static string GetSqlDateTime(DateTime aDateTime, int aReturnType)
        {
            if (aReturnType == 0) { return String.Format("{0:yyyy-MM-dd HH:mm:ss}", aDateTime); }
            else
                if (aReturnType == 1) { return String.Format("{0:yyyy-MM-dd}", aDateTime); }
            else
                    if (aReturnType == 2) { return String.Format("{0:yyyy-MM-dd HH:mm}", aDateTime); }
            else return "";
        }
        public static string GenerateRandomPin()
        {

            string s = "";
            for (int i = 0; i < 6; i++)
                s = String.Concat(s, random.Next(10).ToString());
            return s;

        }
        public static bool IsValidEmail(string email)
        {
            var r = new Regex(@"^([0-9a-zA-Z]([-\.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$");

            return !string.IsNullOrEmpty(email) && r.IsMatch(email);
        }
        public static string SerializeObject<T>(T objectToSerialize)
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream memStr = new MemoryStream();

            try
            {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                bf.Serialize(memStr, objectToSerialize);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                memStr.Position = 0;

                return Convert.ToBase64String(memStr.ToArray());
            }
            finally
            {
                memStr.Close();
            }
        }

        public static T DerializeObject<T>(string objectToDerialize)
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            byte[] byteArray = Convert.FromBase64String(objectToDerialize);
            MemoryStream memStr = new MemoryStream(byteArray);

            try
            {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                return (T)bf.Deserialize(memStr);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            }
            finally
            {
                memStr.Close();
            }
        }

        public static bool IsDelimitedStringOfInt(string sParam)
        {
            while (sParam.EndsWith(","))
                sParam = sParam.Substring(0, sParam.Length - 1);
            Regex regex = new Regex(@"^[0-9]+(,[0-9]*)*$");
            Match match = regex.Match(sParam);
            if (match.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string CleanCyrilicFileName(string fileName)
        {
            Regex regex = new Regex(@"[\s,:();?!}{=#%&*/\\а-яА-Я]+");
            string cleanText = regex.Replace(fileName, "1");
            return cleanText;

        }
        public static bool IsDate(string str)
        {
            try
            {
                Convert.ToDateTime(str);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool IsValidBulstat(string sbul)
        {
            sbul = sbul.Trim();
            string se = "";
            int ei = 0;
            int s = 0;
            int m = 0;
            if (sbul.Trim().Length < 9) { return false; };
            if (!IsNumeric((object)sbul)) { /*"Bulstast трябва да се състои само от цифри.";*/ return false; }

            if ((sbul.Length == 9) || (sbul.Length == 13))
            {
                s = 0;
                for (int i = 0; i < 8; i++)
                {

                    ei = int.Parse(sbul.Substring(i, 1));
                    s += ei * (i + 1);
                }
                m = (s % 11);
                if (m == 10)
                {
                    s = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        ei = int.Parse(sbul.Substring(i, 1));
                        s += ei * (i + 3);
                    }
                    m = (s % 11);
                    if (m == 10) m = 0;
                }
                int nlast = int.Parse(sbul.Substring(8, 1));
                if (m != nlast) { return false; } else { return true; };
            }
            if (sbul.Length == 13)
            {
                int a9, a10, a11, a12;
                a9 = Convert.ToInt32(sbul.Substring(8, 1));
                a10 = Convert.ToInt32(sbul.Substring(9, 1));
                a11 = Convert.ToInt32(sbul.Substring(10, 1));
                a12 = Convert.ToInt32(sbul.Substring(11, 1));
                s = (2 * a9) + (7 * a10) + (3 * a11) + (5 * a12);
                m = Convert.ToByte(s % 11);
                if (m != 10)
                {
                    se = sbul.Substring(12, 1);
                    ei = Convert.ToInt32(se);
                    if (m == ei) { return true; };

                }
                if (m == 10)
                {
                    s = (4 * a9) + (9 * a10) + (5 * a11) + (7 * a12);
                    m = Convert.ToByte(s % 11);
                    if (m == 10) m = 0;
                    se = sbul.Substring(12, 1);
                    ei = Convert.ToInt32(se);
                    if (m == ei) { return true; } else { return false; };
                }
            }
            return false;
        }
        private static bool IsNumeric(object Expression)
        {
            // Променлива съхраняваща стойността от Return на методът TryParse.
            bool isNum;
            // Define variable to collect out parameter of the TryParse method. If the conversion fails, the out parameter is zero.
            double retNum;
            // The TryParse method converts a string in a specified style and culture-specific format to its double-precision floating point number equivalent.
            // The TryParse method does not generate an exception if the conversion fails. If the conversion passes, True is returned. If it does not, False is returned.
            isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
        public static bool IsValidEgn(string str)
        {
            if (str.Length != 10) { /*"егн-то трябва да е 10 символа.";*/ return false; }
            if (!IsNumeric((object)str)) { /*"егн-то трябва да се състои само от цифри.";*/ return false; }
            //'promenlivi
            int g, m, d;
            byte[] months = new byte[] { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            //'opravq godinata i meseca
            g = Convert.ToInt32(str.Substring(0, 2));
            m = Convert.ToInt32(str.Substring(2, 2));
            d = Convert.ToInt32(str.Substring(4, 2));
            if (m >= 41 && m <= 52) { m -= 40; g += 2000; }
            else if (m >= 21 && m <= 32) { m -= 20; g += 1800; }
            else if (m >= 1 && m <= 12) { g += 1900; }
            if (m < 1 || m > 12) { /*"егн-то е с грешен месец.";*/ return false; }
            if (DateTime.IsLeapYear(g)) { months[2] = 29; }
            if (d < 1 || d > months[m]) { /*"егн-то е с грешен ден.";*/ return false; }
            byte i, ost;
            long sum = 0;
            byte[] tegla = new byte[] { 0, 2, 4, 8, 5, 10, 9, 7, 3, 6 };
            for (i = 1; i <= 9; i++) { sum += tegla[i] * Convert.ToSByte(str.Substring((i - 1), 1)); }
            ost = Convert.ToByte(sum % 11);
            if (ost == 10) ost = 0;
            if (ost != Convert.ToByte(str.Substring(9, 1))) { /*"Грешна контролна цифра.";*/ return false; }
            return true;
        }
        public static DataSet ExcelToDataTable(string filePath)
        {
            DataSet dataSet = new DataSet();
            using (XLWorkbook workBook = new XLWorkbook(filePath))
            {
                foreach (IXLWorksheet workSheet in workBook.Worksheets)
                {
                    DataTable dt = new DataTable(workSheet.Name);

                    // Read First Row of Excel Sheet to add Columns to DataTable
                    workSheet.FirstRowUsed().CellsUsed().ToList()
                    .ForEach(x => { dt.Columns.Add(x.Value.ToString()); });

                    foreach (IXLRow row in workSheet.RowsUsed().Skip(1))
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            dr[i] = row.Cell(i + 1).Value.ToString();
                        }
                        dt.Rows.Add(dr);
                    }
                    dataSet.Tables.Add(dt);
                }
            }
            return dataSet;
        }
        public static DataTable CsvToDataTable(string filePath)
        {
            var dtRes = new DataTable();
            using (var parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.TrimWhiteSpace = true;
                parser.SetDelimiters(",");
                var columns = parser.ReadFields();
                foreach (var col in columns)
                    dtRes.Columns.Add(col);
                while (!parser.EndOfData)
                {
                    var row = parser.ReadFields();
                    dtRes.LoadDataRow(row, true);
                }
            }
            return dtRes;
        }
        public static List<string> ExtractCalcArgs(string? s) {
            if (s == null) return new List<string>();
            var t = new Tokenizer(new StringReader(s));
            List<string> vars = new ();
            while (t.Token != Token.EOF)
            {
                if (vars.IndexOf(t.Identifier) < 0)
                { vars.Add(t.Identifier); }

                t.NextToken();
            }
            return vars;
        }
      
        public static string ReplaceCalculationFormula(string Source, Dictionary<string, string> dic)
        {
            string[] el = Source.Split(new char[] { '/', '*', '+', '-', ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            // string result=string.Empty;
            foreach (var (key, value) in dic)
            {
                //int Place = Source.IndexOf(key);
                int Place = Source.IndexOfWholeWord(key);
                Source = Source.Remove(Place, key.Length).Insert(Place, !string.IsNullOrWhiteSpace(value) ? value : "0");
            }

            return Source;
        }

    }
}
