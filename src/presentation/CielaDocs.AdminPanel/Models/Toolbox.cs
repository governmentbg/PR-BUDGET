using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CielaDocs.AdminPanel.Models
{
    public class Toolbox
    {
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

        public static string Get6CharacterRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", "");
            path = path.Replace("/", "");
            path = path.Replace("\\", "");
            path = path.Replace(",", "");
            return path.Substring(0, 6);
        }
    }
}
