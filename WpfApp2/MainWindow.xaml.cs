using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mqttClient = new MqttFactory().CreateMqttClient();
            mqttClient.ApplicationMessageReceived += MqttClient_ApplicationMessageReceived;
            mqttClient.Connected += MqttClient_Connected;
            mqttClient.Disconnected += MqttClient_Disconnected;

            WatchDog = new Timer((stateInfo) => { if (!mqttClient.IsConnected) { DisplayMessage("Watchdog reports the client is not connected");WatchDog.Change(Timeout.Infinite, Timeout.Infinite); } });
        }

        private Timer WatchDog;
        private IMqttClientOptions options;
        private IMqttClient mqttClient;

        private async void Button_Connect(object sender, RoutedEventArgs e)
        {
            try
            {
                options = new MqttClientOptionsBuilder()
                            .WithCommunicationTimeout(TimeSpan.FromSeconds(10))     // Default value
                            .WithKeepAlivePeriod(TimeSpan.FromSeconds(15))          // Default Value
                            .WithWebSocketServer(mqttAddress.Text)
                            .Build();

                var result = await mqttClient.ConnectAsync(options);
                DisplayMessage(mqttClient.IsConnected ? "Apparently the client is connected" : "WHAT the client isn't connected?");
                WatchDog.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }
            catch(Exception ex)
            {
                DisplayMessage($"EXCEPTION IN Button_Connect {ex.Message}");
            }
            
        }

        private void DisplayMessage(string message)
        {
            Dispatcher.BeginInvoke((Action)(() => { messages.Items.Add($"{DateTime.Now}\t{message}"); }));
        }
        private  void MqttClient_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            DisplayMessage("Disconnected");
            
        }

        private  void MqttClient_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            DisplayMessage("Connected");
            
        }

        private  void MqttClient_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {

            try
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                DisplayMessage(message);
                
            }
            catch(Exception ex)
            {
                DisplayMessage(ex.Message);
            }
        }
 
        private async void Button_Disconnect(object sender, RoutedEventArgs e)
        {
            await mqttClient.DisconnectAsync();
        }
        private async void Button_Subscribe(object sender, RoutedEventArgs e)
        {
            DisplayMessage(mqttClient.IsConnected.ToString());
            try
            {
                if (mqttClient.IsConnected)
                {
                    DisplayMessage("Subscribing");
                    var result = await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(Topic.Text).Build());
                    DisplayMessage($"Subscribe result: {result.SingleOrDefault()?.ReturnCode.ToString()}");
                }
                else
                    DisplayMessage("Client is not connected");
            }
            catch(Exception ex)
            {
                DisplayMessage($"EXCEPTION IN Button_Subscribe.  {ex.Message}");
            }
        }

        private async void Button_Unsubscribe(object sender, RoutedEventArgs e)
        {
            await mqttClient.UnsubscribeAsync(Topic.Text);
        }

        private async void Button_Publish(object sender, RoutedEventArgs e)
        {
            DisplayMessage($"Publishing message {message.Text} to topic {Topic.Text}");
            await mqttClient.PublishAsync(new MqttApplicationMessageBuilder().WithTopic(Topic.Text).WithPayload(message.Text).Build());
        }

        private void Button_MessageClear(object sender, RoutedEventArgs e)
        {
            messages.Items.Clear();
        }
    }
}
