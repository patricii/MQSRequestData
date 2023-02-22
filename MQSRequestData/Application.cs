﻿using GlobalOperations.Definitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MQSRequestData
{
    public partial class ApkMQS : MetroFramework.Forms.MetroForm
    {
        public ApkMQS()
        {
            InitializeComponent();
        }
        string url = "mqs.motorola.com";
        string urlYield = "mqs.motorola.com/Collab_GridCpt/default.aspx?enc=INbDb3OXdMp3vde2LbmjfOqENqspcgXNx/9sSFuBt4l9YJObzeJfOcCHKc3GbKGAGwWF5fcyX0zSJaKBMrGv7/9C3vQtCHLGErFQnT+6UylYGmdsJPlvfKLrkaYE5qCz";
        string home = "MQS Home";
        string yield = "MQS - Yield Report";
        string user = "jagapatr"; //debug
        string password = "L@ura2022.7"; //debug
        string erroMsg = string.Empty;
        string strAction = string.Empty;

        private void webpage_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            ((Dictionary<string, object>)((WebBrowser)sender).Tag)["NavigationError"] = string.Empty;
            ((Dictionary<string, object>)((WebBrowser)sender).Tag)["Navigated"] = false;

        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
           // erroMsg = GetYieldThreadSafeWithLogin(user, password, url, home);
            erroMsg = GetYieldThreadSafe(url, home);
            if (erroMsg == string.Empty)
                labelStatus.Text = "Page loaded successfully!";
            else
                labelStatus.Text = "Page Error: " + erroMsg;

            Environment.Exit(0);
        }
        public void webBrowser1_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the new window event
            WebBrowser browser = (WebBrowser)sender; // Get the current WebBrowser control
            // Open the new URL in the current WebBrowser control
            browser.Navigate(browser.StatusText);
        }
        private void webpage_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                string domain;

                if (((WebBrowser)sender).Document != null)
                    domain = ((WebBrowser)sender).Document.Domain;
                else
                    throw new Exception("Page Docment does not exist");

                if (((WebBrowser)sender).Tag == null)
                    throw new Exception("Web Infos cannot be null");


                if (!((Dictionary<string, object>)((WebBrowser)sender).Tag).ContainsKey("URL_Title"))
                    throw new Exception("Web infos missing URL_Title Key");

                string strExpectedTitle = ((Dictionary<string, object>)((WebBrowser)sender).Tag)["URL_Title"].ToString();

                if (strExpectedTitle != null && !((WebBrowser)sender).Document.Title.Equals(strExpectedTitle))
                    throw new Exception("page reached mismatch");

            }
            catch (Exception error)
            {
                if (!((Dictionary<string, object>)((WebBrowser)sender).Tag).ContainsKey("NavigationError"))
                    throw new Exception("Web infos missing NavigationError Key");

                ((Dictionary<string, object>)((WebBrowser)sender).Tag)["NavigationError"] = error.Message;

                if (error.TargetSite.Name.Equals("GetDomain"))
                    ((Dictionary<string, object>)((WebBrowser)sender).Tag)["NavigationError"] = "Cannot load Page";

            }

            if (!((Dictionary<string, object>)((WebBrowser)sender).Tag).ContainsKey("Navigated"))
                throw new Exception("Web infos missing Navigated Key");

            ((Dictionary<string, object>)((WebBrowser)sender).Tag)["Navigated"] = true;

        }

        public string GetYieldThreadSafeWithLogin(string user, string password, string url,string urlTitle = null)
        {
            string errorMessage = string.Empty;

            try
            {
                using (WebBrowser webComponent = new WebBrowser())
                {
                    TabPage tab = new TabPage();
                    metroTabControl1.Controls.Add(tab);
                    metroTabControl1.SelectTab(metroTabControl1.TabCount - 1);
                    webComponent.Parent = tab;
                    webComponent.Dock = DockStyle.Fill;

                    webComponent.Navigating += new WebBrowserNavigatingEventHandler(webpage_Navigating);
                    webComponent.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);
                    webComponent.NewWindow += new CancelEventHandler(webBrowser1_NewWindow);


                    Dictionary<string, object> WebInfos = new Dictionary<string, object>() { { "NavigationError", "" }, { "Navigated", false }, { "URL_Title", urlTitle } };

                    webComponent.Tag = WebInfos;
                    webComponent.Navigate(url);

                    do
                    {
                        System.Windows.Forms.Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());


                    loginMQS(webComponent);

                    errorMessage = StartBrowser(webComponent, url, urlTitle);

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        Thread.Sleep(1000);
                        errorMessage = StartBrowser(webComponent, url, urlTitle);
                    }

                    metroTabControl1.SelectedTab.Text = webComponent.DocumentTitle;

                    navigateYieldTab(webComponent);

                    //New TAB "MQS - Yield Report"

                    webComponent.Navigate(urlYield);
                    WebInfos = new Dictionary<string, object>() { { "NavigationError", "" }, { "Navigated", false }, { "URL_Title", yield } };
                    webComponent.Tag = WebInfos;
                    do
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

                    metroTabControl1.SelectedTab.Text = webComponent.DocumentTitle;
                    Thread.Sleep(2000);

                    setYieldParameters(webComponent);
                    do
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

                    //Report Page
                    //exportData(webComponent);

                    documentTextParser(webComponent.DocumentText);

                    if (!string.IsNullOrEmpty(errorMessage))
                        throw new Exception(errorMessage);
                }


            }
            catch (Exception error)
            {
                errorMessage = error.Message;
            }
            return errorMessage;

        }
        public string GetYieldThreadSafe(string url, string urlTitle = null)
        {
            string errorMessage = string.Empty;

            try
            {
                using (WebBrowser webComponent = new WebBrowser())
                {
                    TabPage tab = new TabPage();
                    metroTabControl1.Controls.Add(tab);
                    metroTabControl1.SelectTab(metroTabControl1.TabCount - 1);
                    webComponent.Parent = tab;
                    webComponent.Dock = DockStyle.Fill;

                    webComponent.Navigating += new WebBrowserNavigatingEventHandler(webpage_Navigating);
                    webComponent.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);
                    webComponent.NewWindow += new CancelEventHandler(webBrowser1_NewWindow);


                    Dictionary<string, object> WebInfos = new Dictionary<string, object>() { { "NavigationError", "" }, { "Navigated", false }, { "URL_Title", urlTitle } };

                    webComponent.Tag = WebInfos;
                    webComponent.Navigate(url);

                    do
                    {
                        System.Windows.Forms.Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());
                  
                    errorMessage = StartBrowser(webComponent, url, urlTitle);

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        Thread.Sleep(1000);
                        errorMessage = StartBrowser(webComponent, url, urlTitle);
                    }

                    metroTabControl1.SelectedTab.Text = webComponent.DocumentTitle;

                    navigateYieldTab(webComponent);

                    //New TAB "MQS - Yield Report"

                    webComponent.Navigate(urlYield);
                    WebInfos = new Dictionary<string, object>() { { "NavigationError", "" }, { "Navigated", false }, { "URL_Title", yield } };
                    webComponent.Tag = WebInfos;
                    do
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

                    metroTabControl1.SelectedTab.Text = webComponent.DocumentTitle;
                    Thread.Sleep(2000);

                    setYieldParameters(webComponent);
                    do
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

                    //Report Page
                    //exportData(webComponent);

                    documentTextParser(webComponent.DocumentText);

                    if (!string.IsNullOrEmpty(errorMessage))
                        throw new Exception(errorMessage);
                }


            }
            catch (Exception error)
            {
                errorMessage = error.Message;
            }
            return errorMessage;

        }
        public void documentTextParser(string documentText)
        {          
            documentText = documentText.Substring(documentText.LastIndexOf("border=\"1\" rules=\"all\" cellSpacing=\"0\">") + 39);
            documentText = documentText.Replace("<TBODY>", "<HTML><HEAD></HEAD><BODY><FORM><TABLE><TBODY>");
            string cleanPage = documentText;
            string directoryName = textBoxSave.Text + "DailyYield.html";
            using (StreamWriter sw = File.CreateText(directoryName))
            {
                sw.Write(cleanPage);
            }
            directoryName = textBoxSave.Text + "DailyYield.xls";
            using (StreamWriter sw = File.CreateText(directoryName))
            {
                sw.Write(cleanPage);
            }



        }
        public void exportData(WebBrowser webComponent)
        {
            strAction = "Exporting";
            HtmlElement buttonExportElement = webComponent.Document.GetElementById("btn_export");
            if (buttonExportElement == null)
                throw new Exception("Cannot find btn_export Element");

            buttonExportElement.InvokeMember("click");

        }
        public void loginMQS(WebBrowser webComponent)
        {

            strAction = "EnterLogin";

            HtmlElement UserElement = webComponent.Document.GetElementById("ctl00_main_txtUserID");
            if (UserElement == null)
                throw new Exception("Cannot find User Name Element");
            UserElement.SetAttribute("value", user);

            HtmlElement PassElement = webComponent.Document.GetElementById("ctl00_main_txtPassword");
            if (PassElement == null)
                throw new Exception("Cannot find Password Element");
            PassElement.SetAttribute("value", password);

            HtmlElement buttonLoginElement = webComponent.Document.GetElementById("ctl00_main_btnLogin");
            if (buttonLoginElement == null)
                throw new Exception("Cannot find button login Element");
            buttonLoginElement.InvokeMember("click");

            Thread.Sleep(2000);

        }
        public void navigateYieldTab(WebBrowser webComponent)
        {
            strAction = "MqsLoad";

            HtmlElement tabMainElement = webComponent.Document.GetElementById("ctl00_main_tabMain_tabReports_tab");

            if (tabMainElement == null)
                throw new Exception("Cannot find tabMain Element");

            tabMainElement.InvokeMember("click");

            HtmlElement tabYieldElement = webComponent.Document.GetElementById("ctl00_main_tabMain_tabReports_HyperLink7");
            if (tabYieldElement == null)
                throw new Exception("Cannot find tabMain Hyperlink Element");

            tabYieldElement.InvokeMember("click");

        }
        public void setYieldParameters(WebBrowser webComponent)
        {

            HtmlElement LocationElement = webComponent.Document.GetElementById("LocationList");
            if (LocationElement == null)
                throw new Exception("Cannot find LocationList Element");

            LocationElement.SetAttribute("value", "16");

            HtmlElement dateT1 = webComponent.Document.GetElementById("TextBox1");
            if (dateT1 == null)
                throw new Exception("Cannot find LocationList Element");

            DateTime today = DateTime.Today;
            dateT1.SetAttribute("value", today.ToString("MM/dd/yyyy"));

            HtmlElement dateT2 = webComponent.Document.GetElementById("TextBox2");
            if (dateT2 == null)
                throw new Exception("Cannot find LocationList Element");
            dateT2.SetAttribute("value", today.ToString("MM/dd/yyyy"));


            HtmlElement timeT1 = webComponent.Document.GetElementById("TextBox6");
            if (timeT1 == null)
                throw new Exception("Cannot find LocationList Element");
            timeT1.SetAttribute("value", "00:00:00");


            HtmlElement timeT2 = webComponent.Document.GetElementById("TextBox7");
            if (timeT2 == null)
                throw new Exception("Cannot find LocationList Element");
            timeT2.SetAttribute("value", "23:59:00");


            HtmlElement button3Element = webComponent.Document.GetElementById("Button3");
            if (button3Element == null)
                throw new Exception("Cannot find button3 Element");

            button3Element.InvokeMember("click");
        }
        public string StartBrowser(WebBrowser webComponent, string url, string urlTitle = null)
        {
            string errorMessage = string.Empty;
            try
            {
                if (webComponent == null)
                {
                    webComponent = new WebBrowser();
                    webComponent.Navigating += new WebBrowserNavigatingEventHandler(webpage_Navigating);
                    webComponent.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);
                    webComponent.NewWindow += new CancelEventHandler(webBrowser1_NewWindow);

                    Dictionary<string, object> WebInfos = new Dictionary<string, object>() { { "NavigationError", "" }, { "Navigated", false }, { "URL_Title", "" }, { "RawResult", "" }, { "ResultObject", new List<MqsDefinitions.TestProcess>() } };

                    webComponent.Tag = WebInfos;
                }

                ((Dictionary<string, object>)webComponent.Tag)["URL_Title"] = urlTitle;

                strAction = "MqsLoad";

                webComponent.Navigate(url);
                do
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                    throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

            }
            catch (Exception error)
            {
                errorMessage = error.Message;
                MessageBox.Show("Error: " + errorMessage);
            }

            return errorMessage;

        }

        private string cleanString(string strDirty)
        {
            Regex trimmer = new Regex(@"\s\s+");
            string strTemp = strDirty.Replace("\r", "").Replace("\n", "");

            return trimmer.Replace(strTemp, " ");
        }

        public List<string> CaptureTAG(string SearchText, string strTAG)
        {
            List<string> strOutputs = new List<string>();
            string pattern = string.Format("<{0}.*?>(.*?)<\\/{0}>", strTAG);

            MatchCollection matches = Regex.Matches(SearchText, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in matches)
                strOutputs.Add(match.Groups[1].Value);

            return strOutputs;
        }

        public string CaptureTAGs(string SearchText, string strTAG)
        {
            string strOutput = string.Empty;
            string pattern = string.Format("<{0}.*?>(.*?)<\\/{0}>", strTAG);

            MatchCollection matches = Regex.Matches(SearchText, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in matches)
                strOutput += match.Groups[1].Value + ",";

            return strOutput.TrimEnd(',');
        }
    }
}
