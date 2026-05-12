import socket
import struct
import numpy as np
from kokoro import KPipeline

HOST = '127.0.0.1'
PORT = 65432

pipeline = KPipeline(lang_code='b')


def recv_exact(conn, n):
    """Receive exactly n bytes or return None if connection closes."""
    data = b''
    while len(data) < n:
        packet = conn.recv(n - len(data))
        if not packet:
            return None
        data += packet
    return data


with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind((HOST, PORT))
    s.listen()

    print("Python server listening...")
    conn, addr = s.accept()

    with conn:
        print(f"Connected by {addr}")

        while True:
            # Step 1: read length (4 bytes)
            raw_len = recv_exact(conn, 4)
            if not raw_len:
                break

            message_length = struct.unpack('!I', raw_len)[0]

            # Step 2: read payload
            payload = recv_exact(conn, message_length)
            if not payload:
                break

            text = payload.decode("utf-8")
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
                response_bytes = audio.tobytes()
                response_len = struct.pack('!I', len(response_bytes))
                conn.sendall(response_len + response_bytes)
                print(f"Sent audio chunk {i} with {len(audio)} samples")

            