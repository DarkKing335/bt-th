import socket
import os
import mimetypes

WWW_DIR = "www"
PORT = 8888

def get_content_type(filename):
    type, _ = mimetypes.guess_type(filename)
    return type or "application/octet-stream"

def handle_client(conn):
    try:
        request = conn.recv(4096).decode()
        if not request:
            conn.close()
            return

        # Lấy request line
        request_line = request.splitlines()[0]
        method, path, _ = request_line.split()

        if path == "/":
            path = "/index.html"

        file_path = os.path.join(WWW_DIR, path.lstrip("/"))

        if os.path.isfile(file_path):
            with open(file_path, "rb") as f:
                body = f.read()
            content_type = get_content_type(file_path)
            response = f"HTTP/1.0 200 OK\r\nContent-Type: {content_type}\r\nContent-Length: {len(body)}\r\n\r\n".encode() + body
        else:
            not_found_path = os.path.join(WWW_DIR, "404.html")
            if os.path.isfile(not_found_path):
                with open(not_found_path, "rb") as f:
                    body = f.read()
            else:
                body = b"<h1>404 Not Found</h1>"
            response = f"HTTP/1.0 404 Not Found\r\nContent-Type: text/html\r\nContent-Length: {len(body)}\r\n\r\n".encode() + body

        conn.sendall(response)
    finally:
        conn.close()

def main():
    if not os.path.isdir(WWW_DIR):
        os.makedirs(WWW_DIR)

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        # Bind tất cả địa chỉ để client trên máy khác kết nối
        s.bind(("0.0.0.0", PORT))
        s.listen(5)
        print(f"Server listening on port {PORT}...")

        while True:
            conn, addr = s.accept()
            print(f"Connection from {addr}")
            handle_client(conn)

if __name__ == "__main__":
    main()
