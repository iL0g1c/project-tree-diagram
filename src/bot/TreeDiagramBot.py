import discord
from discord import app_commands
from discord.ext import commands
from dotenv import load_dotenv
import os
import logging
import sys
import grpc
from proto import database_service_pb2_grpc
from proto import database_service_pb2
from utils.configManager import ConfigManager

load_dotenv()
BOT_TOKEN = os.getenv('BOT_BETA_TOKEN')

class TreeDiagram(commands.Bot):
    def __init__(self, botToken):
        # setting up logger
        self.logger = logging.getLogger("TreeDiagram")
        self.logger.setLevel(logging.DEBUG)
        console_handler = logging.StreamHandler(sys.stdout)
        self.logger.addHandler(console_handler)

        intents = discord.Intents.all()
        super().__init__(command_prefix="=", intents=intents)

    async def on_ready(self):
        self.logger.log(20, f'{self.user} has connected to Discord!')

    async def setup_hook(self) -> None:
        self.logger.log(20, "Starting up...")

        self.logger.log(20, "Loading cogs...")
        await self._load_cogs()

        self.logger.log(20, "Syncing commands...")
        try:
            synced = await self.tree.sync()
            self.logger.log(20, f"Synced {len(synced)} command(s)")
        except Exception as e:
            self.logger.log(40,     f"Exception while syncing commands. Error: {e}")

        self.logger.log(20, "Connecting to discord...")


    async def _load_cogs(self) -> None:
        for extension in ("configuration", "force", "intelligence", "patrolling"):
            await self.load_extension(f"commands.{extension}")

    async def on_guild_join(self, guild):
        with grpc.insecure_channel("localhost:50051") as channel:
            stub = database_service_pb2_grpc.DatabaseServiceStub(channel)
            request = database_service_pb2.InsertNewGuildRequest(guild_id=int(guild.id))
            response = stub.InsertNewGuild(request)
            if response.success:
                self.logger.log(20, f"Joined guild: {guild.name}")
            else:
                self.logger.log(40, f"Failed guild setup: {guild.name}")

def main():
    bot = TreeDiagram(BOT_TOKEN)
    bot.configManager = ConfigManager()
    @bot.tree.command(name="ping", description="Get the bot's latency.")
    async def ping(interaction: discord.Interaction):
        await interaction.response.send_message("# Pong! :ping_pong:\nLatency: " + str(bot.latency*1000) + "ms")
    bot.run(BOT_TOKEN)

if __name__ == "__main__":
    main()