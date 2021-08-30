using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace trackingSystem
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent(); //yusufcihankmr
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Hide();

            radioButton1.Checked = true;
            radioButton2.Checked = false;

            webcam = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo videocapturedevice in webcam)
            {
                comboBox1.Items.Add(videocapturedevice.Name);
            }
            comboBox1.SelectedIndex = 0;
        }

        private double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public void AddLog(string log)
        {
            string str = "";

            var path = Application.StartupPath + $@"\log.txt";

            using (StreamReader sreader = new StreamReader(path))
            {
                str = sreader.ReadToEnd();
            }

            File.Delete(path);

            using (StreamWriter swriter = new StreamWriter(path, false))
            {
                str = $"{DateTime.Now} {log}" + Environment.NewLine + str;
                swriter.Write(str);
            }

            FileInfo info = new FileInfo(path);
            long dosyaBoyutu = info.Length;
            if (dosyaBoyutu > 999999999)
            {
                File.WriteAllText(path, "");
            }
        }
        public void SendMail(string body, string fileLocation, string fileLocation1)
        {
            SmtpClient SmtpServer = new SmtpClient("mail.mymail.com.tr");
            var mail = new MailMessage();
            mail.From = new MailAddress("from@mymail.com.tr");
            mail.To.Add("to@gmail.com");
            mail.Subject = "tracking system";
            mail.IsBodyHtml = true;
            string htmlBody;
            htmlBody = body;
            mail.Body = htmlBody;
            var attachment = new Attachment(fileLocation);
            mail.Attachments.Add(attachment);
            var attachment1 = new Attachment(fileLocation1);
            mail.Attachments.Add(attachment1);
            SmtpServer.Port = 587;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential("from@mymail.com.tr", "myMail123");
            SmtpServer.EnableSsl = true;
            SmtpServer.Timeout = int.MaxValue;
            SmtpServer.Send(mail);
        }
        public Bitmap Screenshot()
        {
            this.Opacity = 0;
            Bitmap Screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics GFX = Graphics.FromImage(Screenshot);
            GFX.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size);
            this.Opacity = 1;

            return Screenshot;
        }

        private FilterInfoCollection webcam;
        private VideoCaptureDevice cam;
        private void cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bit = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = bit;
        }

        [DllImport("user32")]
        public static extern void LockWorkStation();

        private Point SonKonum { get; set; }
        private bool Kontrol { get; set; }
        private int Sayac { get; set; }
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                if (radioButton1.Checked == true)
                {
                    Point konum = trackingSystem.Mouse.GetCursorPosition();

                    if (Kontrol)
                    {
                        SonKonum = konum;
                        Kontrol = false;
                        timer1.Enabled = true;
                        return;
                    }

                    if (SonKonum != konum)
                    {
                        SonKonum = konum;
                        AddLog("mouse konumu değiştirildi: " + konum.ToString());
                        listBox1.Items.Insert(0, DateTime.Now + " - mouse konumu değiştirildi: " + konum.ToString());

                        cam = new VideoCaptureDevice(webcam[comboBox1.SelectedIndex].MonikerString);
                        cam.NewFrame += new NewFrameEventHandler(cam_NewFrame);
                        cam.Start();

                        Thread.Sleep(1000);

                        if (cam.IsRunning)
                            cam.Stop();

                        if (!Directory.Exists(@"C:\trackingSystem"))
                            Directory.CreateDirectory(@"C:\trackingSystem");

                        string fileLocation = $@"C:\trackingSystem\busted {ConvertToUnixTimestamp(DateTime.Now)}.jpg";
                        string fileLocation1 = $@"C:\trackingSystem\screenshot {ConvertToUnixTimestamp(DateTime.Now)}.jpg";
                        pictureBox1.Image.Save(fileLocation);
                        Screenshot().Save(fileLocation1);
                        SendMail(DateTime.Now + " Cihazınıza istenmeyen erişim tespit edildi. " + konum.ToString(), fileLocation, fileLocation1);

                        Sayac++;
                        if (Sayac == 3)
                        {
                            Sayac = 0;
                            LockWorkStation();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog(ex.Message);
            }
            timer1.Enabled = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Kontrol = true;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TamamenKapat == 1)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void açToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private int TamamenKapat = 0;
        private void kapatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TamamenKapat = 1;
            Application.Exit();
        }

        private void linkLabel1_Click(object sender, EventArgs e)
        {
            Process.Start("https://linkedin.com/in/yusufcihankmr");
            Process.Start("https://github.com/yusufcihankmr");
        }
    }
}
