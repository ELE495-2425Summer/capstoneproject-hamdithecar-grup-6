#!/usr/bin/env python3
import sys
import os
from pub_message import send_file_update

# Read TXT and send its content to UI
def read_and_publish_txt(filepath):
    try:
        if not os.path.exists(filepath):
            print(f"Text file not found: {filepath}")
            return False
        
        with open(filepath, 'r', encoding='utf-8') as file:
            content = file.read()
        
        # Send content to UI
        success = send_file_update("TXT", filepath, content)
        
        if success:
            print(f"Successfully read and published TXT: {filepath}")
            return True
        else:
            print(f"Failed to publish TXT: {filepath}")
            return False
            
    except UnicodeDecodeError:
        print(f"File {filepath} is not a valid text file (encoding issue)")
        return False
    except Exception as e:
        print(f"Error reading text file {filepath}: {e}")
        return False

if __name__ == "__main__":
    if len(sys.argv) != 2:
        sys.exit(1)
    
    filepath = sys.argv[1]
    success = read_and_publish_txt("/home/doua/projects/gcloud/denemev2.txt")
    sys.exit(0 if success else 1)