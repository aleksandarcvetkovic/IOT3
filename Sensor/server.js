const { MongoClient } = require('mongodb');
const mqtt = require('mqtt');

const mongoUrl = 'mongodb://localhost:27017'; // Replace with your MongoDB URL
const dbName = 'sobadb'; // Replace with your database name
const collectionName = 'senzor'; // Replace with your collection name

const mqttUrl = 'mqtt://localhost:1883'; // MQTT broker URL
const client = new MongoClient(mongoUrl);

async function findAllDocuments(collection) {
  try {
    
    const documents = await collection.find().toArray();
    //console.log('Documents found in mongo:', documents);
    return documents;
  } catch (error) {
    console.error('Error finding documents:', error);
  }
}
async function closeConnection() {
  try {
      await client.close();
      console.log('Disconnected from MongoDB');
  } catch (error) {
      console.error('Error closing connection:', error);
  }
}

process.on('SIGINT', () => {
  closeConnection().then(() => process.exit(0));
});


async function main() {
  // Connect to MongoDB
  console.log('radi node');

  await client.connect();
  console.log('Connected successfully to MongoDB server');

  const db = client.db(dbName);
  const collection = db.collection(collectionName);

  // Read data from MongoDB
  const data = await findAllDocuments(collection);
  //console.log('Data retrieved from MongoDB:', data);

  // Connect to MQTT broker
  const mqttClient = mqtt.connect(mqttUrl);
  mqttClient.on('connect', async () => {

    for(let i = 0; i < data.length; i++){
      const message = JSON.stringify(data[i]);
      mqttClient.publish('merenje', message, () => {
        console.log('Message published:', message);
        //sleep for 3 second
      });
      await new Promise(resolve => setTimeout(resolve, 2000));
    }
   
  });

  // Close connections after publishing
  mqttClient.on('close', async () => {
    console.log('MQTT connection closed');
    await client.close();
    console.log('MongoDB connection closed');
  });

  mqttClient.on('error', (error) => {
    console.error('MQTT Error:', error);
    mqttClient.end();
  });
  
}

main();
