using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PingServer
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource _cancellationTokenSource;
        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PingServer",
            "localStorage.json");

        public class LocalStorageData
        {
            public string Ip_host { get; set; }
            //public string Hostname { get; set; }
            //public string LastPingResult { get; set; }
        }

        private void EnsureDirectoryExists()
        {
            string directoryPath = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            //if (!File.Exists(_filePath))
            //{
            //    File.WriteAllText(_filePath, "{}"); // Create an empty JSON file
            //}

        }

        public Form1()
        {

            InitializeComponent();
            //StartBackgroundService();



        }

        private void SaveData(string ipAddress)
        {
            EnsureDirectoryExists();

            var data = new LocalStorageData
            {
                Ip_host = ipAddress,
                //Hostname = hostname,
                //LastPingResult = pingResult
            };

            //string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            //if (!File.Exists(_filePath))
            //{

            //    File.WriteAllText(_filePath, jsonData); // Create an empty JSON file
            //}
            //else
            //{
            //    File.WriteAllText(_filePath, jsonData);
            //}
            List<LocalStorageData> dataList = new List<LocalStorageData>();

            if (File.Exists(_filePath))
            {
                string jsonData = File.ReadAllText(_filePath);
                if (!string.IsNullOrEmpty(jsonData))
                {
                    dataList = JsonConvert.DeserializeObject<List<LocalStorageData>>(jsonData);
                }
            }

            dataList.Add(data);

            string updatedJsonData = JsonConvert.SerializeObject(dataList, Formatting.Indented);
            File.WriteAllText(_filePath, updatedJsonData);

        }

        private LocalStorageData[] LoadData()
        {
            if (File.Exists(_filePath))
            {
                string jsonData = File.ReadAllText(_filePath);

                //if json data is not empty


                // Deserialize JSON data into a list of LocalStorageData objects
                var dataList = JsonConvert.DeserializeObject<List<LocalStorageData>>(jsonData);
                return dataList?.ToArray(); // Convert List to array

                //if (dataList != null)
                //{
                //    txtIp_host.Text = dataList.Ip_host;
                //    txtHostname.Text = data.Hostname;
                //    lblStatus.Text = data.LastPingResult;
                //}
            }

            //return File.WriteAllText(_filePath, "{}"); // Create an empty JSON file

            return Array.Empty<LocalStorageData>();
        }




        private void StartBackgroundService()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => PingService(_cancellationTokenSource.Token));
        }

        private async Task PingService(CancellationToken cancellationToken)
        {
            //string ipAddress = "8.8.8.8"; // Replace with your server/IP
            
            // Retrieve all stored data as an array
            LocalStorageData[] storedData = LoadData();
            Ping ping = new Ping();

            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var data in storedData)
                {

                    try
                    {
                        PingReply reply = ping.Send(data.Ip_host);
                        string result = reply.Status == IPStatus.Success
                            ? $"Ping successful: {reply.RoundtripTime} ms"
                            : "Ping failed.";
                        UpdateUI(result);

                    }
                    catch (Exception ex)
                    {
                        UpdateUI($"Error: {ex.Message}");

                    }

                }
                

                await Task.Delay(5000, cancellationToken); // Wait for 5 seconds
            }
        }

        private void UpdateUI(string result)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => label1.Text = result));
            }
            else
            {
                label1.Text = result;
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            //EnsureDirectoryExists();

            LoadData();
            StartBackgroundService();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveData(txtIp_host.Text);
            StartBackgroundService();
        }
    }
}
