#!/usr/bin/env python3
import json
import sys
import os
from pub_message import send_file_update

# Read JSON and send its content to UI
def read_and_publish_json(filepath):
    try:
        if not os.path.exists(filepath):
            print(f"JSON file not found: {filepath}")
            return False
        
        with open(filepath, 'r', encoding='utf-8') as file:
            data = json.load(file)
        
        formatted_json = json.dumps(data, indent=2)
        
        # Send content to UI
        success = send_file_update("JSON", filepath, formatted_json)
        
        if success:
            print(f"Successfully read and published JSON: {filepath}")
            return True
        else:
            print(f"Failed to publish JSON: {filepath}")
            return False
            
    except json.JSONDecodeError as e:
        print(f"Invalid JSON in {filepath}: {e}")
        return False
    except Exception as e:
        print(f"Error reading JSON file {filepath}: {e}")
        return False

if __name__ == "__main__":
    if len(sys.argv) != 2:
        sys.exit(1)
    
    filepath = sys.argv[1]
    success = read_and_publish_json("/home/doua/projects/gcloud/commands.json")
    sys.exit(0 if success else 1)