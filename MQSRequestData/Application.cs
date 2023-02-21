using GlobalOperations.Definitions;
using MQSRequestDataYield;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        string home = "MQS Home";
        string user = "jagapatr";
        string password = "L@ura2022.7";
        string erroMsg = string.Empty;
        string strAction = string.Empty;

        private void webpage_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            ((Dictionary<string, object>)((WebBrowser)sender).Tag)["NavigationError"] = string.Empty;
            ((Dictionary<string, object>)((WebBrowser)sender).Tag)["Navigated"] = false;

        }

        private void buttonRun_Click(object sender, EventArgs e)
        {

            List<MqsDefinitions.TestProcess> UnitInfoResults;
            erroMsg = GetYieldThreadSafeWithLogin(user, password, url, out UnitInfoResults, home);
            if (erroMsg == string.Empty)
                labelStatus.Text = "Page OK!";
            else
                labelStatus.Text = "Page Error: " + erroMsg;
        }
        public void webBrowser1_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the new window event

            WebBrowser browser = (WebBrowser)sender; // Get the current WebBrowser control

            // Open the new URL in the current WebBrowser control
            browser.Navigate(browser.StatusText);
        }

        public void webBrowser2_NewWindow(object sender, EventArgs e)
        {

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
    
        public string GetYieldThreadSafeWithLogin(string user, string password, string url, out List<MqsDefinitions.TestProcess> FetchResults, string urlTitle = null)
        {
            string errorMessage = string.Empty;
            FetchResults = null;

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
                    webComponent.NewWindow += new CancelEventHandler(webBrowser1_NewWindow); //debug


                    Dictionary<string, object> WebInfos = new Dictionary<string, object>() { { "NavigationError", "" }, { "Navigated", false }, { "URL_Title", urlTitle } };

                    webComponent.Tag = WebInfos;

                    webComponent.Navigate(url);

                    do
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

                    strAction = "EnterLogin";

                    HtmlElement UserElement = webComponent.Document.GetElementById("ctl00_main_txtUserID");
                    UserElement.SetAttribute("value", user);

                    HtmlElement PassElement = webComponent.Document.GetElementById("ctl00_main_txtPassword");
                    PassElement.SetAttribute("value", password);

                    HtmlElement buttonLoginElement = webComponent.Document.GetElementById("ctl00_main_btnLogin");
                    buttonLoginElement.InvokeMember("click");

                    Thread.Sleep(2000);

                    errorMessage = StartBrowser(webComponent, url, urlTitle);

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        Thread.Sleep(1000);
                        errorMessage = StartBrowser(webComponent, url, urlTitle);
                    }

                    strAction = "MqsLoad";

                    metroTabControl1.SelectedTab.Text = webComponent.DocumentTitle;

                    HtmlElement tabMainElement = webComponent.Document.GetElementById("ctl00_main_tabMain_tabReports_tab");

                    if (tabMainElement == null)
                        throw new Exception("Cannot find tabMain Element");

                    tabMainElement.InvokeMember("click");

                    HtmlElement tabYieldElement = webComponent.Document.GetElementById("ctl00_main_tabMain_tabReports_HyperLink7");
                    if (tabYieldElement == null)
                        throw new Exception("Cannot find tabMain Hyperlink Element");

                    tabYieldElement.InvokeMember("click");

                    // new Tab opening to do!

                    HtmlElement LocationElement = webComponent.Document.GetElementById("LocationList");
                    if (LocationElement == null)
                        throw new Exception("Cannot find LocationList Element");

                    LocationElement.SetAttribute("value", "16");

                    do
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());


                    errorMessage = ParseUnitHistoryData(webComponent.DocumentText, out FetchResults);

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
                    webComponent.NewWindow += new CancelEventHandler(webBrowser1_NewWindow); //debug

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
     


    }
}
