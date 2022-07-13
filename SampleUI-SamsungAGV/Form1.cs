using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using GroupDocs.Parser.Data;
using GroupDocs.Parser.Options;
using GroupDocs.Parser;
using System.Net.Http;
using System.Threading;
using System.Resources;

namespace SampleUI_SamsungAGV
{
    public partial class Form1 : Form
    {
        System.Drawing.Point location = System.Drawing.Point.Empty;
        private System.Drawing.Point _mouseLoc;
        private static string m_exePath = string.Empty;
        public List<AGVCallingModel> AGVData = new List<AGVCallingModel>();
        public List<AGVCallingModel> AGV2Data = new List<AGVCallingModel>();
        public List<AGVStatusModel> AGVStatus = new List<AGVStatusModel>();
        public List<AGV2StatusModel> AGV2Status = new List<AGV2StatusModel>();
        public List<AGVErrorModel> AGVError = new List<AGVErrorModel>();
        public bool writeFlag = false, rt2 = false, endFlag = false, moveTimer = false;
        public int xAgv, yAgv, interval = 5, cntStop = 0, cntStop4 = 0, counterLog, updateError = 0;
        public int testButton = 0, agvAddress = 1, agv2Address = 2, waitingTime = 0, moveCnt = 0;
        public string agvState, agvName = "AGV-1", agvTime, agvStatus, agvRoute, agvRfid, statusDelivery, obsCode;
        private Color agvColor;
        public string agv2State, agv2Name = "AGV-2", agv2Time, agv2Status, agv2Route, agv2Rfid, statusDelivery2, obsCode2;
        public string AGVUniversalName;
        public string readType;
        public long jobId;
        public double offTime, offTime2;
        public string on1 = "   ON-1", on2 = "ON-2", off1 = "OFF-1", off2 = "OFF-2";
        public long btnState;
        public long btnState2;
        public long obsState;
        public long obsState2;
        public string errorCode, errorCode2, errorTime, errorTime2;

        //public string url = "http://localhost:8000/req";
        //public string url = "http://172.16.101.203:8000/req";
        public string url = "http://10.10.100.100:8000/req";
        //public string url = "http://192.168.77.220:8000/req";
        private double batScale(double value, double min, double max, double minScale, double maxScale)
        {
            double scaled = minScale + (double)(value - min) / (max - min) * (maxScale - minScale);
            return scaled;
        }

