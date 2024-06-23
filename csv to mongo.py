import csv
from pymongo import MongoClient

client = MongoClient('mongodb://localhost:27017/')

db = client['sobadb']

collection = db['senzor']

csv_file_path = 'C:\\Users\\Aleksandar\\Desktop\\Temp-Hum-Sensor.csv'

def import_data_from_csv(csv_file_path, collection):
    with open(csv_file_path, 'r',encoding='utf-8-sig') as file:
        reader = csv.DictReader(file)
        cnt = 0
        for row in reader:
            #print(row)
            cnt +=1
            if cnt % 2 == 0:
                id = 2222
            else:
                id = 1111
           
            timestamp_parts = row['Timestamp'].split(" ")
            date = timestamp_parts[0]
            time = timestamp_parts[1]
            data = {
                'date': date,
                'time': time,
                'device': id,
                'battery': float(row['Battery']),
                'humidity': float(row['Humidity']),
                'temperature': float(row['Temperature']),

            }
            if cnt == 100:
                break
            collection.insert_one(data)
    print("Data imported successfully from CSV to MongoDB!")

import_data_from_csv(csv_file_path, collection)