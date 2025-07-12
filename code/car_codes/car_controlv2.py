import RPi.GPIO as GPIO
import tempfile
import pygame
import time
import json
import io
import os
import subprocess
import numpy as np
import sounddevice as sd
from tts import TextToSpeech
from datetime import datetime
from pydub import AudioSegment
from pydub.playback import play
from google.cloud import texttospeech

# Import our notification system
try:
    from pub_message import send_custom_message, send_file_update
    NOTIFICATIONS_ENABLED = True
except ImportError:
    NOTIFICATIONS_ENABLED = False

# --- PIN DEFINITIONS ---
# Motors (L298N)
IN1 = 17  # Left motor forward
IN2 = 18  # Left motor backward
IN3 = 22  # Right motor forward
IN4 = 23  # Right motor backward
ENA = 12  # Left motor PWM (speed)
ENB = 13  # Right motor PWM (speed)

# Ultrasonic sensors (HC-SR04)
TRIG_FRONT = 5
ECHO_FRONT = 6
TRIG_BACK = 9
ECHO_BACK = 10

# --- GPIO SETUP ---
GPIO.setwarnings(False)
GPIO.setmode(GPIO.BCM)

motor_pins = [IN1, IN2, IN3, IN4, ENA, ENB]
for pin in motor_pins:
    GPIO.setup(pin, GPIO.OUT)
    GPIO.output(pin, GPIO.LOW)

# Setup for additional ultrasonic sensors
ultrasonic_sensors = [
    (TRIG_FRONT, ECHO_FRONT),  
    (TRIG_BACK, ECHO_BACK)     
]

GPIO.setup(TRIG_FRONT, GPIO.OUT)
GPIO.setup(ECHO_FRONT, GPIO.IN)
GPIO.setup(TRIG_BACK, GPIO.OUT)
GPIO.setup(ECHO_BACK, GPIO.IN)

# --- PWM SETUP ---
pwm_freq = 1000  # Hz
pwm_left = GPIO.PWM(ENA, pwm_freq)
pwm_right = GPIO.PWM(ENB, pwm_freq)
pwm_left.start(0)
pwm_right.start(0)

# --- NOTIFICATION FUNCTIONS ---
def send_car_status(action):
    if not NOTIFICATIONS_ENABLED:
        return
    
    try:
        status_data = {
            "action": action
        }
        
        success = send_custom_message("CAR_STATUS", status_data)
        if success:
            print(f"Status sent to UI: {action}")
        else:
            print(f"Failed to send status: {action}")
    except Exception as e:
        print(f"Notification error: {e}")

# --- MOVEMENT FUNCTIONS ---
def set_speed(duty):
    pwm_left.ChangeDutyCycle(duty)
    pwm_right.ChangeDutyCycle(duty)

def move_forward(speed):
    set_speed(speed)
    GPIO.output(IN1, GPIO.HIGH)
    GPIO.output(IN2, GPIO.LOW)
    GPIO.output(IN3, GPIO.HIGH)
    GPIO.output(IN4, GPIO.LOW)

def move_backward(speed):
    set_speed(speed)
    GPIO.output(IN1, GPIO.LOW)
    GPIO.output(IN2, GPIO.HIGH)
    GPIO.output(IN3, GPIO.LOW)
    GPIO.output(IN4, GPIO.HIGH)

def turn_left(speed):
    set_speed(speed)
    GPIO.output(IN1, GPIO.LOW)
    GPIO.output(IN2, GPIO.HIGH)
    GPIO.output(IN3, GPIO.HIGH)
    GPIO.output(IN4, GPIO.LOW)

def turn_right(speed):
    set_speed(speed)
    GPIO.output(IN1, GPIO.HIGH)
    GPIO.output(IN2, GPIO.LOW)
    GPIO.output(IN3, GPIO.LOW)
    GPIO.output(IN4, GPIO.HIGH)

