#!/usr/bin/env python3
import zmq
import json
import time
import threading
import queue
import logging

# Setup logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(message)s')

class PubServer:
    def __init__(self, pub_port=5566, internal_port=5567):
        self.pub_port = pub_port
        self.internal_port = internal_port
        self.running = False
        self.message_queue = queue.Queue()
        
        # Create ZMQ context
        self.context = zmq.Context()
        
        # PUB socket for sending to UI
        self.pub_socket = self.context.socket(zmq.PUB)
        self.pub_socket.bind(f"tcp://*:{pub_port}")
        logging.info(f"PUB server bound to port {pub_port}")
        
        # PULL socket for receiving internal messages
        self.pull_socket = self.context.socket(zmq.PULL)
        self.pull_socket.bind(f"tcp://*:{internal_port}")
        logging.info(f"Internal PULL server bound to port {internal_port}")
    
    def start(self):
        """Start the pub server"""
        self.running = True
        logging.info("PUB server started")
        
        while self.running:
            try:
                # Check for internal messages
                if self.pull_socket.poll(100):  # 100ms timeout
                    message = self.pull_socket.recv_string(zmq.NOBLOCK)
                    logging.info(f"Received internal message")
                    
                    # Forward to UI
                    self.pub_socket.send_string(message)
                    logging.info(f"Forwarded message to UI")
                
            except zmq.Again:
                continue
            except KeyboardInterrupt:
                logging.info("Shutting down PUB server...")
                break
            except Exception as e:
                logging.error(f"PUB server error: {e}")
    
    def stop(self):
        """Stop the server"""
        self.running = False
        self.pub_socket.close()
        self.pull_socket.close()
        self.context.term()
        logging.info("PUB server stopped")

if __name__ == "__main__":
    server = PubServer()
    try:
        server.start()
    finally:
        server.stop()