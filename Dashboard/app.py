import nats
import asyncio
from influxdb_client import InfluxDBClient, Point

# Configuration
nats_url = "nats://localhost:4222"
influxdb_url = "http://localhost:8086"
influxdb_token = "your_token"
influxdb_org = "your_org"
influxdb_bucket = "your_bucket"

async def run():
    # Connect to NATS
    nc = await nats.connect(nats_url)
    print(f"Connected to NATS at {nats_url}")
    # Connect to InfluxDB
    #client = InfluxDBClient(url=influxdb_url, token=influxdb_token, org=influxdb_org)
    #write_api = client.write_api()

    async def message_handler(msg):
        data = msg.data.decode()
        print(f"Received a message: {data}")
        
        # Create a point and write to InfluxDB
        #point = Point("measurement_name").field("field_name", data)
        #write_api.write(bucket=influxdb_bucket, org=influxdb_org, record=point)
    
    # Subscribe to a subject
    await nc.subscribe("prosek", cb=message_handler)
    print(f"Subscribed to 'prosek' subject")
    # Keep the connection alive
    while True:
        await asyncio.sleep(1)

if __name__ == "__main__":
    asyncio.run(run())
