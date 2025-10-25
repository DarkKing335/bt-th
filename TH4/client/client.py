import socket
import threading

UDP_IP = input("Enter server IP: ").strip()
UDP_PORT = 3072

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.settimeout(0.5)

last_seq_num = -1
receiving = True

def receive_thread():
    global last_seq_num, receiving
    while receiving:
        try:
            data, addr = sock.recvfrom(1024)
            msg = data.decode()
            if '|' in msg:
                seq_str, content = msg.split('|', 1)
                seq = int(seq_str)
                if seq > last_seq_num + 1:
                    print(f"--- PACKET LOSS DETECTED! EXPECTED SEQ #{last_seq_num+1} ---")
                last_seq_num = seq
                print(f"{seq}|{content}")
        except:
            continue

# Bắt đầu thread nhận dữ liệu
t = threading.Thread(target=receive_thread, daemon=True)
t.start()

print("[CLIENT] Commands: START, PAUSE, RESUME, EXIT")
while True:
    cmd = input("Enter command: ").strip().upper()
    if cmd in ["START", "PAUSE", "RESUME", "EXIT"]:
        sock.sendto(cmd.encode(), (UDP_IP, UDP_PORT))
    if cmd == "EXIT":
        receiving = False
        break

t.join()
sock.close()
print("[CLIENT] Client stopped")
