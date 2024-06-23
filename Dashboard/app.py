import nats
import json
import asyncio
from datetime import datetime
import influxdb_client, os, time
from influxdb_client import InfluxDBClient, Point, WritePrecision
from influxdb_client.client.write_api import SYNCHRONOUS
# Configuration
nats_url = "nats://localhost:4222"
token = "1lJjTQXQ8enhq6tRk4VbbyvkIAFhSAVMmOVmC_EoSD4VombXR27bHUNa1DohmXiRcFJ-abTN03cSkl7DQAVpbA=="
org = "elfak"
url = "http://localhost:8086"
bucket="senzor"


async def run():
    # Connect to NATS
    nc = await nats.connect(nats_url)
    print(f"Connected to NATS at {nats_url}")
    write_client = influxdb_client.InfluxDBClient(url=url, token=token, org=org)
    write_api = write_client.write_api(write_options=SYNCHRONOUS)

    async def message_handler(msg):
        data = msg.data.decode()
        print(f"Received a message: {data}")
       

        try:
            dataJson = json.loads(data)

            date_str = dataJson.get("Date")
        
            point_time = datetime.fromisoformat(date_str.rstrip("Z"))  

            # Prepare data for InfluxDB
            point = Point("sensor") \
                .tag("sensor_id", dataJson.get("Device")) \
                .field("avg_humidity", dataJson.get("AverageHumidity")) \
                .field("avg_temperature", dataJson.get("AverageTemperature")) \
                .field("battery", dataJson.get("Battery")) \
                .time(dataJson.get("point_time"))

            # Write data to InfluxDB
            write_api.write(bucket=bucket, org="elfak", record=point)
            print(f"Stored data in InfluxDB: {data}")

        except json.JSONDecodeError as e:
            print(f"Failed to decode JSON: {e}")
        except Exception as e:
            print(f"Error storing data in InfluxDB: {e}")

    
    # Subscribe to a subject
    await nc.subscribe("prosek", cb=message_handler)
    print(f"Subscribed to 'prosek' subject")
    # Keep the connection alive
    while True:
        await asyncio.sleep(1)

if __name__ == "__main__":
    asyncio.run(run())
