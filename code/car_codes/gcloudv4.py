import os
import io
import json
import time
import pickle
import webrtcvad
import subprocess
import numpy as np
import collections
import sounddevice as sd
import google.generativeai as genai

from pathlib import Path
from google.cloud import speech
from scipy.io.wavfile import write
from google.cloud import texttospeech
from google.oauth2 import service_account
from resemblyzer import VoiceEncoder, preprocess_wav
from tts import TextToSpeech

# Import notification system
try:
    from pub_message import send_custom_message
    NOTIFICATIONS_ENABLED = True
    print("Notifications enabled - will send status to UI")
except ImportError:
    NOTIFICATIONS_ENABLED = False
    print("pub_message.py not found - notifications disabled")

#activate stt key
client_file = "sa_speech_to_text.json"
credentials = service_account.Credentials.from_service_account_file(client_file)
stt_client = speech.SpeechClient(credentials=credentials)

encoder = VoiceEncoder()

#activate gemini
genai.configure(api_key="AIzaSyDY7d7mn1baR5vp04pozq4Nu5T_IWWyaAk")
gemini_model = genai.GenerativeModel("models/gemini-2.5-pro-preview-05-06")

def send_api_status(status, message):
    if not NOTIFICATIONS_ENABLED:
        return
    
    try:
        status_data = {
            "status": status,
            "message": message
        }
        success = send_custom_message("API_STATUS", status_data)
    except Exception as e:
        print(f"Voice status notification error: {e}")

def send_voice_detection(user, recognized):
    if not NOTIFICATIONS_ENABLED:
        return
    
    try:
        detection_data = {
            "user": user,
            "recognized": recognized
        }

        success = send_custom_message("VOICE_DETECTION", detection_data)
    except Exception as e:
        print(f"Voice detection notification error: {e}")

def Record(filename="audio.wav", samplerate=16000, frame_duration_ms=30):
    TextToSpeech("Sizi dinliyorum.")
    send_api_status("LISTENING", "Konuşma bekleniyor...")

    vad = webrtcvad.Vad(0) 
    frame_size = int(samplerate * frame_duration_ms / 1000)
    buffer_duration = 1  #saniye sonra kaydı durdur

    ring_buffer = collections.deque(maxlen=int(buffer_duration * 1000 / frame_duration_ms))
    voiced_frames = []
    triggered = False

    with sd.InputStream(samplerate=samplerate, channels=1, dtype="int16") as stream:
        while True:
            audio_frame, _ = stream.read(frame_size)
            audio_bytes = audio_frame.tobytes()

            if vad.is_speech(audio_bytes, samplerate):
                if not triggered:
                    send_api_status("LISTENING","Konuşma algılandı, kayıt başlatılıyor...")
                    send_api_status("USER_WAITING", "Kullanıcı tanımlaması bekleniyor...")
                    triggered = True
                    voiced_frames.extend(ring_buffer)
                    ring_buffer.clear()

                voiced_frames.append(audio_frame)
            else:
                if triggered:
                    ring_buffer.append(audio_frame)
                    if len(ring_buffer) == ring_buffer.maxlen:
                        send_api_status("PROCESSING", "Kayıt tamamlandı.")
                        break
                else:
                    ring_buffer.append(audio_frame)

    if voiced_frames:
        audio_np = np.concatenate(voiced_frames, axis=0)
        write(filename, samplerate, audio_np)
    else:
        send_api_status("ERROR", "Konuşma algılanamadı.")

def voice_rec(test_audio_path, embeddings_path="embeddings.pkl", threshold=0.65):
    with open(embeddings_path, "rb") as f:
        db = pickle.load(f)

    wav = preprocess_wav(test_audio_path)
    test_emb = encoder.embed_utterance(wav)

    #cosine benzerliği
    def cosine_sim(a, b):
        return np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b))

    send_api_status("USER_PROCESSING", "Kullanıcı tanımlaması yapılıyor...")
    best_score = -1
    best_user = None

    #üyeleri karşılaştır
    for user, emb in db.items():
        score = cosine_sim(test_emb, emb)
        print(f"Compared to {user}: score = {score:.3f}")
        if score > best_score:
            best_score = score
            best_user = user

    if best_score >= threshold:
        print(f"Speaker matched: {best_user} (score: {best_score:.3f})")
        TextToSpeech(f"Kullanıcı tanımlandı. Hoşgeldin {best_user}")
        send_voice_detection(best_user, True)
        return best_user, best_score
    else:
        print(f"Speaker not recognized (best score: {best_score:.3f})")
        TextToSpeech("Kullanıcı sisteme kayıtlı değil.")
        send_voice_detection("unknown", False)
        return "unknown", best_score

def SpeechToText(filename="audio.wav"):
    send_api_status("PROCESSING", "Kayıt metne dönüştürülüyor...")
    with io.open(filename, "rb") as audio_file:
        content = audio_file.read()
    audio = speech.RecognitionAudio(content=content)

    config = speech.RecognitionConfig(
        encoding=speech.RecognitionConfig.AudioEncoding.LINEAR16,
        sample_rate_hertz=16000,
        language_code="tr-TR",
        enable_automatic_punctuation=True
    )

    response = stt_client.recognize(config=config, audio=audio)
    print(response)

    transcript = [result.alternatives[0].transcript for result in response.results]
    text = " ".join(transcript)

    with open("denemev2.txt", "w",
 encoding="utf-8") as file2write:
        file2write.write(f"{text}\n")

    return text

