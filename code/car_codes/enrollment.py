from resemblyzer import VoiceEncoder, preprocess_wav
from pathlib import Path
import numpy as np
import pickle

encoder = VoiceEncoder()

def enroll_speakers(enroll_dir="enrollments", save_path="embeddings.pkl"):
    db = {}
    enroll_path = Path(enroll_dir)
    
    for user_dir in enroll_path.iterdir():
        if user_dir.is_dir():
            embeddings = []
            audio_files = list(user_dir.glob("*.m4a")) + list(user_dir.glob("*.mp4")) + list(user_dir.glob("*.mp3"))
            for audio_file in audio_files:
                print(f"Enrolling {audio_file.name} for {user_dir.name}")
                wav = preprocess_wav(audio_file)
                emb = encoder.embed_utterance(wav)
                embeddings.append(emb)
            if embeddings:
                db[user_dir.name] = np.mean(embeddings, axis=0)
    
    with open(save_path, "wb") as f:
        pickle.dump(db, f)
    print(f"Enrollments saved to {save_path}")


enroll_speakers("enrollments")
