#!/usr/bin/env python3
import zmq
import json
import time
import sys

def send_to_pub_server(message, internal_port=5567):
    try:
        context = zmq.Context()
        socket = context.socket(zmq.PUSH)
        socket.connect(f"tcp://localhost:{internal_port}")
        
        # Send message to pub server
        socket.send_string(message)
        print(f"Sent message to pub server")
        
        socket.close()
        context.term()
        return True
        
    except Exception as e:
        print(f"Error sending to pub server: {e}")
        return False

def send_file_update(file_type, filepath, content):
    try:
        message = {
            "type": "FILE_UPDATE",
            "file_type": file_type,
            "result": {
                "success": True,
                "data": content,
                "filepath": filepath
            },
            "timestamp": time.time()
        }
        
        json_message = json.dumps(message)
        success = send_to_pub_server(json_message)
        
        if success:
            print(f"Published {file_type} update: {filepath}")
            return True
        else:
            print(f"Failed to publish {file_type} update: {filepath}")
            return False
        
    except Exception as e:
        print(f"Error creating file update message: {e}")
        return False

def send_custom_message(msg_type, data):
    try:
        message = {
            "type": msg_type,
            "data": data,
            "timestamp": time.time()
        }
        
        json_message = json.dumps(message)
        success = send_to_pub_server(json_message)
        
        if success:
            print(f"Published message: {msg_type}")
            return True
        else:
            print(f"Failed to publish message: {msg_type}")
            return False
        
    except Exception as e:
        print(f"Error creating custom message: {e}")
        return False

if __name__ == "__main__":
    if len(sys.argv) >= 4 and sys.argv[1] == "file":
        file_type = sys.argv[2]
        filepath = sys.argv[3]
        content = sys.argv[4] if len(sys.argv) > 4 else ""
        send_file_update(file_type, filepath, content)
    
    elif len(sys.argv) >= 3 and sys.argv[1] == "custom":
        msg_type = sys.argv[2]
        data = sys.argv[3] if len(sys.argv) > 3 else ""
        send_custom_message(msg_type, data)