def Gemi(command_text, output_file="commands.json"):
    send_api_status("PROCESSING", "Komut işleniyor...")
    TextToSpeech("Komutları işliyorum. Lütfen biraz bekleyin.")
    prompt = f"""
        Sen gömülü sistem ile çalışan mini otonom bir araçsın. 
        Kullanıcıdan alınan doğal dil komutlarını al ve onları JSON formatında yapısal komutlara çevir.
        Komutlar birden fazla ise sırasıyla liste olarak ver. Eğer bir komutu birden fazla kez yapman istenirse arka arkaya sırala.
        Sadece geçerli JSON döndür.

        Anlayış kuralları:
        -
        -komutları yüksek doğrulukla değil, esnek şekilde algıla.
        -telaafuz hataları, yazım yanlışları veya benzer sözcükleri düzeltmeye çalış.
        (düzgeç -> düzgit ya da sağdan -> sağa dön ya da sahadan -> sağa dön ya da soldan -> sola dön ya da düz -> düz git gibi)

        Kullanılabilecek komutlar:
        "komut": "ileri_git"|"geri_git"|"saga_don"|"sola_don"|"dur"|"tam_don"|"geriye_don"
        "kosul": "engel_algilayana_kadar"|"45_derece"|"5_saniye" gibi

        Örnek:
        Girdi: "engel görene kadar ilerle sonra sağa dön"
        Çıktı:
        [
          {{"komut": "ileri_git", "kosul": "engel_algilayana_kadar"}},
          {{"komut": "saga_don"}}
        ]

	Eğer yukarıda verilen komutların dışında aracın gerçekleştiremeyeceği herhangi bir komut verilirse; 
	Örneğin "renk tespiti yap.", "uç" bunu da şu şekilde JSON döndür.
        
	Örnek:
        Girdi: "renk tespiti yap"
        Çıktı:
        [
          {{"komut": "gecersiz_komut", "kosul": "renk tespiti"}},
        ]

        Şimdi şu komutu dönüştür:
        "{command_text}"
    """
    gemini_response = gemini_model.generate_content(prompt)
    print("Gemini says:", gemini_response.text)

    cleaned_output = gemini_response.text.strip()
    if cleaned_output.startswith("```") and cleaned_output.endswith("```"):
        lines = cleaned_output.splitlines()
        cleaned_output = "\n".join(lines[1:-1])

    try:
        commands = json.loads(cleaned_output)
        with open(output_file, "w", encoding="utf-8") as f:
            json.dump(commands, f, indent=4, ensure_ascii=False)

        send_api_status("BUSY", "Komutlar hazır, araç çalıştırılıyor...")
        return commands
    
    except json.JSONDecodeError as e:
        print("Gemini output is not valid JSON.")
        print("Error:", e)
        send_api_status("ERROR", "Komut işlenemedi.")
        return None

if __name__ == "__main__":
    TextToSpeech("Merhaba. Ben otonom mini araç Hamdi. Verdiğiniz komutları gerçekleştirmeye hazırım.")
    try:
        try:
            devices = sd.query_devices()
            send_api_status("LOG", "Mikrofon erişimi başarılı")
        except Exception as mic_error:
            send_api_status("LOG", f"Mikrofon hatası: {str(mic_error)}")
            raise
        
        send_api_status("PROCESSING", "Ses tanıma dosyası kontrol ediliyor...")
        if not os.path.exists("embeddings.pkl"):
            send_api_status("embeddings.pkl dosyası bulunamadı", "red")
            raise FileNotFoundError("embeddings.pkl not found")
        
        send_api_status("READY", "Sistem başlatıldı.")

        Record()
        while True:
            try:
                user, score = voice_rec("audio.wav")
                if user != "unknown":
                    print(f"Detected speaker: {user}")
                    command_text = SpeechToText()
                
                    if command_text:                        
                        send_api_status("BUSY", "Araç hareket halinde komut veremezsiniz")
                        commands_json = Gemi(command_text)

                        if commands_json:
                            subprocess.run(["python", "car_controlv2.py"])
                            
                            Record()

                        else:
                            print("Failed to parse commands.")
                            Record()

                    else:
                        print("Komut alınamadı.")
                        Record()

                else:
                    print("Speaker not recognized.")
                    Record()

                    time.sleep(1)

            except KeyboardInterrupt:
                send_api_status("STOPPED", "Sistem durduruldu.")
                break
            except Exception as e:
                send_api_status("ERROR", f"API hatası: {str(e)}")
                time.sleep(5)

    except Exception as e:
        error_msg = f"Kritik hata: {str(e)}"
        print(error_msg)
        send_api_status("ERROR", "Sistem başlatılamadı.")
        
        import traceback
        traceback.print_exc()
        
        try:
            send_api_status("ERROR", f"Hata detayı: {str(e)[:50]}...")
        except:
            pass
        
        raise