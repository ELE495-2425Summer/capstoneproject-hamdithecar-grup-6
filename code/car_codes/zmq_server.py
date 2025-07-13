#!/usr/bin/env python3
import zmq
import logging
import subprocess
import os
import signal
import psutil
import time

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)

class ZMQServer:
    def __init__(self, port=5565):
        self.context = zmq.Context()
        self.socket = self.context.socket(zmq.REP)
        self.socket.bind(f"tcp://*:{port}")
        self.running = False
        self.gcloud_process = None
        self.car_process = None
        self.last_ping_time = time.time()  # Track last client communication
        logging.info(f"ZeroMQ REQ-REP server listening on port {port}")
        logging.info("Server handles PING-PONG and START commands")
    
    # Kill API and car movement codes
    def stop_all_processes(self):
        try:
            # Stop gcloudv4.py
            self.stop_gcloud_api()
            
            # Stop car_controlv2.py
            self.stop_car_processes()
            
            logging.info("All car processes stopped")
            
        except Exception as e:
            logging.error(f"Error stopping processes: {e}")
    
    def stop_car_processes(self):
        try:
            # Kill any car control code
            import subprocess
            result = subprocess.run([
                "pkill", "-f", "car_controlv2.py"
            ], capture_output=True, text=True)
            
            if result.returncode == 0:
                logging.info("car_controlv2.py processes killed")
            else:
                logging.info("No car_controlv2.py processes found to kill")
                
        except Exception as e:
            logging.error(f"Error killing car processes: {e}")
    
    # Check if API code is running
    def is_gcloud_running(self):
        try:
            if self.gcloud_process and self.gcloud_process.poll() is None:
                return True
            return False
        except:
            return False
    
    def start_gcloud_api(self):
        try:
            # Stop existing process if running
            self.stop_gcloud_api()
            
            # Change to project directory
            project_dir = "/home/doua/projects/gcloud"
            
            # Command with full environment setup
            cmd = [
                "sudo", "-u", "doua",  # Run as user doua
                "bash", "-c", 
                f"cd {project_dir} && " +
                f"export DISPLAY=:0 && " +  # Set display for audio
                f"export PULSE_RUNTIME_PATH=/run/user/1000/pulse && " +  # PulseAudio
                f"source bin/activate && " +
                f"export GOOGLE_APPLICATION_CREDENTIALS='{project_dir}/your-key-file.json' && " +
                f"export PYTHONPATH='{project_dir}' && " +
                f"python3 gcloudv4.py"
            ]
            
            # Start the process with full user environment
            env = os.environ.copy()
            env.update({
                'HOME': '/home/doua',
                'USER': 'doua',
                'DISPLAY': ':0',
                'PULSE_RUNTIME_PATH': '/run/user/1000/pulse',
                'XDG_RUNTIME_DIR': '/run/user/1000',
                'GOOGLE_APPLICATION_CREDENTIALS': f"{project_dir}/your-key-file.json",
                'PYTHONPATH': project_dir
            })
            
            self.gcloud_process = subprocess.Popen(
                cmd,
                cwd=project_dir,
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,
                env=env,
                preexec_fn=os.setsid
            )
            
            logging.info(f"Started gcloudv4.py with PID: {self.gcloud_process.pid}")
            return True
            
        except Exception as e:
            logging.error(f"Failed to start gcloudv4.py: {e}")
            return False
    
    def stop_gcloud_api(self):
        """Stop gcloudv4.py process"""
        try:
            if self.gcloud_process:
                # Kill the process group
                os.killpg(os.getpgid(self.gcloud_process.pid), signal.SIGTERM)
                self.gcloud_process.wait(timeout=5)
                logging.info("Stopped gcloudv4.py process")
        except:
            pass
        finally:
            self.gcloud_process = None
    
    def process_message(self, message):
        """Process incoming REQ-REP messages"""
        try:
            # Handle PING-PONG for connection health
            if message == "PING":
                self.last_ping_time = time.time()  # Update last communication time
                return "PONG"
            
            # Handle START command
            elif message == "START":
                if self.is_gcloud_running():
                    logging.info("gcloudv4.py already running")
                    return "ALREADY_RUNNING"
                else:
                    success = self.start_gcloud_api()
                    if success:
                        logging.info("Started gcloudv4.py successfully")
                        return "STARTED"
                    else:
                        logging.error("Failed to start gcloudv4.py")
                        return "START_FAILED"
            
            # Handle STATUS check
            elif message == "STATUS":
                if self.is_gcloud_running():
                    return "RUNNING"
                else:
                    return "STOPPED"
            
            # Handle connection status
            elif message == "CONNECT":
                logging.info("Client connected")
                return "CONNECTED"
            
            # Handle DISCONNECT command
            elif message == "DISCONNECT":
                logging.info("Client disconnected - stopping all car processes")
                self.stop_all_processes()
                return "DISCONNECTED"
            
            # All other messages
            else:
                logging.info(f"Received unknown message: {message}")
                return "OK"
        
        except Exception as e:
            logging.error(f"Error processing message: {e}")
            return f"ERROR: {str(e)}"
    
    def start(self):
        """Start the REQ-REP server loop"""
        self.running = True
        logging.info("REQ-REP server started (PING-PONG + START commands)")
        
        while self.running:
            try:
                if self.socket.poll(1000):  # 1 second timeout
                    message = self.socket.recv_string(zmq.NOBLOCK)
                    logging.info(f"Received: {message}")
                    
                    response = self.process_message(message)
                    self.socket.send_string(response)
                    logging.info(f"Sent: {response}")
                else:
                    # Check if processes are still running when idle
                    if self.gcloud_process and self.gcloud_process.poll() is not None:
                        logging.info("gcloudv4.py process ended, cleaning up")
                        self.gcloud_process = None
                    
                    # Check for client timeout (no PING for 30 seconds = client disconnected)
                    current_time = time.time()
                    if (current_time - self.last_ping_time) > 30:
                        if self.gcloud_process or self.car_process:
                            logging.info("Client timeout detected - stopping all processes")
                            self.stop_all_processes()
                            self.last_ping_time = current_time  # Reset timer
                        
            except zmq.Again:
                continue
            except KeyboardInterrupt:
                logging.info("Shutting down server...")
                break
            except Exception as e:
                logging.error(f"Server error: {e}")
                try:
                    self.socket.send_string(f"ERROR: {str(e)}")
                except:
                    pass
                    
    def stop(self):
        """Stop the server"""
        self.running = False
        self.stop_all_processes()
        self.socket.close()
        self.context.term()
        logging.info("Server stopped")

if __name__ == "__main__":
    server = ZMQServer()
    try:
        server.start()
    finally:
        server.stop()