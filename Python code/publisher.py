# publisher.py
import json
import datetime, time
import paho.mqtt.client as mqtt
import humiture
import logging
from getmac import get_mac_address as gma

myID = '-1'
#mac address: DC:A6:32:D0:6F:55


#client.tls_set(certfile=,keyfile=, cert_reqs=ssl.CERT_REQUIRED)

#broker_address = '192.168.0.9'
broker_address = '192.168.0.12'
#roker_address = '10.0.2.15'
#broker_address = "127.0.0.1"

#How can I get this to be set remotely?
#Can mosquitto set tne ID for the sensor based on a request from the host?
BASE = f"hydromon/sensor/{myID}"

client = mqtt.Client(
    client_id=f"pub-{myID}",
    protocol=mqtt.MQTTv311,
    callback_api_version=mqtt.CallbackAPIVersion.VERSION2
)

logging.basicConfig(level=logging.DEBUG)
client.enable_logger()

def current_time():
    return datetime.datetime.now().isoformat()


client.will_set(f"{BASE}/status",
    json.dumps({"type": "status", "device_id": myID, "timestamp": current_time(), "value": "Offline (Unexpected)"}),
                qos=1,
                retain=True)

def this_on_disconnect(client, userdata, flags, reason_code, properties):
    client.publish(f"{BASE}/status", json.dumps({"type": "status", "device_id": myID, "timestamp": current_time(), "value": "Offline" }), qos=1, retain=True)
    try :
        startup()
    except :
        pass

def saveToken(client, userdata, msg):
    payload = msg.payload.decode("utf-8", errors="ignore")
    print(f'Payload in changeTopic: {payload}')
    jsonPayload = json.loads(payload)
    if (jsonPayload['id'] == myID):
        myToken = jsonPayload['token']
        with open("device_token", "w") as file:
            file.write(str(myToken))


def startup():
    client.username_pw_set(gma().lower(),  "TOKEN")
    client.on_connect = this_on_connect
    client.on_disconnect = this_on_disconnect
    client.connect(broker_address, 1883, keepalive=120)
    client.loop_start()
    retries = 0
    while (client.is_connected() == False):
        print("Polling for connection... ")
        time.sleep(0.1)
        retries = retries + 1
        if (retries > 100 or client.is_connected()):
            break
    if (client.is_connected()):
        print("client connected")


def shutdown():
    client.reconnect()

def changeTopic(client, userdata, msg):
    global myID
    global BASE
    payload = msg.payload.decode("utf-8", errors="ignore")
    print(f'Payload in changeTopic: {payload}')
    jsonPayload = json.loads(payload)
    #if mac address matches
    if (jsonPayload['target'].lower() == gma().lower()):
        print(f"Changing topic to: {jsonPayload['id']}")
        myID = jsonPayload['id']
        with open("device_id", "w") as file:
            file.write(str(myID))
    print(f'my ID is now: {myID}')
    BASE = f"hydromon/sensor/{myID}"

def getSensorId():
    print('subscribing...')
    client.on_message = changeTopic
    client.subscribe(f"hydromon/setup/{gma().lower()}", qos=2)




def this_on_connect(client, userdata, flags, reason_code, properties):
    print('connected...')
    msg = {"type": "status", "device_id": myID, "timestamp": current_time(), "value": "Online"}
    client.publish(f"{BASE}/status", json.dumps(msg), qos=1, retain=True)


def publish():
    #check for something to publish
    result = humiture.single()
    humidity, temperature = result #pull the two values out
    msg = {"reading": {"timestamp": datetime.datetime.now().isoformat(), "value": humidity}}

    #msg = {"type":"humidity", "device_id": myID, "timestamp": current_time(), "value": humidity}
    print(msg)
    #if yes, publish it
    try:
        client.publish(f"{BASE}/humidity", json.dumps(msg), qos=1, retain=True)
    except:
        print("exception occurred")
    msg = {"reading": {"timestamp": current_time(), "value": temperature}}
    #msg = {"type": "temperature", "device_id": myID, "timestamp": current_time(), "value": temperature}
    try:
        client.publish(f"{BASE}/temperature", json.dumps(msg), qos=1, retain=True)
    except:
        print("exception occurred")

if __name__ == '__main__':
    print('starting...')
    #client.on_connect = on_connect
    startup()
    getSensorId()
    try:
        while client.is_connected():
            publish()
    finally:
        shutdown()
    print("we shut down")