import os
import io
import subprocess
import sounddevice as sd
from pydub import AudioSegment
from google.cloud import texttospeech

#activating tts key
os.environ['GOOGLE_APPLICATION_CREDENTIALS'] = 'your-tts-key-file.json'
tts_client = texttospeech.TextToSpeechClient()

sd.default.latency = 'high'
sd.default.blocksize = 2048

def TextToSpeech(command_text, save_as="tts_output.wav"):
    synthesis_input = texttospeech.SynthesisInput(text=command_text)

    voice = texttospeech.VoiceSelectionParams(
        language_code="tr-TR",
        name="tr-TR-Chirp3-HD-Erinome"
    )

    audio_config = texttospeech.AudioConfig(
        audio_encoding=texttospeech.AudioEncoding.MP3,
        effects_profile_id=['small-bluetooth-speaker-class-device'],
        speaking_rate=1
    )

    response = tts_client.synthesize_speech(
        input=synthesis_input,
        voice=voice,
        audio_config=audio_config
    )

    mp3_audio = AudioSegment.from_file(io.BytesIO(response.audio_content), format="mp3")
    mp3_audio.export(save_as, format="wav")
    print(f"Saved TTS as: {save_as}")

    try:
        subprocess.run(["aplay", save_as], check=True)
    except subprocess.CalledProcessError as e:
        print(f"Error playing audio with aplay: {e}")
