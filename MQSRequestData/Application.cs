using GlobalOperations.Definitions;
using MQSInterceptClass;
using MQSRequestDataYield;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MQSRequestData
{
    public partial class Application : Form
    {
        public Application()
        {
            InitializeComponent();
        }
        string url = "mqs.motorola.com";
        string home = "MQS Home";
        string erroMsg = string.Empty;

        private void buttonRun_Click(object sender, EventArgs e)
        {
            List<MqsDefinitions.TestProcess> UnitInfoResults;
            MQSGetYield mGY = new MQSGetYield();

           // MQSInterceptClassI dd = new MQSInterceptClassI();
          //  dd.StartBrowserWithLogin(url, home, "jagapatr", "L@ura2022.5");


            erroMsg = mGY.QueryYieldThread(url, out UnitInfoResults, home);
            if (erroMsg == string.Empty)
                labelStatus.Text = "Page OK!";
            else
                labelStatus.Text = "Page Error: " + erroMsg;
        }
    }
}
