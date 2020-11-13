using MSTSCLib;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDPTester
{
    public partial class Form1 : Form
    {
        private string directoryPath = @"C:\testerRDP";
        private bool loginOK = false;


        public Form1()
        {
            InitializeComponent();
        }

        private void checkDate()
        {
            DateTime now = DateTime.Now;
            DateTime newDate = new DateTime(2020, 11, 21);

            double diff = (newDate - now).TotalDays;

            int days = (int)diff;

            if (days <= 0)
            {
                Application.Exit();
            }
        }

        private void rComputersList_TextChanged(object sender, EventArgs e)
        {
            var lineCount = rComputersList.Lines.Count();
            label4.Text = lineCount.ToString();
        }

        private void checkConnect(string server, string userName, string password)
        {
            try
            {
                rdp.Server = server;
                rdp.UserName = userName;

                IMsTscNonScriptable secured = (IMsTscNonScriptable)rdp.GetOcx();
                secured.ClearTextPassword = password;
                rdp.Connect();

                Debug.WriteLine(rdp.Connected, "checkConnect");
            }
            catch (Exception) { };

        }

        private async Task<bool> isCheckLoginComplete()
        {
            /* await Task.Delay(6000);*/

            Debug.WriteLine(rdp.Connected, "isCheckLoginComplete");
            if (rdp.Connected.ToString() == "1")
            {
                rdp.Disconnect();
                return true;
            }
            return false;
        }

        private bool checkUserAndPassword()
        {
            if (string.IsNullOrEmpty(tbUserName.Text.ToString()))
            {
                MessageBox.Show("NIE WYPEŁNIONO POLA UŻYTKOWNIK");
                return false;
            }
            return true;
        }

        private async void bStartTest_Click(object sender, EventArgs e)
        {
            loginOK = false;

            if (checkUserAndPassword())
            {
                DateTime dateFileName = DateTime.Now;
                string correctDataFileName = dateFileName.ToString("yyyy_MM_dd_HH_mm_ss");

                string fileName = "RaportRDP_" + correctDataFileName;

                dataGridView1.Rows.Clear();
                rLogi.Text = String.Empty;

                tbUserName.Enabled = false;
                mtbUserPassword.Enabled = false;
                rComputersList.Enabled = false;
                bStartTest.Enabled = false;
                bStartTest.Text = "TRWA TEST ...";

                int count = 1;
                int proby = 1;

                var lines = rComputersList.Lines;
                var lineCount = rComputersList.Lines.Count();

                for (int i = 0; i < lineCount; i++)
                {
                    rLogi.Text = String.Empty;
                    DateTime now = DateTime.Now;
                    string correctData = now.ToString("yyyy-MM-dd HH:mm:ss");

                    string computerName = rComputersList.Lines[i];

                    Debug.WriteLine(computerName, "computer name");

                    rLogi.Text += "NAZWA KOMPUTERA: " + computerName + Environment.NewLine;

                    while (proby < 4)
                    {
                        Debug.WriteLine(proby, "Próba");

                        rLogi.Text += "PRÓBA ŁĄCZENIA: " + proby + Environment.NewLine;

                        checkConnect(computerName, tbUserName.Text.ToString(), mtbUserPassword.Text.ToString());

                        /* bool checkLoginComplete = await Task.Run(async () => await isCheckLoginComplete());*/
                        await Task.Delay(6000);

                        if (proby == 3 || loginOK)
                        {
                            if (loginOK)
                            {
                                saveUserAndPasswordToFile(fileName, computerName, "ZALOGOWANO", count.ToString());

                                rLogi.Text += "ZALOGOWANO !!" + Environment.NewLine;
                                addResultToDataGridView(count.ToString(), computerName, "ZALOGOWANO", correctData, false);
                            }
                            else
                            {
                                saveUserAndPasswordToFile(fileName, computerName, "BŁĄD LOGOWANIA", count.ToString());
                                rLogi.Text += "BŁĄD LOGOWANIA !!" + Environment.NewLine;
                                addResultToDataGridView(count.ToString(), computerName, "BŁĄD LOGOWANIA", correctData, true);
                            }
                            proby = 3;
                        }
                        proby++;
                    }

                    loginOK = false;
                    proby = 1;
                    count++;

                    if (rdp.Connected.ToString() == "1")
                    {
                        rdp.Disconnect();
                    }

                }

                tbUserName.Enabled = true;
                mtbUserPassword.Enabled = true;
                rComputersList.Enabled = true;
                bStartTest.Enabled = true;
                bStartTest.Text = "ROZPOCZNIJ TEST";
            }
        }

        private void addResultToDataGridView(string computerNumber, string computerName, string result, string data, bool isError)
        {
            dataGridView1.Rows.Add(computerNumber, computerName, result, data);

            int lastIndex = dataGridView1.Rows.Count - 2;

            if (!isError)
            {
                DataGridViewRow row = dataGridView1.Rows[lastIndex];
                row.DefaultCellStyle.BackColor = Color.Green;
            }
            else
            {
                DataGridViewRow row = dataGridView1.Rows[lastIndex];
                row.DefaultCellStyle.BackColor = Color.Red;
            }
        }
        private bool saveUserAndPasswordToFile(string fileName, string computerName, string computerStatus, string count)
        {
            string tmpFileName = directoryPath + @"\" + fileName + ".txt";

            if (!Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                }
                catch (Exception)
                {
                    MessageBox.Show("NIE UDAŁO SIĘ STWORZYĆ KATALOGU DO RAPORTÓW !! \n" + directoryPath, "BŁĄD", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            if (!File.Exists(tmpFileName))
            {
                try
                {
                    File.Create(tmpFileName).Dispose();
                }
                catch (Exception)
                {
                    MessageBox.Show("NIE UDAŁO SIĘ STWORZYĆ PLIKU ", "BŁĄD", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            if (File.Exists(tmpFileName))
            {
                try
                {
                    File.AppendAllText(tmpFileName, count + ".      " + computerName + "            ");
                    File.AppendAllText(tmpFileName, computerStatus);
                    File.AppendAllText(tmpFileName, Environment.NewLine);
                }
                catch (Exception)
                {
                    MessageBox.Show("NIE UDAŁO SIĘ ZAPISAĆ DANYCH DO PLIKU ", "BŁĄD", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                    return false;
                }

            }
            return true;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            checkDate();
        }

        private void rdp_OnLoginComplete(object sender, EventArgs e)
        {
            Debug.WriteLine("login ok");
            loginOK = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }
    }
}
