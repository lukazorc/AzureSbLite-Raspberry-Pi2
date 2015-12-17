using ppatierno.IoT;
using ppatierno.IoT.Hardware;
//using ppatierno.IoTCoreSensors.Hardware; 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace sbLiteRasp
{
     public sealed partial class MainPage : Page 
     { 
        private string connectionString = "Endpoint=sb://XXXXX-ns.servicebus.windows.net/;SharedAccessKeyName=RasPi;SharedAccessKey=XXXXX";
        private string eventHubEntity = "ehdevices";

        private IIoTClient iotClient; 
          
         public MainPage()
         { 
            this.InitializeComponent();
            InitSPI();
            delay();   
         }

        private async void delay()
        {
            await Task.Delay(120000);

            // set and open the IoT client             
            if (this.iotClient == null)
            {
                //this.iotClient = new IoTClient("raspberrypi2", Guid.NewGuid().ToString(), this.connectionString, this.eventHubEntity); 
                this.iotClient = new IoTClientConnectTheDots("raspberrypi2", Guid.NewGuid().ToString(), this.connectionString, this.eventHubEntity);
            }


            if (!this.iotClient.IsOpen)
                this.iotClient.Open();

            // just to start without UI :-) 
            this.btnStart_Click(null, null);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
         { 
             Task.Run(async() =>
             { 
                // bool isOpened = await this.tmp102.OpenAsync(); 
 
 
                 IDictionary bag = new Dictionary<SensorType, double>();
                // float te = convertToDouble(readBuffer);
                 //float t = 20;
                 while (true) 
                 {
                     SpiDisplay.TransferFullDuplex(writeBuffer, readBuffer);
                     double te = convertToDouble(readBuffer);
                     //float temperature = tmp102.Temperature(); 
                     double temperature = te;
                    // t++;
                     SensorType sensorType = SensorType.Temperature; 
     
 
                     if (!bag.Contains(sensorType)) 
                         bag.Add(sensorType, temperature); 
                    else 
                         bag[sensorType] = temperature; 
 
                     if ((this.iotClient != null) && (this.iotClient.IsOpen)) 
                     {
                        this.iotClient.SendAsync(bag);
                     } 
 
                     await Task.Delay(5000); 
                 } 
             }); 
        } 
 
         private void btnStop_Click(object sender, RoutedEventArgs e)         { 
             this.iotClient.Close(); 
         }

        public double convertToDouble(byte[] data)
        {
            /*Uncomment if you are using mcp3208/3008 which is 12 bits output */
            int result = data[1] & 0x0F;
            result <<= 8;
            result += data[2];
            double millivolts = Convert.ToDouble(result * (3.4 / 4095)); // (VOLTS / 4095) Volte prebrat z multimetrom.
           
            double temp_C = (millivolts - 0.5) * 100;
            return temp_C; //Dobimo Celsius
            //return millivolts;
            /*Uncomment if you are using mcp3002*/
            /* int result = data[1] & 0x03;
             result <<= 8;
             result += data[2];
             result =  (int)(result * (((3300.0 / 1024.0) - 100.0) / 10.0) - 40.0);
             return result ;*/
        }

        private async void InitSPI()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 500000;// 10000000;
                settings.Mode = SpiMode.Mode0; //Mode3;

                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                SpiDisplay = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
            }

            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        /*RaspBerry Pi2  Parameters*/
        private const string SPI_CONTROLLER_NAME = "SPI0";  /* For Raspberry Pi 2, use SPI0                             */
        private const Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */

        /*Uncomment if you are using mcp3208/3008 which is 12 bits output */
        byte[] readBuffer = new byte[3]; /*this is defined to hold the output data*/
        byte[] writeBuffer = new byte[3] { 0x06, 0x00, 0x00 };//00000110 00; // It is SPI port serial input pin, and is used to load channel configuration data into the device

        /*Uncomment if you are using mcp3002*/
        /* byte[] readBuffer = new byte[3]; /*this is defined to hold the output data*/
        // byte[] writeBuffer = new byte[3] { 0x68, 0x00, 0x00 };//01101000 00; /* It is SPI port serial input pin, and is used to load channel configuration data into the device*/

        private SpiDevice SpiDisplay;
    } 
 } 

