#!/usr/bin/env python3
import os
import sys
import time
import subprocess
import logging
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)

class FileChangeHandler(FileSystemEventHandler):
    def __init__(self, json_file, txt_file):
        self.json_file = os.path.abspath(json_file)
        self.txt_file = os.path.abspath(txt_file)
        
        logging.info(f"File monitor initialized")
        logging.info(f"Monitoring JSON: {self.json_file}")
        logging.info(f"Monitoring TXT: {self.txt_file}")
        
        # Check if files exist
        if not os.path.exists(self.json_file):
            logging.warning(f"JSON file does not exist: {self.json_file}")
        if not os.path.exists(self.txt_file):
            logging.warning(f"TXT file does not exist: {self.txt_file}")
    
    # Handle file modification events
    def on_modified(self, event):
        if event.is_directory:
            return
        
        file_path = os.path.abspath(event.src_path)
        
        # Check if this is one of our monitored files
        if file_path == self.json_file:
            logging.info(f"JSON file changed: {file_path}")
            self.handle_json_change(file_path)
        
        elif file_path == self.txt_file:
            logging.info(f"TXT file changed: {file_path}")
            self.handle_txt_change(file_path)
    
    # Handle file creation events
    def on_created(self, event):
        if event.is_directory:
            return
        
        file_path = os.path.abspath(event.src_path)
        
        if file_path == self.json_file:
            logging.info(f"JSON file created: {file_path}")
            self.handle_json_change(file_path)
        
        elif file_path == self.txt_file:
            logging.info(f"TXT file created: {file_path}")
            self.handle_txt_change(file_path)
    
    # Handle JSON file changes 
    def handle_json_change(self, filepath):
        try:
            logging.info(f"Calling JSON reader for: {filepath}")
            
            # Call read_json.py as separate process
            result = subprocess.run([
                sys.executable, 'read_json.py', filepath
            ], capture_output=True, text=True, timeout=15)
            
            if result.returncode == 0:
                logging.info(f"JSON reader completed successfully")
                if result.stdout.strip():
                    logging.info(f"JSON reader output: {result.stdout.strip()}")
            else:
                logging.error(f"JSON reader failed with return code {result.returncode}")
                if result.stderr.strip():
                    logging.error(f"JSON reader error: {result.stderr.strip()}")
        
        except subprocess.TimeoutExpired:
            logging.error(f"JSON reader timed out after 15 seconds")
        except FileNotFoundError:
            logging.error(f"read_json.py not found in current directory")
        except Exception as e:
            logging.error(f"Error calling JSON reader: {e}")
    
    # Handle TXT file changes
    def handle_txt_change(self, filepath):
        try:
            logging.info(f"Calling TXT reader for: {filepath}")
            
            # Call read_txt.py as separate process
            result = subprocess.run([
                sys.executable, 'read_txt.py', filepath
            ], capture_output=True, text=True, timeout=15)
            
            if result.returncode == 0:
                logging.info(f"TXT reader completed successfully")
                if result.stdout.strip():
                    logging.info(f"TXT reader output: {result.stdout.strip()}")
            else:
                logging.error(f"TXT reader failed with return code {result.returncode}")
                if result.stderr.strip():
                    logging.error(f"TXT reader error: {result.stderr.strip()}")
        
        except subprocess.TimeoutExpired:
            logging.error(f"TXT reader timed out after 15 seconds")
        except FileNotFoundError:
            logging.error(f"read_txt.py not found in current directory")
        except Exception as e:
            logging.error(f"Error calling TXT reader: {e}")

def validate_files(json_file, txt_file):
    """Validate that files exist and are accessible"""
    issues = []
    
    if not os.path.exists(json_file):
        issues.append(f"JSON file not found: {json_file}")
    elif not os.access(json_file, os.R_OK):
        issues.append(f"JSON file not readable: {json_file}")
    
    if not os.path.exists(txt_file):
        issues.append(f"TXT file not found: {txt_file}")
    elif not os.access(txt_file, os.R_OK):
        issues.append(f"TXT file not readable: {txt_file}")
    
    return issues

def main():
    """Main file monitor function"""
    if len(sys.argv) != 3:
        sys.exit(1)
    
    json_file = sys.argv[1]
    txt_file = sys.argv[2]
    
    # Validate files
    issues = validate_files(json_file, txt_file)
    if issues:
        logging.warning("File validation issues found:")
        for issue in issues:
            logging.warning(f"  - {issue}")
        logging.info("Continuing anyway - files may be created later")
    
    # Validate required scripts exist
    if not os.path.exists('read_json.py'):
        logging.error("read_json.py not found in current directory")
        sys.exit(1)
    
    if not os.path.exists('read_txt.py'):
        logging.error("read_txt.py not found in current directory")
        sys.exit(1)
    
    # Setup file watcher
    event_handler = FileChangeHandler(json_file, txt_file)
    observer = Observer()
    
    # Watch the directories containing the files
    json_dir = os.path.dirname(json_file)
    txt_dir = os.path.dirname(txt_file)
    
    # Add watches for directories
    observer.schedule(event_handler, json_dir, recursive=False)
    if txt_dir != json_dir:
        observer.schedule(event_handler, txt_dir, recursive=False)
    
    logging.info(f"Watching directory: {json_dir}")
    if txt_dir != json_dir:
        logging.info(f"Watching directory: {txt_dir}")
    
    # Start monitoring
    observer.start()
    logging.info("File monitoring started successfully")
    logging.info("Press Ctrl+C to stop monitoring")
    
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        logging.info("Received interrupt signal, stopping file monitor...")
        observer.stop()
    except Exception as e:
        logging.error(f"Unexpected error in file monitor: {e}")
        observer.stop()
    
    # Wait for observer to finish
    observer.join()
    logging.info("File monitor stopped")

if __name__ == "__main__":
    main()