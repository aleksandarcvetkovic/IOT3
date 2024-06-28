const mqtt = require('mqtt');
const WebSocket = require('ws');
const express = require('express');

//exxpress configuration
const app = express();
const port = 3001;

app.use(express.static('public'));

const server = app.listen(port, () => {
    console.log(`Server is running on http://localhost:${port}`);
});

const webSocketServer = new WebSocket.Server({ server });


// MQTT configuration
const mqttBrokerUrl = 'mqtt://mqttumrezi:1883';
const mqttTopic = 'event';


// Create MQTT client
const mqttClient = mqtt.connect(mqttBrokerUrl);


// Handle new MQTT messages
mqttClient.on('message', (topic, message) => {
    // Convert the message to a string
    const messageString = message.toString();
    console.log(messageString);
    // Broadcast the message to all connected WebSocket clients

    webSocketServer.clients.forEach((client) => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(messageString);
        }
    });
   
});

// Subscribe to the MQTT topic
mqttClient.on('connect', () => {
    mqttClient.subscribe(mqttTopic);
});

// Handle new WebSocket connections
webSocketServer.on('connection', (webSocket) => {
    console.log('New WebSocket connection');


    // Handle WebSocket errors
    webSocket.on('error', (error) => {
        console.error('WebSocket error:', error);
    });
});


console.log('Server started');