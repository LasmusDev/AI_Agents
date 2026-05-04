import socket
import struct
import TTSServer

HOST = '127.0.0.1'
PORT = 65432


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
            TTSServer.stream_tts(text)

            # Process
            word_count = len(text.strip().split())

            # Send response (also length-prefixed)
            response_bytes = str(word_count).encode("utf-8")
            response_len = struct.pack('!I', len(response_bytes))

            conn.sendall(response_len + response_bytes)