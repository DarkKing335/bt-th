import asyncio

async def listen_server(reader):
    """Nhận tin nhắn từ server và in ra màn hình"""
    while True:
        data = await reader.readline()
        if not data:
            print("Server closed the connection.")
            break
        print(data.decode().strip())

async def send_server(writer):
    """Gửi tin nhắn từ người dùng lên server"""
    loop = asyncio.get_event_loop()
    while True:
        message = await loop.run_in_executor(None, input)
        if message.lower() == "/quit":
            writer.close()
            await writer.wait_closed()
            print("Disconnected from server.")
            break
        writer.write((message + "\n").encode())
        await writer.drain()

async def main():
    server_ip = input("Enter server IP: ").strip()
    port = 9000  # port phải giống server

    reader, writer = await asyncio.open_connection(server_ip, port)

    greeting = await reader.readline()
    print(greeting.decode().strip())

    # Nhập nickname, xử lý nếu bị trùng
    while True:
        prompt = await reader.readline()
        if not prompt:
            print("Server closed connection")
            return
        print(prompt.decode().strip(), end=' ')
        nickname = input()
        writer.write((nickname + "\n").encode())
        await writer.drain()

        response = await reader.readline()
        if not response:
            print("Server closed connection")
            return
        msg = response.decode().strip()
        if "Sorry" in msg:
            print(msg)
            continue
        else:
            break

    await asyncio.gather(
        listen_server(reader),
        send_server(writer)
    )

if __name__ == "__main__":
    asyncio.run(main())
