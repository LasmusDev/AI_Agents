import sounddevice as sd
import numpy as np
from kokoro import KPipeline

# Initialize pipeline
pipeline = KPipeline(lang_code='b')  # change language if needed

def stream_tts(text):
    stream = pipeline(
        text,
        voice='af_heart',   # example voice
        speed=1.0,
        split_pattern=r'\n+'
    )

    for i, (graphemes, phonemes, audio) in enumerate(stream):
        if audio is None:
            continue

        # Ensure numpy float32
        audio = np.array(audio, dtype=np.float32)

        # Play chunk immediately (streaming)
        sd.play(audio, samplerate=24000)
        sd.wait()

if __name__ == "__main__":
    while True:
        text = input("Enter text: ")
        stream_tts(text)