def stop():
    set_speed(0)
    GPIO.output(IN1, GPIO.LOW)
    GPIO.output(IN2, GPIO.LOW)
    GPIO.output(IN3, GPIO.LOW)
    GPIO.output(IN4, GPIO.LOW)
    
def measure_distance(trig_pin, echo_pin):
    GPIO.output(trig_pin, False)
    time.sleep(0.01)  
    GPIO.output(trig_pin, True)
    time.sleep(0.00001)  
    GPIO.output(trig_pin, False)

    start = time.time()
    timeout = start + 0.04 

    while GPIO.input(echo_pin) == 0:
        start = time.time()
        if start > timeout:
            return None

    stop = time.time()
    while GPIO.input(echo_pin) == 1:
        stop = time.time()
        if stop > timeout:
            return None

    duration = stop - start
    distance = (duration * 34300) / 2 #cm dönüşümü
    return round(distance, 2)

def get_adaptive_speed_front(max_speed=85, min_speed=30, slow_distance=40, stop_distance=45):
    """Calculate speed based on front obstacle distance."""
    dist = measure_distance(TRIG_FRONT, ECHO_FRONT)

    # Sensör okumadıysa ya da çok saçma düşük bir değer verdiyse: maksimum hızla devam et
    if dist is None or dist < 5:
        return max_speed

    if dist <= stop_distance:
        return 0  # Çok yakın engel → dur

    if dist > slow_distance:
        return max_speed  # Uzakta engel yok → tam hız

    # Engel orta mesafedeyse → lineer hız ayarla
    speed_range = max_speed - min_speed
    distance_range = slow_distance - stop_distance
    ratio = (dist - stop_distance) / distance_range
    speed = min_speed + ratio * speed_range

    return int(speed)

def get_adaptive_speed_back(max_speed=85, min_speed=30, slow_distance=40, stop_distance=45):
    dist = measure_distance(TRIG_BACK, ECHO_BACK)

    if dist is None:
        return max_speed 

    if dist <= stop_distance:
        return 0  # Too close → stop

    if dist > slow_distance:
        return max_speed  # Far enough → full speed

    speed_range = max_speed - min_speed
    distance_range = slow_distance - stop_distance
    ratio = (dist - stop_distance) / distance_range
    speed = min_speed + ratio * speed_range

    return int(speed)

