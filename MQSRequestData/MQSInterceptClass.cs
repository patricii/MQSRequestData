using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalOperations.LogDebug;
using GlobalOperations.Definitions;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;

namespace MQSInterceptClass
{
    public class MQSInterceptClassI
    {
        public WebBrowser m_webComponent;
        string strAction = string.Empty;

        public DebugMessage DebugType = DebugMessage.ENGINE_DEBUG;
        public bool bLogEnabled = true;

        public List<MqsDefinitions.TestProcess> m_GlobalResults
        {
            get
            {
                if (m_webComponent == null)
                    return null;

                if (m_webComponent.Tag == null)
                    return null;

                if (!((Dictionary<string, object>)m_webComponent.Tag).ContainsKey("ResultObject"))
                    return null;

                return (List<MqsDefinitions.TestProcess>)((Dictionary<string, object>)m_webComponent.Tag)["ResultObject"];

            }
        }

        public MQSInterceptClassI(WebBrowser webComponent)
        {
            m_webComponent = webComponent;

            m_webComponent.Navigating += new WebBrowserNavigatingEventHandler(webpage_Navigating);
            m_webComponent.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);

            Dictionary<string, object> WebInfos = new Dictionary<string, object>() { { "NavigationError", "" }, { "Navigated", "false" }, { "URL_Title", "" }, { "RawResult", "" }, { "ResultObject", new List<MqsDefinitions.TestProcess>() } };

            m_webComponent.Tag = WebInfos;

        }

        public MQSInterceptClassI()
        {

        }

        ~MQSInterceptClassI()
        {
            if (m_webComponent != null)
            {
                m_webComponent.Navigating -= new WebBrowserNavigatingEventHandler(webpage_Navigating);
                m_webComponent.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);

                if (m_webComponent.Tag != null)
                    m_webComponent.Tag = null;

                m_webComponent.Dispose();
                m_webComponent = null;
            }

            GC.Collect();
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


