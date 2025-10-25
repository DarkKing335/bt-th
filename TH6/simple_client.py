import socket
import os

WWW_DIR = "www"

def list_files(base_dir):
    print("\n===== Danh sach tep tin co san =====")
    file_paths = []
    index = 1
    for root, dirs, files in os.walk(base_dir):
        for file in files:
            rel_path = os.path.relpath(os.path.join(root, file), base_dir)
            print(f"{index}. /{rel_path.replace(os.sep, '/')}")
            file_paths.append("/" + rel_path.replace(os.sep, "/"))
            index += 1
    if not file_paths:
        print("Khong co tep tin nao trong thu muc www/")
    print("====================================\n")
    return file_paths

def main():
    host = input("Nhap IP cua server: ").strip()
    port = 8888  # port mặc định

    if not os.path.isdir(WWW_DIR):
        print(f"Thu muc '{WWW_DIR}' khong ton tai!")
        return

    files = list_files(WWW_DIR)
    if not files:
        return

    choice = int(input("Chon so tep muon gui yeu cau: "))
    if choice < 1 or choice > len(files):
        print("Lua chon khong hop le!")
        return
    path = files[choice - 1]

    # Gửi request
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.connect((host, port))
        request = f"GET {path} HTTP/1.0\r\nHost: {host}\r\n\r\n"
        s.sendall(request.encode())

        response = b""
        while True:
            data = s.recv(4096)
            if not data:
                break
            response += data

    # Tách header và body
    response_text = response.decode(errors='replace')
    header_end = response_text.find("\r\n\r\n")
    headers = response_text[:header_end]
    body = response[header_end+4:]

    print("\n===== Response Headers =====")
    print(headers)
    print("============================\n")

    # Lưu body ra file
    ext = os.path.splitext(path)[1]
    filename = "output" + (ext if ext else ".html")
    with open(filename, "wb") as f:
        f.write(body)

    print(f"Body saved to {filename}")

if __name__ == "__main__":
    main()
