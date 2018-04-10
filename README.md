# MqttNetWPFClientSample
A very simple client using MQTTNet.

Default values are provided to get you up and running connected to a test MQTT Broker over websockets.

You can use http://mitsuruog.github.io/what-mqtt/ as another client, and presto you have a public chat app
(because there are never enough public chat apps)

You'll see in the Message window all the events being fired.

At the moment, I'm having difficulty seeing the Disconnected event firing, when the disconnect is physical (i.e. unplugging
the network)