# --- MAIN COMMAND EXECUTION ---
def execute_commands(commands):
    # Send initial status
    send_car_status("Araç harekete başlıyor.")
    send_car_status(f"Toplam {len(commands)} komut çalıştırılacak.")
    
    for idx, command in enumerate(commands):
        print(f"\n[Command {idx+1}] {command}")

        komut = command.get("komut")

        if komut == "ileri_git":
            send_car_status("Araç ileri gidiyor.")
            kosul = command.get("kosul")
            
            if kosul == "engel_algilayana_kadar":
                print("Moving forward with adaptive speed until obstacle detected...")
                send_car_status("Engele yaklaşılıyor araç ileri yönde yavaşlıyor.")
                TextToSpeech("Engel algılayana kadar ileri gidiyorum.")
                
                while True:
                    speed = get_adaptive_speed_front()
                    if speed == 0:
                        stop()
                        print("Front obstacle too close, stopping.")
                        send_car_status("Ön tarafta çok yakında engel algılandı araç duruyor.")
                        break

                    else:
                        move_forward(speed=speed)
                        if speed < 100:  # Only print when slowing down
                            print(f"Slowing down to speed {speed}")               
                        time.sleep(0.2)
                stop()
            
            elif kosul and "_saniye" in kosul:
                try:
                    duration = int(kosul.split('_')[0])
                    print(f"Moving forward for {duration} seconds...")
                    send_car_status(f"Araç {duration} saniye ileri gidiyor.")
                    TextToSpeech(f"{duration} saniye ileri gidiyorum.")
                    move_forward(speed=100)
                    time.sleep(duration)
                    stop()
                except:
                    print("Invalid duration format, using default 1s")
                    move_forward(speed=100)
                    time.sleep(1)
                    stop()
            
            else:
                print("Moving forward (default 1s)...")
                TextToSpeech("İleri gidiyorum.")
                move_forward(speed=100)
                time.sleep(1)
                stop()

        elif komut == "geri_git":
            send_car_status("Araç geri gidiyor.")
            kosul = command.get("kosul")
            
            if kosul == "engel_algilayana_kadar":
                print("Moving backward with adaptive speed until obstacle detected...")
                send_car_status("Engele yaklaşılıyor araç geri yönde yavaşlıyor.")
                TextToSpeech("Engel algılayana kadar geri gidiyorum.")
                
                while True:
                    speed = get_adaptive_speed_back()
                    if speed == 0:
                        stop()
                        print("Back obstacle too close, stopping.")
                        send_car_status("Arka tarafta çok yakında engel algılandı araç duruyor.")
                        break
                    else:
                        move_backward(speed=speed)
                        if speed < 100:  # Only print when slowing down
                            print(f"Slowing down to speed {speed}")
                        time.sleep(0.2)
                stop()
            
            elif kosul and "_saniye" in kosul:
                try:
                    duration = int(kosul.split('_')[0])
                    print(f"Moving backward for {duration} seconds...")
                    TextToSpeech(f"{duration} saniye geri gidiyorum.")
                    send_car_status(f"Araç {duration} saniye geri gidiyor.")
                    move_backward(speed=100)
                    time.sleep(duration)
                    stop()
                except:
                    print("Invalid duration format, using default 1s")
                    move_backward(speed=100)
                    time.sleep(1)
                    stop()
            
            else:
                print("Moving backward (default 1s)...")
                TextToSpeech("Geriye gidiyorum.")
                move_backward(speed=100)
                time.sleep(1)
                stop()

        elif komut == "sola_don":
            kosul = command.get("kosul")

            if kosul and "_derece" in kosul:
                try:
                    angle = float(kosul.split('_')[0])
                    duration = (angle/ 90) * 0.28 #90 derecenin 0.28 saniye olduğu varsayılırsa
                    print(f"Turning left for {angle}° ")
                    send_car_status(f"Araç {int(angle)} derece sola dönüyor.")
                    TextToSpeech(f"{int(angle)} derece sola dönüyorum.")
                    turn_left(speed=100)
                    time.sleep(duration)
                    stop()
                except:
                    print("Invalid angle format, using default 0.35s")
                    send_car_status(f"Araç sola dönüyor.")
                    TextToSpeech("Sola dönüyorum.")
                    turn_left(speed=100)
                    time.sleep(0.28)
                    stop()

            elif kosul and "_saniye" in kosul:
                # Format: "1_saniye"
                try:
                    duration = float(kosul.split('_')[0])
                    print(f"Turning left for {duration} seconds...")
                    send_car_status(f"Araç {duration} saniye sola dönüyor.")
                    TextToSpeech(f"{duration} saniye sola dönüyorum.")
                    turn_left(speed=100)
                    time.sleep(duration)
                    stop()
                except:
                    send_car_status(f"Araç sola dönüyor.")
                    TextToSpeech("Sola dönüyorum.")
                    turn_left(speed=100)
                    time.sleep(0.28)
                    stop()
            else:
                send_car_status(f"Araç sola dönüyor.")
                TextToSpeech("Sola dönüyorum")
                turn_left(speed=100)
                time.sleep(0.28)
                stop()

        elif komut == "saga_don":
            kosul = command.get("kosul")

            if kosul and "_derece" in kosul:
                try:
                    angle = float(kosul.split('_')[0])
                    duration = (angle/ 90) * 0.28 #90 derecenin 0.7 saniye olduğu varsayılırsa
                    print(f"Turning right for {angle}° ")
                    send_car_status(f"Araç {int(angle)} derece sağa dönüyor.")
                    TextToSpeech(f"{int(angle)} derece sağa dönüyorum.")
                    turn_right(speed=100)
                    time.sleep(duration)
                    stop()
                except:
                    send_car_status(f"Araç sağa dönüyor.")
                    TextToSpeech("Sağa dönüyorum.")
                    turn_right(speed=100)
                    time.sleep(0.28)
                    stop()
            
            elif kosul and "_saniye" in kosul:
                try:
                    duration = float(kosul.split('_')[0])
                    print(f"Turning right for {duration} seconds...")
                    send_car_status(f"Araç {duration} saniye sağa dönüyor.")
                    TextToSpeech(f"{duration} saniye sağa dönüyorum.")
                    turn_right(speed=100)
                    time.sleep(duration)
                    stop()
                except:
                    print("Invalid duration format, using default 0.7s")
                    send_car_status(f"Araç sağa dönüyor.")
                    TextToSpeech("sağa dönüyorum.")
                    turn_right(speed=100)
                    time.sleep(0.28)
                    stop()
            else:
                send_car_status(f"Araç sağa dönüyor")
                TextToSpeech("Sağa dönüyorum.")
                turn_right(speed=100)
                time.sleep(0.28)
                stop()

        elif komut == "dur":
            kosul = command.get("kosul")
            if kosul and "_saniye" in kosul:
                try:
                    duration = int(kosul.split('_')[0])
                    print(f"Stopping for {duration} seconds...")
                    send_car_status(f"Araç {duration} saniye duruyor.")
                    TextToSpeech(f"{duration} saniye duruyorum.")
                    stop()
                    time.sleep(duration)
                except:
                    print("Invalid duration format for stop command, using default 1s")
                    send_car_status("Araç duruyor.")
                    TextToSpeech("Duruyorum.")
                    stop()
                    time.sleep(1)
            else:
                TextToSpeech("Duruyorum.")
                send_car_status("Araç duruyor.")
                stop()
                time.sleep(1)

        elif komut == "tam_don":
            TextToSpeech("Etrafımda tam tur dönüyorum.")
            send_car_status("Araç 360 derece tam tur dönüyor.")
            turn_right(speed=100)
            time.sleep(1)  
            stop()

        elif komut == "geriye_don" :
            TextToSpeech("Geriye dönüyorum.")
            send_car_status("Araç 180 derece geriye dönüyor.")
            turn_right(speed=100)
            time.sleep(0.53)
            stop()


        else:
            print(f"Unknown command: {komut}, skipping.")
            kosul = command.get("kosul")
            TextToSpeech(f"{kosul} komutunu gerçekleştiremem.")
            send_car_status(f"Gerçekleştirilemeyen komut: {kosul}")

        time.sleep(0.5)

    send_car_status(f"Tüm komutlar tamamlandı.")
    TextToSpeech("Komutlar tamamlandı. Yeni komut almaya hazırım.")

# --- MAIN PROGRAM ---
try:
    
    # Load commands from JSON file (UTF-8 encoding)
    try:
        with open('commands.json', 'r', encoding='utf-8') as f:
            commands = json.load(f)
    except FileNotFoundError:
        print("commands.json file not found!")
        commands = []
    except json.JSONDecodeError:
        print("Invalid JSON format in commands.json!")
        commands = []

    if commands:
        print("\nStarting command execution sequence...")
        execute_commands(commands)
        print("\nAll commands executed.")
    else:
        print("No commands to execute.")
        send_car_status("Çalıştırılacak komut bulunamadı.")

finally:
    # Clear the commands.json file
    try:
        with open("commands.json", "w", encoding="utf-8") as f:
            f.write("[]")
        print("commands.json cleared.")
    except Exception as e:
        print(f"Failed to clear commands.json: {e}")

    # Cleanup GPIO and PWM
    pwm_left.stop()
    pwm_right.stop()
    GPIO.cleanup()
    print("GPIO cleanup done.")
