using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Management;
using System.IO.Ports;
using USBClassLibrary;

namespace dryice_programming_gen_frequency {
    class Program {
        private static Process proc = new Process();
        static SerialPort mySerialPort = new SerialPort();
        static private List<USBClassLibrary.USBClass.DeviceProperties> ListOfUSBDeviceProperties;
        static void Main(string[] args) {
            string path_hex = "";
            string hex_file = "pay10.hex";
            string stlink = "38FF70064B46323623530443";
            string checksum = "0x003A5761";
            File.WriteAllText("dryice_program.bat",
                              "\"C:\\Program Files\\STMicroelectronics\\STM32 ST-LINK Utility\\ST-LINK Utility\\ST-" +
                              "LINK_CLI.exe\" -c \"SN\"=\"" + stlink + "\" SWD UR -P " + path_hex + hex_file +
                              " -V -Cksum " + path_hex + hex_file + " -Rst > dryice_programming.log");

            Console.WriteLine("st_link = " + stlink); Thread.Sleep(50);
            Console.WriteLine("hex_file = " + hex_file); Thread.Sleep(50);
            Console.WriteLine("checksum = " + checksum); Thread.Sleep(50);
            Stopwatch timeout_ = new Stopwatch();
            //goto aaa;
            proc.StartInfo.WorkingDirectory = "";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.FileName = "dryice_program.bat";

            bool flag_e2 = false;
            string checksum_sup = "";
            File.Delete("dryice_programming.log");
            while (true) {
                try { proc.Start(); break; } catch { Thread.Sleep(50); }
            }
            string data = "";
            Console.Write("wait log file");
            timeout_.Restart();
            while (timeout_.ElapsedMilliseconds < 5000) {
                try {
                    data = File.ReadAllText("dryice_programming.log");
                } catch (Exception) {
                    Console.Write(".");
                    Thread.Sleep(50);
                    continue;
                }
                timeout_.Stop();
                break;
            }
            Console.WriteLine(".");
            if (timeout_.IsRunning) { Console.WriteLine("upload program fail because over timeout."); Console.ReadKey(); return; }

            bool flag_e2lite = true;
            if (!data.Contains("Verification...OK")) {
                flag_e2lite = false;
                Console.WriteLine("");
                Console.WriteLine("Verification fail");
            } else {
                Console.WriteLine("");
                Console.WriteLine("Verification...OK");
            }
            Thread.Sleep(50);
            if (!data.Contains("Programming Complete.")) {
                flag_e2lite = false;
                Console.WriteLine("Programming Complete fail");
            } else Console.WriteLine("Programming Complete.");
            Thread.Sleep(50);
            if (!data.Contains(checksum)) {
                flag_e2lite = false;

                if (data.Contains("No target")) {
                    Console.WriteLine("No target");

                } else {
                    string[] ss = data.Replace("Checksum:", "$").Split('$');
                    string[] vv = ss[1].Split('[');
                    checksum_sup = vv[0].Trim().Replace("\r", "").Replace("\n", "");
                    Console.WriteLine("checksum = " + checksum_sup + " fail");
                }
                
            } else Console.WriteLine("checksum =" + checksum_sup + " pass");
            Thread.Sleep(50);

            if (flag_e2lite == true) flag_e2 = true;
            File.Delete("dryice_program.bat");
            if (flag_e2 == true) Console.WriteLine("#.......................-Programming Complete-.....................#");
            else Console.WriteLine("#.......................-Fail Fail Fail-.....................#");
            Thread.Sleep(50);

            aaa:
            string tx = "g8m";
            string rx = "PASS\n";
            ListOfUSBDeviceProperties = new List<USBClass.DeviceProperties>();
            Nullable<UInt32> MI = 0;
            MI = null;
            string comport = "COM6";
            try { comport = File.ReadAllText("comport.txt"); } catch { }
            //Console.Write("scan comport");
            //timeout_.Restart();
            //while (timeout_.ElapsedMilliseconds < 5000) {
            //    Console.Write(".");
            //    if (USBClass.GetUSBDevice(uint.Parse("0483", System.Globalization.NumberStyles.AllowHexSpecifier), uint.Parse("5740", System.Globalization.NumberStyles.AllowHexSpecifier), ref ListOfUSBDeviceProperties, true, MI)) {
            //        Console.WriteLine(".");
            //        Console.WriteLine("comport = " + ListOfUSBDeviceProperties.Count + "comport");
            //        Thread.Sleep(50);
            //        for (int iii = 0; iii < ListOfUSBDeviceProperties.Count; iii++) {
            //            comport = ListOfUSBDeviceProperties[iii].COMPort;
            //            Console.WriteLine((iii + 1) + ". " + comport);
            //            timeout_.Stop();
            //            break;
            //        }
            //    }
            //    Thread.Sleep(50);
            //}
            //if (timeout_.IsRunning) { Console.WriteLine(""); Console.WriteLine("not fine comport"); Console.ReadKey(); return; }
            //Thread.Sleep(50);

            mySerialPort.PortName = comport;
            mySerialPort.BaudRate = 9600; //19200
            mySerialPort.DataBits = 8;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.Parity = Parity.None;
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.RtsEnable = true;
            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            timeout_.Restart();
            Console.WriteLine("Opening port " + comport);
            while (timeout_.ElapsedMilliseconds < 5000) {
                try {
                    mySerialPort.Open();
                    timeout_.Stop();
                    break;
                } catch {
                    Thread.Sleep(50);
                }
                try { mySerialPort.Close(); } catch { }
                Console.Write(".");
            }
            if (timeout_.IsRunning) { Console.WriteLine(""); Console.WriteLine("Open port " + comport + " fail"); mySerialPort.Dispose(); Console.ReadKey(); return; }
            Console.WriteLine("Open port " + comport + " pass");
            Thread.Sleep(50);

            flag_data = false;
            timeout_.Restart();
            while (timeout_.ElapsedMilliseconds < 500) {
                if (flag_data == true) { flag_data = false; timeout_.Restart(); }
                Thread.Sleep(50);
            }
            timeout_.Stop();
            Console.WriteLine("send: " + tx);
            mySerialPort.DiscardInBuffer();
            mySerialPort.DiscardOutBuffer();
            rx_ = "";
            mySerialPort.Write(tx);
            if (tx == "g8m") {
                Console.WriteLine("#.......................-Command Complete-.....................#");
                mySerialPort.Close();
                mySerialPort.Dispose();
                Console.ReadKey();
                return;
            }
            timeout_.Restart();
            while (timeout_.ElapsedMilliseconds < 5000) {
                if (flag_data != true) { Thread.Sleep(100); continue; }
                flag_data = false;
                timeout_.Stop();
                break;
            }
            if (timeout_.IsRunning) {
                Console.WriteLine("read rx fail because over timeout.");
                Console.ReadKey(); 
                return;
            }
            bool result = false;
            if (rx_ == rx) result = true;
            else result = false;
            mySerialPort.Close();
            mySerialPort.Dispose();
            if (result) Console.WriteLine("#.......................-Command Complete-.....................#");
            else Console.WriteLine("#.......................-Command Complete-.....................#");

            Console.ReadKey();
        }

        static string rx_ = "";
        static bool flag_data = false;
        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {
            Thread.Sleep(50);
            rx_ = mySerialPort.ReadExisting();
            Console.WriteLine("Read: " + rx_);
            rx_ = rx_.Replace("\n", "").Replace("\r", "");
            mySerialPort.DiscardInBuffer();
            mySerialPort.DiscardOutBuffer();
            flag_data = true;
        }
    }
}
