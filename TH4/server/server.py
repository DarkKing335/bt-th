import socket
import time
import random

# Cấu hình server
UDP_IP = "0.0.0.0"  # lắng nghe tất cả IP trên máy
UDP_PORT = 3072
SLEEP_TIME = 0.1
PACKET_LOSS_RATE = 0.2  # 20% giả lập mất gói

# Tạo socket UDP
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))
print("[SERVER] Waiting for client commands...")

# Load file dữ liệu
file_path = "data.txt"
with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

client_addr = None
streaming = False
seq = 0

while True:
    sock.settimeout(0.1)
    try:
        data, addr = sock.recvfrom(1024)
        msg = data.decode().strip().upper()
        if client_addr is None:
            client_addr = addr  # lưu địa chỉ client
        if msg == "START":
            streaming = True
            print("[SERVER] START streaming")
        elif msg == "PAUSE":
            streaming = False
            print("[SERVER] PAUSE streaming")
        elif msg == "RESUME":
            streaming = True
            print("[SERVER] RESUME streaming")
        elif msg == "EXIT":
            print("[SERVER] Client requested exit")
            break
    except socket.timeout:
        pass

    # Gửi dữ liệu nếu streaming đang bật
    if streaming and client_addr is not None:
        if seq < len(lines):
            if random.random() >= PACKET_LOSS_RATE:
                packet = f"{seq}|{lines[seq].strip()}"
                sock.sendto(packet.encode(), client_addr)
            seq += 1
        else:
            # Gửi xong file, dừng streaming
            streaming = False
            seq = 0
    time.sleep(SLEEP_TIME)

sock.close()
print("[SERVER] Server stopped")
