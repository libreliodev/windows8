using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace StrConvert
{
    class Program
    {
        static string TEMPLPATE = "template.resw";
  //<data name="string1" xml:space="preserve">
  //  <value>Hello World</value>
  //</data>
  //<data name="WelcomeHeader.Text" xml:space="preserve">
  //  <value>Hello World</value>
  //</data>
  //<data name="WelcomeHeader.Width" xml:space="preserve">
  //  <value>115</value>
  //</data>
  //<data name="appDescription" xml:space="preserve">
  //  <value>Application Resources C# sample</value>
  //</data>
  //<data name="appDisplayName" xml:space="preserve">
  //  <value>Application Resources C# sample</value>
  //</data>
  //<data name="displayName" xml:space="preserve">
  //  <value>Application Resources C# sample</value>
  //</data>
  //<data name="shortName" xml:space="preserve">
  //  <value>Resources C#</value>
  //  <comment>Must be less than 13 characters and should be an abbreviation for the displayName</comment>
  //</data>
  //<data name="webservicelang" xml:space="preserve">
  //  <value>en</value>
  //</data>

        static void processLang(string inp, string lang) {
            string sfxInp = "-" + lang, sfxOut = lang;
            if (lang == "en")
                sfxInp = "";


            string inFile = inp + "values" + sfxInp + "/strings.xml";
            XmlDocument inDoc = new XmlDocument();

            using (XmlTextReader reader = new XmlTextReader(inFile))
            {
                inDoc.Load(reader);
            }

            string templ;
            using (TextReader reader = new StreamReader(TEMPLPATE, System.Text.Encoding.UTF8))
            {
                templ = reader.ReadToEnd();
            }


            XmlNodeList stringNodes = inDoc.SelectNodes("resources/string");
            string res = "";
            foreach (XmlNode n in stringNodes)
            {
                string key = n.Attributes["name"].Value;
                string val = n.InnerText;
                //TODO: Check for the special symbols and so on..
                res += String.Format("  <data name=\"{0}\" xml:space=\"preserve\">\r\n  <value>{1}</value>  \r\n  </data>\r\n", key, val);
            }

            templ = templ.Replace("TODODEBUG", res);


            System.IO.Directory.CreateDirectory("res/strings/" + sfxOut);
            string outFile = "res/strings/" + sfxOut +"/resources.resw";

            using (TextWriter writer = new StreamWriter(outFile))
            {
                writer.Write(templ);
            }

        }

        static void Main(string[] args)
        {
            string inFile = args[0];
            //string inFile = "F:/Work/Freelance/Projects/w6/android-prj/librelio-android/res/";
            processLang(inFile, "en");
            processLang(inFile, "fr");
        }
    }
}