        private void webpage_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            ((Dictionary<string, object>)((WebBrowser)sender).Tag)["NavigationError"] = string.Empty;
            ((Dictionary<string, object>)((WebBrowser)sender).Tag)["Navigated"] = false;

        }

        public string StartBrowser(string url, string urlTitle = null)
        {
            string errorMessage = string.Empty;

            try
            {
                if (m_webComponent == null)
                {
                    m_webComponent = new WebBrowser();
                    m_webComponent.Navigating += new WebBrowserNavigatingEventHandler(webpage_Navigating);
                    m_webComponent.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);

                    Dictionary<string, object> WebInfos = new Dictionary<string, object>() { { "NavigationError", "" }, { "Navigated", false }, { "URL_Title", "" }, { "RawResult", "" }, { "ResultObject", new List<MqsDefinitions.TestProcess>() } };

                    m_webComponent.Tag = WebInfos;
                }

                ((Dictionary<string, object>)m_webComponent.Tag)["URL_Title"] = urlTitle;

                strAction = "MqsLoad";

                m_webComponent.Navigate(url);

                do
                {  
                    Application.DoEvents();
                    Thread.Sleep(1);
                } while ((bool)((Dictionary<string, object>)m_webComponent.Tag)["Navigated"] == false);

                if (!string.IsNullOrEmpty(((Dictionary<string, object>)m_webComponent.Tag)["NavigationError"].ToString()))
                    throw new Exception(((Dictionary<string, object>)m_webComponent.Tag)["NavigationError"].ToString());

            }
            catch (Exception error)
            {
                errorMessage = error.Message;
                LogError("Error: " + errorMessage);
                MessageBox.Show("Error: " + errorMessage);
            }


            return errorMessage;

        }
     
        public string StartBrowserWithLogin(string url, string urlTitle, string UserName, string PassWord)
        {
            string errorMessage = string.Empty;

            try
            {
                if (m_webComponent == null)
                {
                    m_webComponent = new WebBrowser();

                    m_webComponent.Navigating += new WebBrowserNavigatingEventHandler(webpage_Navigating);
                    m_webComponent.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);

                    Dictionary<string, object> WebInfos = new Dictionary<string, object>() { { "NavigationError", "" }, { "Navigated", false }, { "URL_Title", "" }, { "RawResult", "" }, { "ResultObject", new List<MqsDefinitions.TestProcess>() } };

                    m_webComponent.Tag = WebInfos;
                }

                ((Dictionary<string, object>)m_webComponent.Tag)["URL_Title"] = "MQS Home";
                strAction = "MQSopen";

                m_webComponent.Navigate(url);


                do
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                } while ((bool)((Dictionary<string, object>)m_webComponent.Tag)["Navigated"] == false);

                if (!string.IsNullOrEmpty(((Dictionary<string, object>)m_webComponent.Tag)["NavigationError"].ToString()))
                    throw new Exception(((Dictionary<string, object>)m_webComponent.Tag)["NavigationError"].ToString());

                strAction = "EnterLogin";

                HtmlElement UserElement = m_webComponent.Document.GetElementById("ctl00_main_txtUserID");
                UserElement.SetAttribute("value", UserName);

                HtmlElement PassElement = m_webComponent.Document.GetElementById("ctl00_main_txtPassword");
                PassElement.SetAttribute("value", PassWord);

                HtmlElement buttonLoginElement = m_webComponent.Document.GetElementById("ctl00_main_btnLogin");
                buttonLoginElement.InvokeMember("click");

                do
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                } while ((bool)((Dictionary<string, object>)m_webComponent.Tag)["Navigated"] == false);

                if (!string.IsNullOrEmpty(((Dictionary<string, object>)m_webComponent.Tag)["NavigationError"].ToString()))
                    throw new Exception(((Dictionary<string, object>)m_webComponent.Tag)["NavigationError"].ToString());

                Thread.Sleep(2000);

                errorMessage = StartBrowser(url, urlTitle);

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Thread.Sleep(1000);
                    errorMessage = StartBrowser(url, urlTitle);
                }

            }
            catch (Exception error)
            {
                errorMessage = error.Message;
                LogError("Error: " + errorMessage);
            }


            return errorMessage;

        }

        public void ClearWebComponent()
        {
            if (m_webComponent != null)
            {
                m_webComponent.Navigating -= new WebBrowserNavigatingEventHandler(webpage_Navigating);
                m_webComponent.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);


                m_webComponent.Dispose();
                m_webComponent = null;
            }
        }

        public string QueryUnitThread(string trackID, string url, out List<MqsDefinitions.TestProcess> FetchResults, string urlTitle = null)
        {
            string errorMessage = string.Empty;
            FetchResults = null;

            try
            {
                //m_strurlTitle = urlTitle;
                strAction = "MqsLoad";

                Thread NavigateThread;

                LogInfo("Creating Navigation Thread...");

                List<MqsDefinitions.TestProcess> ResultsObject = null;

                NavigateThread = new Thread(delegate () { errorMessage = GetUnitHistoryThreadSafe(trackID, url, out ResultsObject, urlTitle); });

                LogInfo("Starting Navigation Thread...");
                NavigateThread.SetApartmentState(ApartmentState.STA);
                NavigateThread.Start();

                while (NavigateThread.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    throw new Exception(errorMessage);

                FetchResults = ResultsObject;

            }
            catch (Exception error)
            {
                FetchResults = null;
                errorMessage = error.Message;
                LogError("Error: " + errorMessage);
            }


            return errorMessage;

        }

        public string QueryProcessThread(string urlDirect, string urlTitle, string urlDetail, string urlDetailTitle, string process, out List<MqsDefinitions.TestProcess> FetchResults)
        {
            string errorMessage = string.Empty;
            FetchResults = new List<MqsDefinitions.TestProcess>();

            try
            {
                //m_strurlTitle = urlTitle;
                strAction = "MqsLoad";

                Thread NavigateThread;

                LogInfo("Creating Navigation Thread...");

                List<MqsDefinitions.TestProcess> ResultsObject = new List<MqsDefinitions.TestProcess>();

                NavigateThread = new Thread(delegate () { errorMessage = GetUnitProcessThreadSafe(urlDirect, urlTitle, urlDetail, urlDetailTitle, process, out ResultsObject); });

                LogInfo("Starting Navigation Thread...");
                NavigateThread.SetApartmentState(ApartmentState.STA);
                NavigateThread.Start();

                while (NavigateThread.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    throw new Exception(errorMessage);

                FetchResults = ResultsObject;

            }
            catch (Exception error)
            {
                FetchResults = new List<MqsDefinitions.TestProcess>();
                errorMessage = error.Message;
                LogError("Error: " + errorMessage);
            }


            return errorMessage;

        }
        private string GetUnitHistoryThreadSafe(string trackID, string url, out List<MqsDefinitions.TestProcess> FetchResults, string urlTitle = null)
        {
            string errorMessage = string.Empty;
            FetchResults = null;

            try
            {
                LogInfo("Navigation Thread Started");

                using (WebBrowser webComponent = new WebBrowser())
                {
                    webComponent.Navigating += new WebBrowserNavigatingEventHandler(webpage_Navigating);
                    webComponent.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);

                    Dictionary<string, object> WebInfos = new Dictionary<string, object>() { { "NavigationError", "" }, { "Navigated", false }, { "URL_Title", urlTitle } };

                    webComponent.Tag = WebInfos;

                    LogInfo(string.Format("Navigating to url: '{0}'", url));

                    webComponent.Navigate(url);

                    do
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    LogInfo(string.Format("URL loaded!!!"));

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

                    strAction = "UnitLoad";

                    LogInfo(string.Format("Getting Unit Textbox and Filling Track_id: '{0}'...", trackID));

                    HtmlElement textElement = webComponent.Document.GetElementById("trackid");

                    if (textElement == null)
                        throw new Exception("Cannot find Textbox Element");

                    textElement.SetAttribute("value", trackID);

                    LogInfo(string.Format("Sellecting View Mode to normal_view..."));

                    HtmlElement comboElement = webComponent.Document.GetElementById("normal_view");

                    if (comboElement == null)
                        throw new Exception("Cannot find Radiobox of Full testresults Element");

                    comboElement.InvokeMember("click");

                    LogInfo(string.Format("Getting View Button and performing Click..."));

                    HtmlElement buttonViewElement = webComponent.Document.GetElementById("btn_view");

                    if (buttonViewElement == null)
                        throw new Exception("Cannot find View Button Element");

                    buttonViewElement.InvokeMember("click");

                    LogInfo(string.Format("Waiting for history result..."));

                    do
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

                    LogInfo(string.Format("History result LOADED!!!"));

                    LogInfo(string.Format("Parsing data to lookup result..."));

                    errorMessage = ParseUnitHistoryData(webComponent.DocumentText, out FetchResults);

                    if (!string.IsNullOrEmpty(errorMessage))
                        throw new Exception(errorMessage);


                    LogInfo(string.Format("Unit test Data parsed!!!"));

                }


            }
            catch (Exception error)
            {
                errorMessage = error.Message;
                LogInfo("Error: " + errorMessage);
            }

            return errorMessage;


        }

        private string GetUnitProcessThreadSafe(string urlDirect, string urlTitle, string urlDetail, string urlDetailTitle, string process, out List<MqsDefinitions.TestProcess> FetchResult)
        {
            string errorMessage = string.Empty;
            FetchResult = new List<MqsDefinitions.TestProcess>();

            try
            {
                LogInfo("Navigation Thread Started");

                using (WebBrowser webComponent = new WebBrowser())
                {
                    webComponent.Navigating += new WebBrowserNavigatingEventHandler(webpage_Navigating);
                    webComponent.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);

                    Dictionary<string, object> WebInfos = new Dictionary<string, object>() { { "NavigationError", "" }, { "Navigated", false }, { "URL_Title", urlTitle } };

                    webComponent.Tag = WebInfos;

                    LogInfo(string.Format("Navigating to url: '{0}'", urlDirect));

                    webComponent.Navigate(urlDirect);

                    do
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                    LogInfo(string.Format("URL '{0}' loaded!!!", urlDirect));

                    if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                        throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

                    List<MqsDefinitions.TestInfo> TestinfoList = new List<MqsDefinitions.TestInfo>();

                    errorMessage = ParseProcessHeaderData(webComponent.DocumentText, process, out TestinfoList);

                    if (!string.IsNullOrEmpty(errorMessage))
                        throw new Exception(errorMessage);

                    foreach (MqsDefinitions.TestInfo ThisProcessInfo in TestinfoList)
                    {
                        MqsDefinitions.TestProcess tempResults = new MqsDefinitions.TestProcess();

                        tempResults.TestHeaders = ThisProcessInfo;

                        string strdetailUrl = string.Format("{1}{0}", tempResults.TestHeaders.FailDesc, urlDetail);

                        ////Capture only Results
                        LogInfo(string.Format("Navigating to url: '{0}'", strdetailUrl));

                        ((Dictionary<string, object>)webComponent.Tag)["URL_Title"] = urlDetailTitle;

                        webComponent.Navigate(strdetailUrl);

                        do
                        {
                            Application.DoEvents();
                            Thread.Sleep(1);
                        } while ((bool)((Dictionary<string, object>)webComponent.Tag)["Navigated"] == false);

                        LogInfo(string.Format("URL '{0}' loaded!!!", strdetailUrl));

                        if (!string.IsNullOrEmpty(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString()))
                            throw new Exception(((Dictionary<string, object>)webComponent.Tag)["NavigationError"].ToString());

                        errorMessage = ParseProcessTestData(webComponent.DocumentText, out tempResults.testResultsList);

                        if (!string.IsNullOrEmpty(errorMessage))
                            throw new Exception(errorMessage);

                        FetchResult.Add(tempResults);

                        LogInfo(string.Format("Unit test Data parsed!!!"));

                    }

                }

            }
            catch (Exception error)
            {
                errorMessage = error.Message;
                LogInfo("Error: " + errorMessage);
            }

            return errorMessage;


        }

        public string QueryUnit(string track_id)
        {
            string errorMessage = string.Empty;

            try
            {
                if (m_webComponent == null)
                    throw new Exception("Component Web cannot be null");

                strAction = "UnitLoad";

                HtmlElement textElement = m_webComponent.Document.GetElementById("trackid");
                textElement.SetAttribute("value", track_id);

                HtmlElement comboElement = m_webComponent.Document.GetElementById("normal_view");
                comboElement.InvokeMember("click");

                HtmlElement buttonViewElement = m_webComponent.Document.GetElementById("btn_view");
                buttonViewElement.InvokeMember("click");

                do
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                } while ((bool)((Dictionary<string, object>)m_webComponent.Tag)["Navigated"] == false);

                ((Dictionary<string, object>)m_webComponent.Tag)["RawResult"] = m_webComponent.DocumentText;

                List<MqsDefinitions.TestProcess> ParsedResults = null;

                errorMessage = ParseUnitHistoryData(m_webComponent.DocumentText, out ParsedResults);

                if (!string.IsNullOrEmpty(errorMessage))
                    throw new Exception(errorMessage);

                ((Dictionary<string, object>)m_webComponent.Tag)["ResultObject"] = ParsedResults;


            }
            catch (Exception error)
            {
                errorMessage = error.Message;
                LogError("Error: " + errorMessage);
            }

            return errorMessage;

        }

        public string QueryUnitProcess(string urlDirect, string urlTitle, string urlDetail, string urlDetailTitle, string process)
        {
            string errorMessage = string.Empty;

            try
            {
                errorMessage = GetWebResult(urlDirect, urlTitle);

                if (!string.IsNullOrEmpty(errorMessage))
                    throw new Exception(errorMessage);

                List<MqsDefinitions.TestInfo> HeaderList = new List<MqsDefinitions.TestInfo>();

                errorMessage = ParseProcessHeaderData(m_webComponent.DocumentText, process, out HeaderList);

                if (!string.IsNullOrEmpty(errorMessage))
                    throw new Exception(errorMessage);

                List<MqsDefinitions.TestProcess> ParsedResultList = new List<MqsDefinitions.TestProcess>();

                foreach (MqsDefinitions.TestInfo thisProcessHerader in HeaderList)
                {
                    MqsDefinitions.TestProcess ParsedResult = new MqsDefinitions.TestProcess();

                    ParsedResult.TestHeaders = thisProcessHerader;

                    string strdetailUrl = string.Format("{1}{0}", ParsedResult.TestHeaders.FailDesc, urlDetail);
                    ////Capture only Results
                    errorMessage = GetWebResult(strdetailUrl, urlDetailTitle);

                    if (!string.IsNullOrEmpty(errorMessage))
                        throw new Exception(errorMessage);

                    errorMessage = ParseProcessTestData(m_webComponent.DocumentText, out ParsedResult.testResultsList);

                    if (!string.IsNullOrEmpty(errorMessage))
                        throw new Exception(errorMessage);

                    ParsedResultList.Add(ParsedResult);
                }



                ((Dictionary<string, object>)m_webComponent.Tag)["ResultObject"] = ParsedResultList;


            }
            catch (Exception error)
            {
                errorMessage = error.Message;
                LogError("Error: " + errorMessage);
            }

            return errorMessage;

        }

        public string Export()
        {
            string errorMessage = string.Empty;

            try
            {
                //OnStatusChanged("Getting Unit History");

                strAction = "Exporting";

                HtmlElement buttonExportElement = m_webComponent.Document.GetElementById("btn_export");
                buttonExportElement.InvokeMember("click");

            }
            catch (Exception error)
            {
                errorMessage = error.Message;
                LogError("Error: " + errorMessage);
            }

            return errorMessage;

        }

        private string ParseUnitHistoryData(string strRawData, out List<MqsDefinitions.TestProcess> ResultsObject)
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

        private string ParseProcessHeaderData(string strRawData, string ProcessName, out List<MqsDefinitions.TestInfo> unitInfoListTemp)
        {
            string errorMessage = string.Empty;
            unitInfoListTemp = new List<MqsDefinitions.TestInfo>();

            try
            {
                //check if has data for this track_id
                if (strRawData.Contains("<TD>TestPlatform</TD></TR></TBODY></TABLE></TD></TR>"))
                    throw new Exception("No data for this Serial");

                //get only testresults content======
                int nStart = strRawData.IndexOf("<TD>TestPlatform</TD></TR>") + 26;
                int nEnd = strRawData.IndexOf("</TR></TBODY></TABLE></TD></TR>", nStart) + 21;

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
                    nbreak = strTestBreak.IndexOf("</TD></TR>");

                    if (nbreak != -1)
                    {
                        nbreak += 10;
                        TestList.Add(strTestBreak.Substring(0, nbreak));
                        strTestBreak = strTestBreak.Remove(0, nbreak);

                    }

                } while (nbreak != -1);
                //===================

                List<string> strProcessHeaderList = new List<string>();

                for (int nIndex = TestList.Count - 1; nIndex >= 0; nIndex--)
                {
                    List<string> ProcessInfos = CaptureTAG(TestList[nIndex], "TD");
                    //---------------             

                    if (ProcessInfos.Count != 19)
                        throw new Exception("History Header seems to be invalid!");

                    if (!string.IsNullOrEmpty(ProcessInfos[7]) && ProcessInfos[7].Equals(ProcessName))
                        strProcessHeaderList.Add(TestList[nIndex]);

                }

                if (strProcessHeaderList.Count < 1)
                    throw new Exception(string.Format("Process {0} not found at Unit history", ProcessName));

                foreach (string strProcessHeader in strProcessHeaderList)
                {
                    List<string> TestInfos = CaptureTAG(strProcessHeader, "TD");

                    MqsDefinitions.TestInfo unitInfoTemp = new MqsDefinitions.TestInfo();

                    if (!TestInfos[1].Contains("&nbsp")) unitInfoTemp.TimeStamp = Convert.ToDateTime(TestInfos[1]);
                    if (!TestInfos[2].Contains("&nbsp")) unitInfoTemp.TrackID = CaptureTAGs(TestInfos[2], "A");
                    if (!TestInfos[3].Contains("&nbsp")) unitInfoTemp.OverallPF = cleanString(TestInfos[3]);
                    if (!TestInfos[4].Contains("&nbsp")) unitInfoTemp.Prime = cleanString(TestInfos[4]);
                    if (!TestInfos[5].Contains("&nbsp")) unitInfoTemp.Model = cleanString(TestInfos[5]);
                    if (!TestInfos[6].Contains("&nbsp")) unitInfoTemp.Location = cleanString(TestInfos[6]);
                    if (!TestInfos[7].Contains("&nbsp")) unitInfoTemp.Process = cleanString(TestInfos[7]);
                    if (!TestInfos[8].Contains("&nbsp")) unitInfoTemp.Station = cleanString(TestInfos[8]);
                    if (!TestInfos[9].Contains("&nbsp")) unitInfoTemp.Fixture = cleanString(TestInfos[9]);
                    if (!TestInfos[17].Contains("&nbsp")) unitInfoTemp.Testtime = Convert.ToDouble(TestInfos[17]);
                    if (!TestInfos[10].Contains("&nbsp")) unitInfoTemp.UnitId = CaptureTAGs(TestInfos[10], "A");
                    if (!TestInfos[11].Contains("&nbsp")) unitInfoTemp.FailureTestcode = cleanString(TestInfos[11]);
                    if (!TestInfos[12].Contains("&nbsp")) unitInfoTemp.TestcodeDescription = cleanString(TestInfos[12]);
                    if (!TestInfos[13].Contains("&nbsp")) unitInfoTemp.PassFail = cleanString(TestInfos[13]);
                    if (!TestInfos[14].Contains("&nbsp")) unitInfoTemp.TestVal = Convert.ToDouble(TestInfos[14]);
                    if (!TestInfos[15].Contains("&nbsp")) unitInfoTemp.LoLimit = Convert.ToDouble(TestInfos[15]);
                    if (!TestInfos[16].Contains("&nbsp")) unitInfoTemp.UpLimit = Convert.ToDouble(TestInfos[16]);
                    if (!TestInfos[18].Contains("&nbsp")) unitInfoTemp.TestPlatform = cleanString(TestInfos[18]);

                    //Mount TestDetailsURL

                    //get only testresults content======
                    nStart = TestInfos[0].IndexOf("false, \"\", \"") + 12;
                    nEnd = TestInfos[0].IndexOf("\", false, false", nStart);

                    if (nEnd < nStart)
                        throw new Exception("Error to extract Details URL Results from Page Loaded");

                    unitInfoTemp.FailDesc = TestInfos[0].Substring(nStart, nEnd - nStart);

                    unitInfoListTemp.Add(unitInfoTemp);
                }

            }
            catch (Exception error)
            {
                unitInfoListTemp = null;
                errorMessage = error.Message;
            }

            return errorMessage;
        }

        private string ParseProcessTestData(string strRawData, out List<MqsDefinitions.TestResult> testResultsList)
        {
            string errorMessage = string.Empty;
            testResultsList = new List<MqsDefinitions.TestResult>();

            try
            {
                //get only testresults content======
                int nStart = strRawData.IndexOf("<TD>TestTime</TD></TR>") + 22;
                int nEnd = strRawData.IndexOf("</TBODY></TABLE></TD>", nStart);

                if (nEnd < nStart)
                    throw new Exception("Error to extract Clean Results from Page Loaded");

                string WebContentClean = strRawData.Substring(nStart, nEnd - nStart);
                //===================

                List<string> TestTagList = CaptureTAG(WebContentClean, "TR");

                for (int nResultIndex = 0; nResultIndex < TestTagList.Count; nResultIndex++)
                {
                    List<string> Resultsinfo = CaptureTAG(TestTagList[nResultIndex], "TD");

                    MqsDefinitions.TestResult resultInfoTemp = new MqsDefinitions.TestResult();

                    //resultInfoTemp.LinkID = nIndexloop;
                    resultInfoTemp.ID = nResultIndex;
                    if (!Resultsinfo[0].Contains("&nbsp")) resultInfoTemp.TestCode = cleanString(Resultsinfo[0]);
                    if (!Resultsinfo[1].Contains("&nbsp")) resultInfoTemp.TestCodeDesc = cleanString(Resultsinfo[1]);
                    if (!Resultsinfo[2].Contains("&nbsp")) resultInfoTemp.PassFail = cleanString(Resultsinfo[2]);
                    if (!Resultsinfo[3].Contains("&nbsp")) resultInfoTemp.TestVal = Convert.ToDouble(Resultsinfo[3]);
                    if (!Resultsinfo[4].Contains("&nbsp")) resultInfoTemp.LoLimit = Convert.ToDouble(Resultsinfo[4]);
                    if (!Resultsinfo[5].Contains("&nbsp")) resultInfoTemp.UpLimit = Convert.ToDouble(Resultsinfo[5]);
                    if (!Resultsinfo[6].Contains("&nbsp")) resultInfoTemp.Testtime = Convert.ToDouble(Resultsinfo[6]);

                    testResultsList.Add(resultInfoTemp);

                }
                //----------            

            }
            catch (Exception error)
            {
                testResultsList = new List<MqsDefinitions.TestResult>();
                errorMessage = error.Message;
                LogError("Error: " + errorMessage);
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

        public string LookupOcourrence(string strProcess, string strTestCode, out MqsDefinitions.TestResult result, bool bFirstOccurrence, List<MqsDefinitions.TestProcess> UnitInfoResults = null)
        {
            string errorMessage = string.Empty;
            result = new MqsDefinitions.TestResult();

            try
            {

                if (UnitInfoResults == null)
                {
                    if (m_webComponent == null)
                        throw new Exception("Component Web cannot be null");

                    if (m_webComponent.Tag == null)
                        throw new Exception("Web Infos cannot be null");

                    if (!((Dictionary<string, object>)m_webComponent.Tag).ContainsKey("ResultObject"))
                        throw new Exception("Web infos missing ResultObject Key");

                    UnitInfoResults = (List<MqsDefinitions.TestProcess>)((Dictionary<string, object>)m_webComponent.Tag)["ResultObject"];
                }


                if (bFirstOccurrence)
                {
                    for (int nProcIDX = 0; nProcIDX < UnitInfoResults.Count; nProcIDX++)
                    {
                        if (!string.IsNullOrEmpty(strProcess) && !UnitInfoResults[nProcIDX].TestHeaders.Process.Equals(strProcess))
                            continue;

                        List<MqsDefinitions.TestResult> ProcessResults = new List<MqsDefinitions.TestResult>();

                        foreach (MqsDefinitions.TestResult testResultInfo in UnitInfoResults[nProcIDX].testResultsList)
                        {
                            if (testResultInfo.TestCode.Equals(strTestCode))
                                ProcessResults.Add(testResultInfo);
                        }

                        if (ProcessResults.Count > 0)
                            result = ProcessResults.Last();

                        break;
                    }

                }
                else
                {
                    for (int nProcIDX = UnitInfoResults.Count - 1; nProcIDX >= 0; nProcIDX--)
                    {
                        if (!string.IsNullOrEmpty(strProcess) && !UnitInfoResults[nProcIDX].TestHeaders.Process.Equals(strProcess))
                            continue;

                        List<MqsDefinitions.TestResult> ProcessResults = new List<MqsDefinitions.TestResult>();

                        foreach (MqsDefinitions.TestResult testResultInfo in UnitInfoResults[nProcIDX].testResultsList)
                        {
                            if (testResultInfo.TestCode.Equals(strTestCode))
                                ProcessResults.Add(testResultInfo);
                        }

                        if (ProcessResults.Count > 0)
                            result = ProcessResults.Last();

                        break;
                    }

                }

            }
            catch (Exception error)
            {
                errorMessage = error.Message;
                LogError("Error: " + errorMessage);
            }

            return errorMessage;

        }

        public string LookupAllOcourrences(string strProcess, string strTestCode, out List<MqsDefinitions.TestResult> results, List<MqsDefinitions.TestProcess> UnitInfoResults = null)
        {
            string errorMessage = string.Empty;
            results = new List<MqsDefinitions.TestResult>();

            try
            {
                if (UnitInfoResults == null)
                {
                    if (m_webComponent == null)
                        throw new Exception("Component Web cannot be null");

                    if (m_webComponent.Tag == null)
                        throw new Exception("Web Infos cannot be null");

                    if (!((Dictionary<string, object>)m_webComponent.Tag).ContainsKey("ResultObject"))
                        throw new Exception("Web infos missing ResultObject Key");

                    UnitInfoResults = (List<MqsDefinitions.TestProcess>)((Dictionary<string, object>)m_webComponent.Tag)["ResultObject"];
                }

                for (int nProcIDX = 0; nProcIDX < UnitInfoResults.Count; nProcIDX++)
                {
                    if (!string.IsNullOrEmpty(strProcess) && !UnitInfoResults[nProcIDX].TestHeaders.Process.Equals(strProcess))
                        continue;

                    foreach (MqsDefinitions.TestResult testResultInfo in UnitInfoResults[nProcIDX].testResultsList)
                    {
                        if (testResultInfo.TestCode.Equals(strTestCode))
                            results.Add(testResultInfo);
                    }
                }


            }
            catch (Exception error)
            {
                errorMessage = error.Message;
                LogError("Error: " + errorMessage);
            }

            return errorMessage;

        }

        private void LogInfo(string strInfo)
        {
            if (bLogEnabled)
                LogDebuginfo.SetEquipmentDebugMessage(strInfo, DebugType);

        }

        private void LogError(string strInfo)
        {
            if (bLogEnabled)
                LogDebuginfo.SetEquipmentErrorMessage(strInfo, DebugType);

        }

        private string GetWebResult(string url, string urlTitle)
        {
            string errorMessage = string.Empty;

            try
            {
                if (m_webComponent == null)
                    throw new Exception("Component Web cannot be null");

                ((Dictionary<string, object>)m_webComponent.Tag)["URL_Title"] = urlTitle;

                m_webComponent.Navigate(url);

                do
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                } while ((bool)((Dictionary<string, object>)m_webComponent.Tag)["Navigated"] == false);

                if (!string.IsNullOrEmpty(((Dictionary<string, object>)m_webComponent.Tag)["NavigationError"].ToString()))
                    throw new Exception(((Dictionary<string, object>)m_webComponent.Tag)["NavigationError"].ToString());

                ((Dictionary<string, object>)m_webComponent.Tag)["RawResult"] = m_webComponent.DocumentText;


            }
            catch (Exception error)
            {
                errorMessage = error.Message;
                LogError("Error: " + errorMessage);
            }

            return errorMessage;

        }
    }
}