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
            mqttClient = new MqttFactory().CreateManagedMqttClient();
            mqttClient.ApplicationMessageReceived += MqttClient_ApplicationMessageReceived;
            mqttClient.Connected += MqttClient_Connected;
            mqttClient.Disconnected += MqttClient_Disconnected;

        }

        private IManagedMqttClientOptions options;
        private IManagedMqttClient mqttClient;

        private async void Button_Connect(object sender, RoutedEventArgs e)
        {
            try
            {
                options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                    .WithClientOptions( new MqttClientOptionsBuilder()
                            .WithCommunicationTimeout(TimeSpan.FromSeconds(5))     // Default value
                            .WithKeepAlivePeriod(TimeSpan.FromSeconds(7.5))          // Default Value
                            .WithWebSocketServer(mqttAddress.Text)
                            .Build())
                            .Build();

                await mqttClient.StartAsync(options);
                
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
            await mqttClient.StopAsync();
            DisplayMessage("Client Stopped");
        }
        private async void Button_Subscribe(object sender, RoutedEventArgs e)
        {
            DisplayMessage(mqttClient.IsConnected.ToString());
            try
            {
                if (mqttClient.IsConnected)
                {
                    DisplayMessage("Subscribing");
                    await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(Topic.Text).Build());                    
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
