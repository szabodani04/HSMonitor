using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.Windows.Threading;
using Microsoft.Win32;
using System.IO;
using OpenHardwareMonitor.Hardware;
namespace HSMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Computer computer = new Computer() { CPUEnabled = true, GPUEnabled = true, MainboardEnabled = true, RAMEnabled = true, HDDEnabled = true };
        
        List<string> adatok = new List<string>();
        List<string> programsave = new List<string>();
        public void DispatcherTimerSample()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(800);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            CPUValues();
            RamValues();
            GpuValues();
            Disks();
        }

        string[] meretek = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public MainWindow()
        {
            computer.Open();
            InitializeComponent();
            Gep();
            OPrendszer();
            Proci();
            Videokartya();
            Memory();
            Hattertar();
            Audio();
            Programok();
            DispatcherTimerSample();
        }
        public void CPUValues()
        {
            string procho = "";
            string prochasznalat = "";
            string oszzhasznalat = "";
            foreach (var hardwareItem in computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.CPU)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            procho += $"{sensor.Name} = {sensor.Value.Value}°C\r\n";
                            if (sensor.Name == "CPU Package")
                            {
                                processzorhofok.Content = $"{sensor.Value.Value}°C";
                                if (Math.Round(sensor.Value.Value, 1) < 50)
                                    prochoprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                                else if (Math.Round(sensor.Value.Value, 1) < 70)
                                    prochoprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(255, 153, 0));
                                else
                                    prochoprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                                prochoprocessbar.Value = sensor.Value.Value;
                            }
                        }
                        else if (sensor.SensorType == SensorType.Load)
                        {
                            prochasznalat += $"{sensor.Name} = {Math.Round(sensor.Value.Value, 1):N0}%\r\n";
                            if (sensor.Name == "CPU Total")
                            {
                                oszzhasznalat = $"{Math.Round(sensor.Value.Value, 1):N0}%";
                                if (Math.Round(sensor.Value.Value, 1) < 50)
                                    procprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                                else if (Math.Round(sensor.Value.Value, 1) < 80)
                                    procprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(255, 153, 0));
                                else
                                    procprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                                procprocessbar.Value = Math.Round(sensor.Value.Value, 1);
                            }
                        }
                    }
                }
            }
            processzorhasznalat.Content = oszzhasznalat;
            procsensor.Content = procho;
            procusage.Content = prochasznalat;
        }
        public void RamValues()
        {
            string memhasznalat = "";
            foreach (var hardwareItem in computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.RAM)
                {
                    hardwareItem.Update();
                    foreach (IHardware subHardware in hardwareItem.SubHardware)
                        subHardware.Update();

                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load)
                        {
                            memhasznalat += $"{Math.Round(sensor.Value.Value, 1):N0}%\r\n";
                            if (Math.Round(sensor.Value.Value, 1)<50)
                                ramprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                            else if (Math.Round(sensor.Value.Value, 1) < 80)
                                ramprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(255, 153, 0));
                            else
                                ramprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                            ramprocessbar.Value = Math.Round(sensor.Value.Value, 1);
                        }
                        else if (sensor.SensorType == SensorType.Data && sensor.Name == "Used Memory")
                        {
                            memusge.Content = $"{sensor.Value.Value:N1} GB";
                        }
                    }
                }
            }
            ramload.Content = memhasznalat;
        }
        public void GpuValues()
        {
            string gpuho = "";
            string gpuhasznalat = "";
            foreach (var hardwareItem in computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAti)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            gpuho += $" {sensor.Value.Value}°C\r\n";
                            if (Math.Round(sensor.Value.Value, 1) < 50)
                                gpuprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                            else if (Math.Round(sensor.Value.Value, 1) < 70)
                                gpuprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(255, 153, 0));
                            else
                                gpuprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                            gpuprocessbar.Value = sensor.Value.Value;
                        }
                        else if(sensor.SensorType == SensorType.Load && sensor.Name == "GPU Memory")
                        {
                            gpuhasznalat += $"{Math.Round(sensor.Value.Value, 1):N0}%\r\n";
                            if (Math.Round(sensor.Value.Value, 1) < 50)
                                vidmemprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                            else if (Math.Round(sensor.Value.Value, 1) < 80)
                                vidmemprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(255, 153, 0));
                            else
                                vidmemprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                            vidmemprocessbar.Value = sensor.Value.Value;
                        }
                        else if(sensor.SensorType == SensorType.Fan)
                        {
                            fanspeed.Content = $"{Math.Round(sensor.Value.Value, 0)}RPM\r\n";
                            gpuventiprocessbar.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                            gpuventiprocessbar.Value = Math.Round(sensor.Value.Value, 0);
                        }
                            
                    }
                }
                videokartyaho.Content = gpuho;
                videokartyahasznalat.Content = gpuhasznalat;
            }
        }
        public void Disks()
        {
            disks.Items.Clear();
            ManagementObjectSearcher gep = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
            foreach (var item in gep.Get())
            {
                long hasznalt = Convert.ToInt64(item["Size"]) - Convert.ToInt64(item["FreeSpace"]);
                disks.Items.Add(item["Name"] + " " + Meret(Convert.ToInt64(item["Size"])) + " / " + Meret(hasznalt) );
            }
            
        }
        public void Gep()
        {
            ManagementObjectSearcher gep = new ManagementObjectSearcher("SELECT * FROM Win32_Baseboard");
            foreach (var item in gep.Get())
            {
                alaplapgyarto.Content = $"{item["Manufacturer"]}";
                adatok.Add($"Számítógép: {item["Manufacturer"]}");
            }
        }
        public void OPrendszer()
        {
            ManagementObjectSearcher gep = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (var item in gep.Get())
            {
                oprendszer1.Content = $"{item["Caption"]}";
                adatok.Add($"Operációs rendszer: {item["Caption"]}");
            }
        }
        public void Proci()
        {
            ManagementObjectSearcher processzor = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var item in processzor.Get())
            {
                processzornev.Content = $"{item["Name"]}";
                processzormag.Content = $"{item["ThreadCount"]}";
                adatok.Add($"Processzor: {item["Name"]}");
                adatok.Add($"Processzor magok száma: {item["ThreadCount"]}");
            }
        }
        public void Videokartya()
        {
            adatok.Add("Videókártyák:");
            ManagementObjectSearcher videokartya = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (var item in videokartya.Get())
            {
                videokartyalista.Items.Add(item["Name"] + "   | " + Meret(Convert.ToInt64(item["AdapterRam"])));
                adatok.Add("  " + item["Name"] + "   | " + Meret(Convert.ToInt64(item["AdapterRam"])));
            }
        }
        public void Memory()
        {
            adatok.Add("Memóriák:");
            ManagementObjectSearcher memory = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            foreach (var item in memory.Get())
            {
                string ddrtype = "";
                if (Convert.ToInt32(item["MemoryType"]) == 20)
                    ddrtype = $"DDR";
                else if (Convert.ToInt32(item["MemoryType"]) == 21)
                    ddrtype = $"DDR2";
                else if (Convert.ToInt32(item["MemoryType"]) == 24)
                    ddrtype = $"DDR3";
                else if (Convert.ToInt32(item["MemoryType"]) == 26)
                    ddrtype = $"DDR4";
                ram.Items.Add($" {Convert.ToString(ddrtype)} {item["Manufacturer"]} {item["Tag"]} {Meret(Convert.ToInt64(item["Capacity"]))} {item["Speed"]} MHz");
                    adatok.Add("  " + Convert.ToString(ddrtype) + item["Manufacturer"] + item["Tag"] + " " + Meret(Convert.ToInt64(item["Capacity"])) + " " + item["Speed"] + "MHz");
                
            }
        }
        public void Hattertar()
        {
            adatok.Add("Háttértárak:");
            ManagementObjectSearcher disk = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (var item in disk.Get())
            {
                hattertarlist.Items.Add(item["Model"] + " " + Meret(Convert.ToInt64(item["Size"])) + " ");
                adatok.Add("  " + item["Model"] + " " + Meret(Convert.ToInt64(item["Size"])) + " ");
               
            }
        }
        public void Audio()
        {
            adatok.Add("Audioeszközök:");
            ManagementObjectSearcher disk = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice");
            foreach (var item in disk.Get())
            {
                    hang.Items.Add(item["Name"]);
                    adatok.Add("  " + Convert.ToString(item["Name"]));
            }
        }
        public void Programok()
        {
            ManagementObjectSearcher prog = new ManagementObjectSearcher("SELECT * FROM Win32_Product");
            foreach (var item in prog.Get())
            {
                programok.Items.Add($"{item["Name"]}    | {item["Version"]}");
                programsave.Add($"{item["Name"]}    | {item["Version"]}");
            }
        }

        private string Meret(Int64 value)
        {
            if (value < 0) { return "-" + Meret(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, meretek[mag]);
        }

        private void save_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Title = "Számítógép adatainak mentése";
            save.Filter = "Szöveges fájl (*.txt)|*.txt|Minden állomány (*.*)|*.*";
            save.DefaultExt = ".txt";
            save.CheckPathExists = true;
            if (save.ShowDialog() == true)
            {
                for (int i = 0; i < adatok.Count; i++)
                {
                    File.AppendAllText(save.FileName, adatok[i] + Environment.NewLine);
                }
            }
        }
        private void alkalmazasment_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Title = "Számítógép adatainak mentése";
            save.Filter = "Szöveges fájl (*.txt)|*.txt|Minden állomány (*.*)|*.*";
            save.DefaultExt = ".txt";
            save.CheckPathExists = true;
            if (save.ShowDialog() == true)
            {
                for (int i = 0; i < programsave.Count; i++)
                {
                    File.AppendAllText(save.FileName, programsave[i] + Environment.NewLine);
                    
                }
            }
        }
    }
}