        private void pictureBox23_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void activeLine(bool l1, bool l2, bool l3, bool l4, bool l5, bool l6)
        {
            this.Invoke(new MethodInvoker(delegate () {
                send1.Visible = l1;
                send2.Visible = l2;
                send3.Visible = l3;
                send4.Visible = l4;
                send5.Visible = l5;
                send6.Visible = l6;
                standby1.Visible = !l1;
                standby2.Visible = !l2;
                standby3.Visible = !l3;
                standby4.Visible = !l4;
                standby5.Visible = !l5;
                standby6.Visible = !l6;
                //wip1.Visible = l1;
                //wip2.Visible = l2;
                //wip3.Visible = l3;
                //wip4.Visible = l4;
                //wip5.Visible = l5;
                //wip6.Visible = l6;
            }));

        }
        class RequestData
        {
            public string command { get; set; }
            public int serialNumber { get; set; }
        }
        class ResponseData
        {
            public string errMark { get; set; }
            public List<List<dynamic>> msg { get; set; }        // add command checker
            public string command { get; set; }
        }
        class ResponseData2
        {
            public string errMark { get; set; }
            public List<dynamic> msg { get; set; }               
            public string command { get; set; }
        }
        public class AGVDeviceModel
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
            public AGVDeviceModel(string agvId, string agvName,string status)
            {
                this.ID = agvId;
                this.Name = agvName;
                this.Status=status;
            }
        }
        public class AGV2DeviceModel
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
            public AGV2DeviceModel(string agvId, string agvName, string status)
            {
                this.ID = agvId;
                this.Name = agvName;
                this.Status = status;
            }
        }
        public class AGVStatusModel
        {
            //public string ID { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
            public string Status { get; set; }
            public AGVStatusModel(string agvName,string power, string status)
            {
                //this.ID = agvId;
                this.Name = agvName;
                this.State = power;
                this.Status = status;
            }
        }
        public class AGVCallingModel
        {
            public string Time { get; set; }
            public string Name { get; set; }
            public string Station { get; set; }
            public string Status { get; set; }
            public AGVCallingModel(string time,string agvname, string deliv, string status )
            {
                this.Time = time;
                this.Name = agvname;
                this.Station = deliv;
                this.Status = status;
            }
        }
        public class AGVErrorModel
        {
            public string Time { get; set; }
            public string Name { get; set; }
            public string Error { get; set; }
            public string Obstacle { get; set; }
            public AGVErrorModel(string dateTimeNow, string agvName, string errorCode, string obscode)
            {
                this.Time = dateTimeNow;
                this.Name = agvName;
                this.Error = errorCode;
                this.Obstacle = obscode;
            }
        }

        public class AGV2StatusModel
        {
            //public string ID { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
            public string Status { get; set; }
            public AGV2StatusModel(string agvName, string power, string status)
            {
                //this.ID = agvId;
                this.Name = agvName;
                this.State = power;
                this.Status = status;
            }
        }
        public class AGV2CallingModel
        {
            public string Time { get; set; }
            public string Name { get; set; }
            public string Station { get; set; }
            public string Status { get; set; }
            public AGV2CallingModel(string time, string agvname, string deliv, string status)
            {
                this.Time = time;
                this.Name = agvname;
                this.Station = deliv;
                this.Status = status;
            }
        }
        public class AGV2ErrorModel
        {
            public string Time { get; set; }
            public string Name { get; set; }
            public string Error { get; set; }
            public string Obstacle { get; set; }
            public AGV2ErrorModel(string dateTimeNow, string agvName, string errorCode, string obscode)
            {
                this.Time = dateTimeNow;
                this.Name = agvName;
                this.Error = errorCode;
                this.Obstacle = obscode;
            }
        }

        private async void callAPI()
        {
            //Console.WriteLine("\t Start Call API");
            string[] arrayMachine1 = new string[] { "SMD_01", "SMD_02", "SMD_03", "SMD_04", "SMD_05", "SMD_06" };
            string[] arrayMachine2 = new string[] { "SMD_07", "SMD_08", "SMD_09", "SMD_10", "SMD_11", "SMD_12" };

            string[] arrayPosition = new string[] {"HOME2","HOME1", "TRAFFIC2", "TRAFFIC1","BRANCH2", "BRANCH1","WIPFULL2", "WIPFULL1", // --- 8rfid
                                                   "WIP-IN-6", "WIP-IN-5","WIP-IN-4","WIP-IN-3","WIP-IN-2","WIP-IN-1","BRANCHLINE", // --- 7rfid
                                                   "RELEASE1", "STRAIGHTLINE1","TRAFFICHOME1","STRAIGHTLINE1", "STRAIGHTLINE1", "STRAIGHTLINE1", "STRAIGHTLINE1","STRAIGHTLINE1","STRAIGHTLINE1","STRAIGHTLINE1", // --- 10rfid
                                                   "SMD01","SMD01","SMD02","SMD02","SMD03","SMD03","SMD04","SMD04","SMD05","SMD05","SMD06","SMD06", "ENDLINE", // --- 13rfid
                                                   "BRANCHHOME","RELEASE2","STRAIGHTLINE2","TRAFFICHOME2","STRAIGHTLINE2","STRAIGHTLINE2","STRAIGHTLINE2","STRAIGHTLINE2", // --- 8rfid
                                                   "SMD07","SMD07","SMD08","SMD08","SMD09","SMD09","SMD10","SMD10","SMD11","SMD11","SMD12","SMD12","ENDLINE"}; // --- 13rfid


            string[] arrayRFID = new string[] { "128","129","126","127","46","131","13","130",                                                      //HOME to WIP --- 8rfid
                                                "125","124","123","122","121","120","2",                                                            //WIP to LINE --- 7rfid
                                                "119","1","117","11","56","54","53","52","31","32",                                                 //to Line 1-6 --- 10rfid
                                                "33", "34", "15","16","37","38","39","40","41","42","43","44","8",                                  //Line 1-6 --- 13rfid
                                                "30","118","101","116","48","49","102","103",                                                       //to Line 7-12 --- 8rfid
                                                "104","105","106","107","108,","109","110","111","112","113","114","115" ,"8"};                     //Line 7-12 --- 13rfid

            string[] arrayRFIDHorizontal = new string[] { "128", "129", "126", "127", "119", "1", "117", "11", "56", "54", "53", "52", "31", "32", "30", "118", "101", "116", "48", "49", "102"};
            string[] arrayRFIDVertical = new string[] { "46", "131", "13", "130", "33", "34", "15", "16", "37", "38", "39", "40", "41", "42", "43", "44", "8", "104", "105", "106", "107", "108,", "109", "110", "111", "112", "113", "114", "115", "8" };

            int[] arrayRfidLoc_X = new int[] {rfid128.Location.X,rfid129.Location.X,rfid126.Location.X,rfid127.Location.X,rfid46.Location.X,rfid131.Location.X,rfid13.Location.X,rfid130.Location.X,
                                              rfid125.Location.X,rfid124.Location.X,rfid123.Location.X,rfid122.Location.X,rfid121.Location.X,rfid120.Location.X,rfid2.Location.X,
                                              rfid119.Location.X,rfid1.Location.X,rfid117.Location.X,rfid11.Location.X,rfid56.Location.X,rfid54.Location.X,rfid53.Location.X,rfid52.Location.X,rfid31.Location.X,rfid32.Location.X,
                                              rfid33.Location.X,rfid34.Location.X,rfid15.Location.X,rfid16.Location.X,rfid37.Location.X,rfid38.Location.X,rfid39.Location.X,rfid40.Location.X,rfid41.Location.X,rfid42.Location.X,rfid43.Location.X,rfid44.Location.X,rfid8.Location.X,
                                              rfid30.Location.X,rfid118.Location.X,rfid101.Location.X,rfid116.Location.X,rfid48.Location.X,rfid49.Location.X,rfid102.Location.X,rfid103.Location.X,
                                              rfid104.Location.X,rfid105.Location.X,rfid106.Location.X,rfid107.Location.X,rfid108.Location.X,rfid109.Location.X,rfid110.Location.X,rfid111.Location.X,rfid112.Location.X,rfid113.Location.X,rfid114.Location.X,rfid115.Location.X,rfid8.Location.X};

            int[] arrayRfidLoc_Y = new int[] {rfid128.Location.Y,rfid129.Location.Y,rfid126.Location.Y,rfid127.Location.Y,rfid46.Location.Y,rfid131.Location.Y,rfid13.Location.Y,rfid130.Location.Y,
                                              rfid125.Location.Y,rfid124.Location.Y,rfid123.Location.Y,rfid122.Location.Y,rfid121.Location.Y,rfid120.Location.Y,rfid2.Location.Y,
                                              rfid119.Location.Y,rfid1.Location.Y,rfid117.Location.Y,rfid11.Location.Y,rfid56.Location.Y,rfid54.Location.Y,rfid53.Location.Y,rfid52.Location.Y,rfid31.Location.Y,rfid32.Location.Y,
                                              rfid33.Location.Y,rfid34.Location.Y,rfid15.Location.Y,rfid16.Location.Y,rfid37.Location.Y,rfid38.Location.Y,rfid39.Location.Y,rfid40.Location.Y,rfid41.Location.Y,rfid42.Location.Y,rfid43.Location.Y,rfid44.Location.Y,rfid8.Location.Y,
                                              rfid30.Location.Y,rfid118.Location.Y,rfid101.Location.Y,rfid116.Location.Y,rfid48.Location.Y,rfid49.Location.Y,rfid102.Location.Y,rfid103.Location.Y,
                                              rfid104.Location.Y,rfid105.Location.Y,rfid106.Location.Y,rfid107.Location.Y,rfid108.Location.Y,rfid109.Location.Y,rfid110.Location.Y,rfid111.Location.Y,rfid112.Location.Y,rfid113.Location.Y,rfid114.Location.Y,rfid115.Location.Y,rfid8.Location.Y};

            //==================================================================== PHASE 2 ==============================================================// --> 

            //==================================================================================================================================// --> API1
            ResponseData data = await API("missionC.missionGetActiveList()");
            if (data.errMark == "ok")
            {
                List<AGVCallingModel> showData = new List<AGVCallingModel>();
                List<AGV2CallingModel> showData2 = new List<AGV2CallingModel>();
                string lastTIme = "";
                for (int i = 0; i < data.msg.Count; i++)
                {
                    counterLog += 1;
                    agvTime = UnixTimeStampToDateTime(data.msg[i][11]).ToString();
                    statusDelivery = data.msg[i][10];
                    jobId = data.msg[i][0];
                    lastTIme = agvTime;
                    string SMDdata = data.msg[i][1].ToString();
                    bool AGV1Deliver = arrayMachine1.Contains(SMDdata);
                    bool AGV2Deliver = arrayMachine2.Contains(SMDdata);
                    //string text = System.IO.File.ReadAllText(@"D:\WORK\SAMSUNG\SampleUI-SamsungAGV\log.txt");
                    //string[] lines = System.IO.File.ReadAllLines(@"D:\WORK\SAMSUNG\SampleUI-SamsungAGV\log.txt");

                    if (AGV1Deliver == true)
                    {
                        AGVUniversalName = agvName;
                    }
                    else if (AGV2Deliver == true)
                    {
                        AGVUniversalName = agv2Name;
                    }

                    if (counterLog < 11)
                    {
                        if (statusDelivery == "执行") { statusDelivery = "RUNNING";}
                        else if (statusDelivery == "放弃") { statusDelivery = "HOLD";}
                        else if (statusDelivery == "正常结束") { statusDelivery = "FINISH";}
                        else if (statusDelivery == "错误") 
                        { 
                            string disc = AGVUniversalName + " DISCONNECTED";
                            if (labelDisconnect.InvokeRequired)
                            {
                                labelDisconnect.Invoke(new Action(callAPI));
                                return;
                            }
                            labelDisconnect.Text = disc;
                            labelDisconnect.Visible = true;
                        }
                        else 
                        { 
                            Console.WriteLine("Status Delivery : {0}", statusDelivery); 
                            labelDisconnect.Visible = false;
                        }

                        //Console.WriteLine("{0} {1} {2} {3} {4} cnt : {5} {6} lastTime : {7}", agvTime, agvName, jobId, statusDelivery, data.msg.Count,
                        //counterLog, writeFlag, lastTIme);
                        //System.Console.WriteLine("Contents of WriteText.txt = \n{0}", text);
                        //foreach (string line in lines)
                        //{
                        //    // Use a tab to indent each line of the file.
                        //    //Console.WriteLine("\n" + line);
                        //}
                        //Console.WriteLine(lines[0]);

                        //// Display the file contents by using a foreach loop.
                        //System.Console.WriteLine("Contents of WriteLines2.txt = \n
                    }
                    else { //Console.WriteLine("API 1 Else ");
                    }

                    if (statusDelivery == "执行")
                    {
                        statusDelivery = "RUNNING";
                        jobId = data.msg[i][0];
                        long[] arrayJobId = new long[] {data.msg[0][0], data.msg[1][0], data.msg[2][0], data.msg[3][0], data.msg[4][0],
                                            data.msg[5][0], data.msg[6][0], data.msg[7][0], data.msg[8][0],data.msg[9][0] };

                        long maxJobid = arrayJobId.Last(), searchJobid = jobId;
                        long indexJob = Array.IndexOf(arrayJobId, searchJobid);

                        //string searchString = data.msg[(int)indexJob][1];               //Read first call jobID then search Name
                        //int index = Array.IndexOf(arrayMachine, searchString);
                        AGVCallingModel temp = new AGVCallingModel(agvTime, AGVUniversalName, data.msg[i][1].ToString(), statusDelivery);
                        showData.Add(temp);
                        //if (index == 0) { activeLine(true, false, false, false, false, false);}
                        //else if (index == 1){ activeLine(false, true, false, false, false, false);}
                        //else if (index == 2){ activeLine(false, false, true, false, false, false);}
                        //else if (index == 3){ activeLine(false, false, false, true, false, false);}
                        //else if (index == 4){ activeLine(false, false, false, false, true, false);}
                        //else if (index == 5){ activeLine(false, false, false, false, false, true);}
                        //else { activeLine(false, false, false, false, false, false);}
                    }
                    else if (statusDelivery == "放弃")
                    {
                        statusDelivery = "HOLD";
                        AGVCallingModel temp = new AGVCallingModel(agvTime, AGVUniversalName, SMDdata, statusDelivery);
                        //showData.Add(temp);
                    }
                    else if (statusDelivery == "正常结束")
                    {
                        statusDelivery = "FINISH";
                        AGVCallingModel temp = new AGVCallingModel(agvTime, AGVUniversalName, SMDdata, statusDelivery);
                        showData.Add(temp);
                    }
                    else if (statusDelivery == "错误")
                    {
                        statusDelivery = "COM ERR";
                        AGVCallingModel temp = new AGVCallingModel(agvTime, AGVUniversalName, data.msg[i][1].ToString(), statusDelivery);
                        showData.Add(temp);
                    }
                    else { activeLine(false, false, false, false, false, false);}
                }
                
               
                gridViewDS.Invoke((MethodInvoker)delegate { 
                    gridViewDS.DataSource = showData;
                    this.gridViewDS.Columns[0].Width = 150;
                    this.gridViewDS.Columns[1].Width = 60;
                    this.gridViewDS.Columns[2].Width = 60;
                });

            }

            //==================================================================================================================================// --> API2
            data = await API("devC.getCarList()");

            if (data.errMark == "OK")
            {
                List<AGVStatusModel> showData = new List<AGVStatusModel>();


                for (int i = 0; i < data.msg.Count; i++)
                {
                    double power = data.msg[i][7];

                    //"车" Read RFID and detail Car activity
                    double dataMovement = data.msg[i][15], dataRute = data.msg[i][31], dataRfid = data.msg[i][33], readAddress = data.msg[i][0];
                    readType = data.msg[i][2];
                    string searchRFID = dataRfid.ToString();

                    agvRoute = dataRute.ToString();

                    int agvRfid = (int)dataRfid;

                    if (readAddress == 1 && readType == "车" && i == 0)
                    {
                        if (dataMovement == 0) { agvStatus = "STOP"; agvColor = Color.Yellow; }
                        else if (dataMovement == 1) { agvStatus = "PAUSE"; agvColor = Color.Yellow; }
                        else if (dataMovement == 2) { agvStatus = "RUN"; agvColor = Color.Lime; }
                        else { }

                        agv1Vertical.BackColor = agvColor;
                        agv1Horizontal.BackColor = agvColor;
                        AGV1StatusLabel.Text = agvStatus;

                        batteryLevel1.Value = (int)power;

                        if (batValue1.InvokeRequired)
                        {
                            batValue1.Invoke(new Action(callAPI));
                            return;
                        }
                        batValue1.Text = power.ToString();

                        int indexRFID1 = Array.IndexOf(arrayRFID, searchRFID);

                        bool horizontalTarget = arrayRFIDHorizontal.Contains(searchRFID);
                        bool verticalTarget = arrayRFIDVertical.Contains(searchRFID);

                        //Clean Code for Image Positioning
                        //if(horizontalTarget == true)
                        //{
                        //    agv1Horizontal.Left = arrayRfidLoc_X[agvRfid];
                        //    agv1Horizontal.Top = arrayRfidLoc_Y[agvRfid];
                        //    agv1Horizontal.Visible = true;
                        //}
                        //else if(verticalTarget == true)
                        //{
                        //  agv1Vertical.Left = arrayRfidLoc_X[agvRfid];
                        //   agv1Vertical.Top = arrayRfidLoc_Y[agvRfid];
                        //   agv1Vertical.Visible = true;
                        //}
                        //else
                        //{
                        //    agv1Vertical.Visible = false;
                        //    agv1Horizontal.Visible = false;
                        //}
                        //Clean Code for Image Positioning Ends Here

                        //Image Positioning
                        if (dataRfid == 129)
                        {
                            agv1Horizontal.Left = rfid129.Left;
                            agv1Horizontal.Top = rfid129.Top;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 127)
                        {
                            agv1Horizontal.Left = rfid127.Left;
                            agv1Horizontal.Top = rfid127.Top;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 131)
                        {
                            agv1Vertical.Top = rfid131.Top;
                            agv1Vertical.Left = rfid131.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 13)
                        {
                            agv1Vertical.Top = rfid13.Top;
                            agv1Vertical.Left = rfid13.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 130)
                        {
                            agv1Vertical.Top = rfid130.Top;
                            agv1Vertical.Left = rfid130.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 125)
                        {
                            agv1Vertical.Top = rfid125.Top;
                            agv1Vertical.Left = rfid125.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 124)
                        {
                            agv1Vertical.Top = rfid124.Top;
                            agv1Vertical.Left = rfid124.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 123)
                        {
                            agv1Vertical.Top = rfid123.Top;
                            agv1Vertical.Left = rfid123.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 122)
                        {
                            agv1Vertical.Top = rfid122.Top;
                            agv1Vertical.Left = rfid122.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 121)
                        {
                            agv1Vertical.Top = rfid121.Top;
                            agv1Vertical.Left = rfid121.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 120)
                        {
                            agv1Vertical.Top = rfid120.Top;
                            agv1Vertical.Left = rfid120.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                            if (dataRute == 101)
                            {
                                wip1.Visible = true;
                            }
                            else
                            {
                                wip1.Visible = false;
                            }
                        }
                        else if (dataRfid == 2)
                        {
                            agv1Vertical.Top = rfid2.Top;
                            agv1Vertical.Left = rfid2.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 119)
                        {
                            agv1Horizontal.Top = rfid119.Top;
                            agv1Horizontal.Left = rfid119.Left;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 119)
                        {
                            agv1Horizontal.Top = rfid119.Top;
                            agv1Horizontal.Left = rfid119.Left;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 1)
                        {
                            agv1Horizontal.Top = rfid1.Top;
                            agv1Horizontal.Left = rfid1.Left;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 117)
                        {
                            agv1Horizontal.Top = rfid117.Top;
                            agv1Horizontal.Left = rfid117.Left;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 11)
                        {
                            agv1Horizontal.Top = rfid11.Top;
                            agv1Horizontal.Left = rfid11.Left;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 56)
                        {
                            agv1Horizontal.Top = rfid56.Top;
                            agv1Horizontal.Left = rfid56.Left;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 54)
                        {
                            agv1Horizontal.Top = rfid54.Top;
                            agv1Horizontal.Left = rfid54.Left;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 53)
                        {
                            agv1Horizontal.Top = rfid53.Top;
                            agv1Horizontal.Left = rfid53.Left;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 52)
                        {
                            agv1Horizontal.Top = rfid52.Top;
                            agv1Horizontal.Left = rfid52.Left;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 31)
                        {
                            agv1Horizontal.Top = rfid31.Top;
                            agv1Horizontal.Left = rfid31.Left;
                            agv1Horizontal.Visible = true;
                            agv1Vertical.Visible = false;
                        }
                        else if (dataRfid == 32)
                        {
                            agv1Vertical.Top = rfid32.Top;
                            agv1Vertical.Left = rfid32.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 33)
                        {
                             agv1Vertical.Top = rfid33.Top;
                             agv1Vertical.Left = rfid33.Left;
                             agv1Vertical.Visible = true;
                             agv1Horizontal.Visible = false;
                         }
                        else if (dataRfid == 34)
                        {
                            agv1Vertical.Top = rfid34.Top;
                            agv1Vertical.Left = rfid34.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 15)
                        {
                             agv1Vertical.Top = rfid15.Top;
                             agv1Vertical.Left = rfid15.Left;
                             agv1Vertical.Visible = true;
                             agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 16)
                        {
                            agv1Vertical.Top = rfid16.Top;
                            agv1Vertical.Left = rfid16.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 37)
                        {
                             agv1Vertical.Top = rfid37.Top;
                             agv1Vertical.Left = rfid37.Left;
                             agv1Vertical.Visible = true;
                             agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 38)
                        {
                            agv1Vertical.Top = rfid38.Top;
                            agv1Vertical.Left = rfid38.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 39)
                        {
                             agv1Vertical.Top = rfid39.Top;
                             agv1Vertical.Left = rfid39.Left;
                             agv1Vertical.Visible = true;
                             agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 40)
                        {
                            agv1Vertical.Top = rfid40.Top;
                            agv1Vertical.Left = rfid40.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 41)
                        {
                             agv1Vertical.Top = rfid41.Top;
                             agv1Vertical.Left = rfid41.Left;
                             agv1Vertical.Visible = true;
                             agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 42)
                        {
                            agv1Vertical.Top = rfid42.Top;
                            agv1Vertical.Left = rfid42.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 43)
                        {
                             agv1Vertical.Top = rfid43.Top;
                             agv1Vertical.Left = rfid43.Left;
                             agv1Vertical.Visible = true;
                             agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 44)
                        {
                            agv1Vertical.Top = rfid44.Top;
                            agv1Vertical.Left = rfid44.Left;
                            agv1Vertical.Visible = true;
                            agv1Horizontal.Visible = false;
                        }
                        else if (dataRfid == 8)
                        {
                             agv1Vertical.Top = rfid8.Top;
                             agv1Vertical.Left = rfid8.Left;
                             agv1Vertical.Visible = true;
                             agv1Horizontal.Visible = false;
                        }
                        //Image Positioning ends here

                        //Rute WIP 
                        //if (dataRute == 101 && dataRfid == 120 )
                        //{
                        //    wip1.Visible = true;
                        //}
                        //else if (dataRute == 105 && dataRfid == 121)
                        //{
                        //    wip2.Visible = true;
                        //}
                        //else if (dataRute == 106 && dataRfid == 122)
                        //{
                        //    wip3.Visible = true;
                        //}
                        //else if (dataRute == 107 && dataRfid == 123)
                        //{
                        //    wip4.Visible = true;
                        //}
                        //else if (dataRute == 108 && dataRfid == 124)
                        //{
                        //    wip5.Visible = true;
                        //}
                        //else if (dataRute == 109 && dataRfid == 125)
                        //{
                        //    wip6.Visible = true;
                        //}
                        //Rute WIP ends here

                        //Rute MC
                        if (dataRute == 1 && dataRfid == 34)
                        {
                            send1.Visible = true;
                            standby1.Visible = false;
                        }
                        else if (dataRute == 3 && dataRfid == 16)
                        {
                            send2.Visible = true;
                            standby2.Visible = false;
                        }
                        else if (dataRute == 4 && dataRfid == 38)
                        {
                            send3.Visible = true;
                            standby3.Visible = false;
                        }
                        else if (dataRute == 5 && dataRfid == 40)
                        {
                            send4.Visible = true;
                            standby4.Visible = false;
                        }
                        else if (dataRute == 6 && dataRfid == 42)
                        {
                            send5.Visible = true;
                            standby5.Visible = false;
                        }
                        else if (dataRute == 7 && dataRfid == 44)
                        {
                            send6.Visible = true;
                            standby6.Visible = false;
                        }

                        // Transfer WIP FUll 1
                        //else if ((dataRute == 20) && (dataRfid == 130 || dataRfid == 13))
                        //{
                        //    wipFull1.Visible = true;

                        //}
                        else if (dataRute == 20 || dataRute == 30)
                        {
                            //agvRoute = "GO HOME";
                            if ((dataRfid == 128 || dataRfid == 129 ) && statusDelivery == "FINISH")
                            {
                                activeLine(false, false, false, false, false, false);

                            }
                            else if (dataRfid == 9)
                            {
                                //agv1Vertical.Visible = false;
                            }
                            wipFull1.Visible = false;
                            wipFull2.Visible = false;
                        }
                        else
                        {
                            agvRoute = "STANDBY";

                        }

                        // Visualization
                        if (dataRfid == 1 && dataRute != 20)
                        {

                        }
                        else if (dataRfid == 11 && dataRute != 20)
                        {
                            //agv1Vertical.Visible = false;
                        }
                        else if ((dataRfid == 31 || dataRfid ==52 || dataRfid == 53 || dataRfid == 54 || dataRfid == 56) && dataRute != 20)
                        {

                        }
                        else if (dataRfid == 32 && dataRute != 20)
                        {

                        }
                        else if (dataRfid == 32)
                        {

                        }
                        else if (dataRfid == 31 && dataRute == 20)
                        {

                            if (dataRfid == 11)
                            {

                            }
                            else 
                            { 
                                //agvFlip4.Visible = true; agvFlip3.Visible = false;
                            }
                        }
                        else if (dataRfid == 11 && dataRute == 20)
                        {

                        }
                        else if (dataRfid == 1 && dataRute == 20)
                        {

                        }
                        else if (dataRfid == 2 && dataRute == 20)
                        {

                        }
                        else if (dataRfid == 5)
                        {
                            //agvLabel1.Visible = true;
                        }
                        else if (dataRute == 1)
                        {


                        }
                        else if (dataRfid == 0)
                        {

                        }
                        else
                        {

                        }
           
                    }

                    else if (readAddress == 2 && readType == "车" && i == 1)
                    {
                        if (dataMovement == 0) { agv2Status = "STOP"; agvColor = Color.Yellow; }
                        else if (dataMovement == 1) { agv2Status = "PAUSE"; agvColor = Color.Yellow; }
                        else if (dataMovement == 2) { agv2Status = "RUN"; agvColor = Color.Lime; }
                        else { }

                        agv2Vertical.BackColor = agvColor;
                        agv2Horizontal.BackColor = agvColor;
                        AGV2StatusLabel.Text = agv2Status;
                        batteryLevel2.Value = (int)power;

                        if (batValue2.InvokeRequired)
                        {
                            batValue2.Invoke(new Action(callAPI));
                            return;
                        }
                        batValue2.Text = power.ToString();

                        //Image Positioning
                        if (dataRfid == 128)
                        {
                            agv2Horizontal.Left = rfid128.Left;
                            agv2Horizontal.Top = rfid128.Top;
                            agv2Horizontal.Visible = true;
                            agv2Vertical.Visible = false;
                            
                        }
                        else if (dataRfid == 126)
                        {
                            agv2Horizontal.Left = rfid126.Left;
                            agv2Horizontal.Top = rfid126.Top;
                            agv2Horizontal.Visible = true;
                            agv2Vertical.Visible = false;
                        }
                        else if (dataRfid == 46)
                        {
                            agv2Vertical.Top = rfid46.Top;
                            agv2Vertical.Left = rfid46.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 131)
                        {
                            agv2Vertical.Top = rfid131.Top;
                            agv2Vertical.Left = rfid131.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                            wipFull2.Visible = false;
                        }
                        else if (dataRfid == 13)
                        {
                            agv2Vertical.Top = rfid13.Top;
                            agv2Vertical.Left = rfid13.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                            if (dataRute == 114)
                            {
                                wipFull2.Visible = true;
                            }
                            else
                            {
                                wipFull2.Visible = false;
                            }
                            
                        }
                        else if (dataRfid == 130)
                        {
                            agv2Vertical.Top = rfid130.Top;
                            agv2Vertical.Left = rfid130.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 125)
                        {
                            agv2Vertical.Top = rfid125.Top;
                            agv2Vertical.Left = rfid125.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 124)
                        {
                            agv2Vertical.Top = rfid124.Top;
                            agv2Vertical.Left = rfid124.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 123)
                        {
                            agv2Vertical.Top = rfid123.Top;
                            agv2Vertical.Left = rfid123.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 122)
                        {
                            agv2Vertical.Top = rfid122.Top;
                            agv2Vertical.Left = rfid122.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 121)
                        {
                            agv2Vertical.Top = rfid121.Top;
                            agv2Vertical.Left = rfid121.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                            if (dataRute == 115)
                            {
                                wip8.Visible = true;
                            }
                            else
                            {
                                wip8.Visible = false;
                            }
                        }
                        else if (dataRfid == 120)
                        {
                            agv2Vertical.Top = rfid120.Top;
                            agv2Vertical.Left = rfid120.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                            wip8.Visible = false;
                        }
                        else if (dataRfid == 2)
                        {
                            agv2Vertical.Top = rfid2.Top;
                            agv2Vertical.Left = rfid2.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 30)
                        {
                            agv2Horizontal.Top = rfid30.Top;
                            agv2Horizontal.Left = rfid30.Left;
                            agv2Horizontal.Visible = true;
                            agv2Vertical.Visible = false;
                        }
                        else if (dataRfid == 118)
                        {
                            agv2Horizontal.Top = rfid118.Top;
                            agv2Horizontal.Left = rfid118.Left;
                            agv2Horizontal.Visible = true;
                            agv2Vertical.Visible = false;
                        }
                        else if (dataRfid == 101)
                        {
                            agv2Horizontal.Top = rfid101.Top;
                            agv2Horizontal.Left = rfid101.Left;
                            agv2Horizontal.Visible = true;
                            agv2Vertical.Visible = false;
                        }
                        else if (dataRfid == 116)
                        {
                            agv2Horizontal.Top = rfid116.Top;
                            agv2Horizontal.Left = rfid116.Left;
                            agv2Horizontal.Visible = true;
                            agv2Vertical.Visible = false;
                        }
                        else if (dataRfid == 48)
                        {
                            agv2Horizontal.Top = rfid48.Top;
                            agv2Horizontal.Left = rfid48.Left;
                            agv2Horizontal.Visible = true;
                            agv2Vertical.Visible = false;
                        }
                        else if (dataRfid == 49)
                        {
                            agv2Horizontal.Top = rfid49.Top;
                            agv2Horizontal.Left = rfid49.Left;
                            agv2Horizontal.Visible = true;
                            agv2Vertical.Visible = false;
                        }
                        else if (dataRfid == 102)
                        {
                            agv2Horizontal.Top = rfid102.Top;
                            agv2Horizontal.Left = rfid102.Left;
                            agv2Horizontal.Visible = true;
                            agv2Vertical.Visible = false;
                        }
                        else if (dataRfid == 103)
                        {
                            agv2Vertical.Top = rfid103.Top;
                            agv2Vertical.Left = rfid103.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 104)
                        {
                            agv2Vertical.Top = rfid104.Top;
                            agv2Vertical.Left = rfid104.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 105)
                        {
                            agv2Vertical.Top = rfid105.Top;
                            agv2Vertical.Left = rfid105.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 106)
                        {
                            agv2Vertical.Top = rfid106.Top;
                            agv2Vertical.Left = rfid106.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                            standby8.Visible = true;
                        }
                        else if (dataRfid == 107)
                        {
                            agv2Vertical.Top = rfid107.Top;
                            agv2Vertical.Left = rfid107.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                            standby8.Visible = false;
                        }
                        else if (dataRfid == 108)
                        {
                            agv2Vertical.Top = rfid108.Top;
                            agv2Vertical.Left = rfid108.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 109)
                        {
                            agv2Vertical.Top = rfid109.Top;
                            agv2Vertical.Left = rfid109.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 110)
                        {
                            agv2Vertical.Top = rfid110.Top;
                            agv2Vertical.Left = rfid110.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 111)
                        {
                            agv2Vertical.Top = rfid111.Top;
                            agv2Vertical.Left = rfid111.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 112)
                        {
                            agv2Vertical.Top = rfid112.Top;
                            agv2Vertical.Left = rfid112.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 113)
                        {
                            agv2Vertical.Top = rfid113.Top;
                            agv2Vertical.Left = rfid113.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 114)
                        {
                            agv2Vertical.Top = rfid114.Top;
                            agv2Vertical.Left = rfid114.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 115)
                        {
                            agv2Vertical.Top = rfid115.Top;
                            agv2Vertical.Left = rfid115.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        else if (dataRfid == 8)
                        {
                            agv2Vertical.Top = rfid8.Top;
                            agv2Vertical.Left = rfid8.Left;
                            agv2Vertical.Visible = true;
                            agv2Horizontal.Visible = false;
                        }
                        //Image Positioning ends here
                    }

                }
                //gridViewStatus.Invoke((MethodInvoker)delegate { gridViewStatus.DataSource = showData; });
            }

            //==================================================================================================================================// --> API3
            data = await API("devC.getDeviceList()");
            //Console.WriteLine("devC.getDeviceList()");
            if (data.errMark == "OK")
            {
                double agvAddress,agvAddress2;
                List<AGVDeviceModel> showDevice = new List<AGVDeviceModel>();
                List<AGV2DeviceModel> showDevice2 = new List<AGV2DeviceModel>();


                for (int i = 0; i < data.msg.Count; i++)
                {
                    agvAddress = data.msg[0][3]; agvAddress2 = data.msg[1][3];
                    offTime = data.msg[0][6];
                    offTime2 = data.msg[1][6];

                    if ((agvAddress == 1) && readType == "车" && offTime >= 10)
                    {
                        agvState = "OFF";
                        string disc = "AGV-1" + " DISCONNECTED";
                        AGV1NameLabel.Text = agvName;
                        Console.WriteLine(disc);
                        AGV1StateLabel.Text = agvState;
                        labelDisconnect.Text = disc;
                        labelDisconnect.Visible = true;
                    }
                    else if (agvAddress == 1 && readType == "车" && offTime < 1)
                    {
                        
                        agvState = "ON";
                        AGV1NameLabel.Text = agvName;
                        AGV1StateLabel.Text = "ON";
                        labelDisconnect.Visible = false;
                    }
                    else
                    {
                        agvState = "OFF";
                        AGV1NameLabel.Text = agvName;
                        string disc = "AGV-1" + " DISCONNECTED";
                        Console.WriteLine(disc);
                        AGV1StateLabel.Text = agvState;
                        labelDisconnect.Text = disc;
                        labelDisconnect.Visible = true;
                    }
                    if ((agvAddress2 == 2) && readType == "车" && offTime2 >= 10)
                    {
                        agv2State = "OFF";
                        string disc = "AGV-2" + " DISCONNECTED";
                        AGV2NameLabel.Text = agv2Name;
                        Console.WriteLine(disc);
                        AGV2StateLabel.Text = agv2State;
                        labelDisconnect.Text = disc;
                        labelDisconnect.Visible = true;
                    }
                    else if (agvAddress2 == 2 && readType == "车" && offTime2 < 0.3)
                    {
                        agv2State = "ON";
                        AGV2NameLabel.Text = agv2Name;
                        AGV2StateLabel.Text = "ON";
                        //labelDisconnect.Visible = false;
                    }
                    else {
                        agv2State = "OFF";
                        string disc = "AGV-2" + " DISCONNECTED";
                        AGV2NameLabel.Text = agv2Name;
                        Console.WriteLine(disc);
                        AGV2StateLabel.Text = agv2State;
                        labelDisconnect.Text = disc;
                        labelDisconnect.Visible = true;
                    }
                    
                }

                List<AGVErrorModel> showError = new List<AGVErrorModel>();
                errorTime = DateTime.Now.ToString();
                errorTime2 = DateTime.Now.ToString();
                ResponseData2 datanonArray = await APInonArray("devC.deviceDic[1].optionsLoader.load(carLib.RAM.DEV.BTN_EMC)");
                ResponseData2 datanonArray2 = await APInonArray("devC.deviceDic[2].optionsLoader.load(carLib.RAM.DEV.BTN_EMC)");


                try
                {
                    btnState = datanonArray.msg[1];
                }
                catch(NullReferenceException)
                {
                    btnState = 1;
                }

                try
                {
                    btnState2 = datanonArray2.msg[1];
                }
                catch (NullReferenceException)
                {
                    btnState2 = 1;
                }
                //Console.WriteLine("Kondisi 1 adalah: {0}", btnState);
                //Console.WriteLine("Kondisi 2 adalah: {0}", btnState2);

                if (datanonArray.errMark == "OK")
                {
                    errorTime = DateTime.Now.ToString();
                    if (btnState == 0) 
                    { 
                        errorCode = "EMC STOP";
                        agv1Horizontal.BackColor = Color.Red;
                        agv1Vertical.BackColor = Color.Red;
                    }
                    else if (btnState == 1)
                    { 
                        errorCode = "-";
                        agv1Vertical.BackColor = agvColor;
                        agv1Horizontal.BackColor = agvColor;
                    }
                }
                else
                {
                    errorTime = "";
                    errorCode = "";
                }

                if (datanonArray2.errMark == "OK")
                {
                    errorTime2 = DateTime.Now.ToString();
                    if (btnState2 == 0)
                    {
                        errorCode2 = "EMC STOP";
                        agv1Horizontal.BackColor = Color.Red;
                        agv1Vertical.BackColor = Color.Red;
                    }
                    else if (btnState2 == 1)
                    {
                        errorCode2 = "-";
                        agv1Vertical.BackColor = agvColor;
                        agv1Horizontal.BackColor = agvColor;
                    }
                }
                else
                {
                    errorTime2 = "";
                    errorCode2 = "";
                }
                //==================================================================================================================================// --> API5

                ResponseData2 nonArrayOBS = await APInonArray("devC.deviceDic[1].optionsLoader.load(carLib.RAM.DEV.OBS)");
                ResponseData2 nonArrayOBS2 = await APInonArray("devC.deviceDic[2].optionsLoader.load(carLib.RAM.DEV.OBS)");

                try
                {
                    obsState = nonArrayOBS.msg[2];
                }
                catch (NullReferenceException)
                {
                    obsState = 3;
                }

                try
                {
                    obsState2 = nonArrayOBS2.msg[2];
                }
                catch (NullReferenceException)
                {
                    obsState2 = 3;
                }

                if (nonArrayOBS.errMark == "OK")
                {

                    if (obsState == 7)
                    {
                        obsCode = "OBS STOP";
                        agv1Horizontal.BackColor = Color.Red;
                        agv1Vertical.BackColor = Color.Red;
                   }
                    else
                    { 
                        obsCode = "-";
                        agv1Vertical.BackColor = agvColor;
                        agv1Horizontal.BackColor = agvColor;
                   }
                }
                else
                {
                    errorTime = "";
                    obsCode = "";
                }

                if (nonArrayOBS2.errMark == "OK")
                {
                    if (obsState2 == 7)
                    {
                        obsCode2 = "OBS STOP";
                        agv1Horizontal.BackColor = Color.Red;
                        agv1Vertical.BackColor = Color.Red;
                   }
                    else
                    { 
                        obsCode2 = "-";
                        agv1Vertical.BackColor = agvColor;
                        agv1Horizontal.BackColor = agvColor;
                   }
                }
                else
                {
                    errorTime2 = "";
                    obsCode2 = "";
                }
                updateError = 0;

                Console.WriteLine("Kondisi OBS 1: {0}", obsState);
                Console.WriteLine("Kondisi OBS 2: {0}", obsState2);
                AGVErrorModel temp = new AGVErrorModel(errorTime, agvName, errorCode, obsCode);
                AGVErrorModel temp2 = new AGVErrorModel(errorTime2, agv2Name, errorCode2, obsCode2);
                showError.Add(temp);
                showError.Add(temp2);
                gridViewError.Invoke((MethodInvoker)delegate { gridViewError.DataSource = showError; });

            }
            else if (data.errMark=="err") { 
                agvState  = "OFF";
                agv2State = "OFF";
                string disc = agvName + " DISCONNECTED";
                //Console.WriteLine(disc);
                labelDisconnect.Text = disc;
                labelDisconnect.Visible = true;
            }
            callAPI();
        }
        
        private void timer5_Tick(object sender, EventArgs e)
        {
            waitingTime += 1;
        }

        [Obsolete]
        public Form1()
        {
            InitializeComponent();
            gridViewDS.Invoke((MethodInvoker)delegate { gridViewDS.DataSource = AGVData; });
            gridViewError.Invoke((MethodInvoker)delegate { gridViewError.DataSource = AGVError; });
            AutoClosingMessageBox.Show("Connecting to The server...","SYSTEM INFO", 10000);
            callAPI();
        }

        //=============================================================Backend Service=============================================================//
        private async Task<ResponseData> API(string command)
        {
            var cmd = new RequestData();
            cmd.command = command;
            cmd.serialNumber = 0;
            var json = JsonConvert.SerializeObject(cmd);
            //Console.WriteLine(json);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            var response = await client.PostAsync(url, data);
            ResponseData ret;
            //Console.Write("Response Data : {0}", data);
            while (true)
            {
                ret = JsonConvert.DeserializeObject<ResponseData>(response.Content.ReadAsStringAsync().Result);
                if (ret.command == cmd.command)
                {
                    break;
                }
            }
            return ret;
            //Console.WriteLine("Error Message: {0}", ret.errMark);
            //Console.WriteLine("Message: {0}", ret.msg);
            //Console.WriteLine("IP Robot: {0}", ret.msg[0][5][0]);
            //Console.WriteLine(ret);
            //string result = response.Content.ReadAsStringAsync().Result;
            ////Console.WriteLine(result);
        }
        private async Task<ResponseData2> APInonArray(string command)
        {
            var cmd = new RequestData();
            cmd.command = command;
            cmd.serialNumber = 0;

            var json = JsonConvert.SerializeObject(cmd);
            //Console.WriteLine(json);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            var response = await client.PostAsync(url, data);
            ResponseData2 ret;
            //Console.Write("Response Data : {0}", data);
            while (true)
            {
                ret = JsonConvert.DeserializeObject<ResponseData2>(response.Content.ReadAsStringAsync().Result);
                if (ret.command == cmd.command)
                {
                    break;
                }
            }
            return ret;
            
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.Location.X - _mouseLoc.X;
                int dy = e.Location.Y - _mouseLoc.Y;
                this.Location = new System.Drawing.Point(this.Location.X + dx, this.Location.Y + dy);
            }
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseLoc = e.Location;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            dateLabel.Text = DateTime.Now.ToString();
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            updateError += 1;
        }
        private void timer3_Tick(object sender, EventArgs e)
        {
            moveCnt += 1;

        }
        private void timer4_Tick(object sender, EventArgs e)
        {

        }
        private void detailError_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Text = "Detail Error History";
            form2.HelpButton = true;
            form2.FormBorderStyle = FormBorderStyle.FixedDialog;
            form2.StartPosition = FormStartPosition.CenterScreen;
            form2.ShowDialog();


        }
        private void detailDelivery_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.Text = "Detail Delivery Status";
            form3.HelpButton = true;
            form3.FormBorderStyle = FormBorderStyle.FixedDialog;
            form3.StartPosition = FormStartPosition.CenterScreen;
            form3.ShowDialog();
            //Console.Write("writelog");

        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;

        }
        public static void writeLog(string logTime, string logName, long logJobid, string logStatusDlv)
        {
            string logData = logTime + "," + logName + "," + logJobid + "," + logStatusDlv;
            try
            {
                File.AppendAllText(@"D:\WORK\SAMSUNG\SampleUI-SamsungAGV\log.txt", logData.ToString() + Environment.NewLine);
            }
            catch (Exception)
            {
                //Console.WriteLine("error catch : {0}", e.Message);
            }
        }
        private void closeButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void batteryLevel1_ProgressChanged(object sender, Bunifu.UI.WinForms.BunifuProgressBar.ProgressChangedEventArgs e)
        {

        }

        private void batteryLevel2_ProgressChanged(object sender, Bunifu.UI.WinForms.BunifuProgressBar.ProgressChangedEventArgs e)
        {

        }

        public static void disini(string textInput,string i2,string i3)
        {
            //Console.WriteLine("Lagi Disini : {0}, {1}, {2}", textInput,i2,i3);
        }
    }
}
