import asyncio

class ChatServer:
    def __init__(self):
        self.clients = {}  # {username: writer}

    async def prompt_username(self, reader, writer):
        """Yêu cầu client nhập nickname, đảm bảo không trùng"""
        while True:
            writer.write("Enter username: ".encode())
            await writer.drain()
            data = await reader.readline()
            if not data:
                return None
            username = data.decode().strip()
            if username and username not in self.clients:
                self.clients[username] = writer
                return username
            writer.write("Sorry, that username is taken.\n".encode())
            await writer.drain()

    async def broadcast(self, message, exclude=None):
        """Gửi message tới tất cả client, có thể loại trừ một client"""
        for user, writer in list(self.clients.items()):
            if user == exclude:
                continue
            try:
                writer.write((message + "\n").encode())
                await writer.drain()
            except ConnectionError:
                continue

    async def handle_client(self, reader, writer):
        """Coroutine xử lý từng client"""
        writer.write("Welcome to ChatServer!\n".encode())
        await writer.drain()

        username = await self.prompt_username(reader, writer)
        if not username:
            writer.close()
            await writer.wait_closed()
            return

        await self.broadcast(f"{username} has joined the chat", exclude=username)

        try:
            while True:
                data = await reader.readline()
                if not data:
                    break
                message = data.decode().strip()
                await self.broadcast(f"{username}: {message}", exclude=username)
        except ConnectionError:
            pass
        finally:
            if username in self.clients:
                del self.clients[username]
            await self.broadcast(f"{username} has left the chat")
            writer.close()
            await writer.wait_closed()

async def main():
    server = ChatServer()
    server_coro = await asyncio.start_server(
        server.handle_client, "0.0.0.0", 9000
    )
    addr = server_coro.sockets[0].getsockname()
    print(f"Server listening on {addr}")
    async with server_coro:
        await server_coro.serve_forever()

if __name__ == "__main__":
    asyncio.run(main())
