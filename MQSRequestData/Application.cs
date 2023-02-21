using GlobalOperations.Definitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public string GetYieldThreadSafe(string url, out List<MqsDefinitions.TestProcess> FetchResults, string urlTitle = null)
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
                    webComponent.NewWindow += new CancelEventHandler(webBrowser1_NewWindow);


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

                    do
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

                    errorMessage = ParseYieldyData(webComponent.DocumentText, out FetchResults);

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
                    webComponent.NewWindow += new CancelEventHandler(webBrowser1_NewWindow);


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

                    //Report Page
                    /*
                    do
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

                    HtmlElement exportElement = webComponent.Document.GetElementById("btn_export");
                    if (exportElement == null)
                        throw new Exception("Cannot find btn_export Element");

                    exportElement.InvokeMember("click");*/

                    errorMessage = ParseYieldyData(webComponent.DocumentText, out FetchResults);

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
        public void loginMQS(WebBrowser webComponent)
        {

            strAction = "EnterLogin";

            HtmlElement UserElement = webComponent.Document.GetElementById("ctl00_main_txtUserID");
            UserElement.SetAttribute("value", user);

            HtmlElement PassElement = webComponent.Document.GetElementById("ctl00_main_txtPassword");
            PassElement.SetAttribute("value", password);

            HtmlElement buttonLoginElement = webComponent.Document.GetElementById("ctl00_main_btnLogin");
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
        private string ParseYieldyData(string strRawData, out List<MqsDefinitions.TestProcess> ResultsObject)
        {
            string errorMessage = string.Empty;
            ResultsObject = null;

            try
            {
                //check if has data for this track_id
                if (strRawData.Contains("** No Data Found **"))
                    throw new Exception("No data for this Serial");

                //get only testresults content======
                int nStart = strRawData.IndexOf("<INPUT name=\"hndExpandedChild\" id=\"hndExpandedChild\" type=\"hidden\">") + 67;
                int nEnd = strRawData.IndexOf("</DIV></TD></TR></TBODY></TABLE></DIV>", nStart) + 38;

                if (nEnd < nStart)
                    throw new Exception("Error to extract Clean Results from Page Loaded");

                string WebContentClean = strRawData.Substring(nStart, nEnd - nStart);
                //===================

                List<string> TestList = new List<string>();

                //break Test events===============
                int nbreak = 0;
                string strTestBreak = WebContentClean;

                do
                {
                    nbreak = strTestBreak.IndexOf("</TBODY></TABLE></DIV><!-- Begin:    Bread Crumb -->");

                    if (nbreak != -1)
                    {
                        nbreak += 52;
                        TestList.Add(strTestBreak.Substring(0, nbreak));
                        strTestBreak = strTestBreak.Remove(0, nbreak);

                    }

                } while (nbreak != -1);
                //===================

                ResultsObject = new List<MqsDefinitions.TestProcess>();


                int nIndexloop = 0;

                for (int nIndex = TestList.Count - 1; nIndex >= 0; nIndex--)
                {
                    //capture only headers
                    nStart = TestList[nIndex].IndexOf("<TR align=\"right\"");
                    nEnd = TestList[nIndex].IndexOf("</TR>", nStart) + 5;
                    string strHeader = TestList[nIndex].Substring(nStart, nEnd - nStart);
                    List<string> TestInfos = CaptureTAG(strHeader, "TD");
                    //---------------             

                    if (TestInfos.Count != 27)
                        throw new Exception("History Header seems to be invalid!");

                    MqsDefinitions.TestProcess unitInfoTemp = new MqsDefinitions.TestProcess();

                    if (!TestInfos[1].Contains("&nbsp")) unitInfoTemp.TestHeaders.TimeStamp = Convert.ToDateTime(TestInfos[1]);
                    if (!TestInfos[3].Contains("&nbsp")) unitInfoTemp.TestHeaders.TrackID = cleanString(TestInfos[3]);
                    if (!TestInfos[4].Contains("&nbsp")) unitInfoTemp.TestHeaders.OverallPF = cleanString(TestInfos[4]);
                    if (!TestInfos[5].Contains("&nbsp")) unitInfoTemp.TestHeaders.Prime = cleanString(TestInfos[5]);
                    if (!TestInfos[6].Contains("&nbsp")) unitInfoTemp.TestHeaders.Model = cleanString(TestInfos[6]);
                    if (!TestInfos[7].Contains("&nbsp")) unitInfoTemp.TestHeaders.Location = cleanString(TestInfos[7]);
                    if (!TestInfos[8].Contains("&nbsp")) unitInfoTemp.TestHeaders.Process = cleanString(TestInfos[8]);
                    if (!TestInfos[9].Contains("&nbsp")) unitInfoTemp.TestHeaders.Station = cleanString(TestInfos[9]);
                    if (!TestInfos[10].Contains("&nbsp")) unitInfoTemp.TestHeaders.Fixture = cleanString(TestInfos[10]);
                    if (!TestInfos[11].Contains("&nbsp")) unitInfoTemp.TestHeaders.Testtime = Convert.ToDouble(TestInfos[11]);
                    if (!TestInfos[12].Contains("&nbsp")) unitInfoTemp.TestHeaders.UnitId = cleanString(TestInfos[12]);
                    if (!TestInfos[13].Contains("&nbsp")) unitInfoTemp.TestHeaders.FailureTestcode = cleanString(TestInfos[13]);
                    if (!TestInfos[14].Contains("&nbsp")) unitInfoTemp.TestHeaders.TestcodeDescription = cleanString(TestInfos[14]);
                    if (!TestInfos[15].Contains("&nbsp")) unitInfoTemp.TestHeaders.PassFail = cleanString(TestInfos[15]);
                    if (!TestInfos[16].Contains("&nbsp")) unitInfoTemp.TestHeaders.TestVal = Convert.ToDouble(TestInfos[16]);
                    if (!TestInfos[17].Contains("&nbsp")) unitInfoTemp.TestHeaders.TextVal = cleanString(TestInfos[17]);
                    if (!TestInfos[18].Contains("&nbsp")) unitInfoTemp.TestHeaders.LoLimit = Convert.ToDouble(TestInfos[18]);
                    if (!TestInfos[19].Contains("&nbsp")) unitInfoTemp.TestHeaders.UpLimit = Convert.ToDouble(TestInfos[19]);
                    if (!TestInfos[20].Contains("&nbsp")) unitInfoTemp.TestHeaders.FailDesc = cleanString(TestInfos[20]);
                    if (!TestInfos[22].Contains("&nbsp")) unitInfoTemp.TestHeaders.TestPlatform = cleanString(TestInfos[22]);
                    if (!TestInfos[23].Contains("&nbsp")) unitInfoTemp.TestHeaders.CarrierId = cleanString(TestInfos[23]);


                    //Capture only Results
                    string strTesResults = TestList[nIndex].Substring(nEnd);
                    strTesResults = strTesResults.Remove(0, strTesResults.IndexOf("<TR align=\"right\""));

                    unitInfoTemp.testResultsList = new List<MqsDefinitions.TestResult>();

                    List<string> TestTagList = CaptureTAG(strTesResults, "TR");

                    for (int nResultIndex = 0; nResultIndex < TestTagList.Count; nResultIndex++)
                    {
                        List<string> Resultsinfo = CaptureTAG(TestTagList[nResultIndex], "TD");

                        MqsDefinitions.TestResult resultInfoTemp = new MqsDefinitions.TestResult();

                        resultInfoTemp.LinkID = nIndexloop;
                        resultInfoTemp.ID = nResultIndex;
                        if (!Resultsinfo[0].Contains("&nbsp")) resultInfoTemp.TestCode = cleanString(Resultsinfo[0]);
                        if (!Resultsinfo[1].Contains("&nbsp")) resultInfoTemp.TestCodeDesc = cleanString(Resultsinfo[1]);
                        if (!Resultsinfo[2].Contains("&nbsp")) resultInfoTemp.PassFail = cleanString(Resultsinfo[2]);
                        if (!Resultsinfo[3].Contains("&nbsp")) resultInfoTemp.TestVal = Convert.ToDouble(Resultsinfo[3]);
                        if (!Resultsinfo[4].Contains("&nbsp")) resultInfoTemp.TextVal = cleanString(Resultsinfo[4]);
                        if (!Resultsinfo[5].Contains("&nbsp")) resultInfoTemp.LoLimit = Convert.ToDouble(Resultsinfo[5]);
                        if (!Resultsinfo[6].Contains("&nbsp")) resultInfoTemp.UpLimit = Convert.ToDouble(Resultsinfo[6]);
                        if (!Resultsinfo[7].Contains("&nbsp")) resultInfoTemp.Testtime = Convert.ToDouble(Resultsinfo[7]);

                        unitInfoTemp.testResultsList.Add(resultInfoTemp);

                    }
                    //----------

                    ResultsObject.Add(unitInfoTemp);

                    nIndexloop++;
                }
            }
            catch (Exception error)
            {
                ResultsObject = null;
                errorMessage = error.Message;
            }

            return errorMessage;

        }



    }
}
