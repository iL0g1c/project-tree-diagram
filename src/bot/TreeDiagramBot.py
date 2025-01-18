import discord
from discord import app_commands
from discord.ext import commands
from discord.ext import tasks
from discord.utils import escape_markdown
import datetime
from dotenv import load_dotenv
import asyncio
import os
import logging
import sys
import grpc
import threading
from flask import Flask, request, jsonify
import random
import string

from proto import database_service_pb2_grpc
from proto import database_service_pb2
from utils.configManager import ConfigManager

load_dotenv()
BOT_TOKEN = os.getenv('BOT_LIVE_TOKEN')

class TreeDiagram(commands.Bot):
    def __init__(self, botToken):
        self.configManager = ConfigManager()
        # setting up logger
        self.logger = logging.getLogger("TreeDiagram")
        self.logger.setLevel(logging.DEBUG)
        console_handler = logging.StreamHandler(sys.stdout)
        self.logger.addHandler(console_handler)

        intents = discord.Intents.all()
        super().__init__(command_prefix="=", intents=intents)

    async def on_ready(self):
        self.logger.log(20, f'{self.user} has connected to Discord!')
        await self.update_callsign_code.start()

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
        with grpc.insecure_channel(self.configManager.host) as channel:
            stub = database_service_pb2_grpc.DatabaseServiceStub(channel)
            request = database_service_pb2.InsertNewGuildRequest(guild_id=int(guild.id))
            response = stub.InsertNewGuild(request)
            if response.success:
                self.logger.log(20, f"Joined guild: {guild.name}")
            else:
                self.logger.log(40, f"Failed guild setup: {guild.name}")
    @tasks.loop(time=datetime.time(hour=0, minute=0, second=0))
    async def update_callsign_code(self):
        first_character = random.choice(string.ascii_uppercase)
        second_character = random.randint(0, 9)
        third_character = random.choice(string.ascii_uppercase)
        callsign_code = first_character + str(second_character) + third_character
        callsign_code_channels = self.configManager.get_all_of_key("callsign_code_channel_id")
        callsign_code_loop_enabled = self.configManager.get_all_of_key("callsign_code_loop_enabled")
        member_roles = self.configManager.get_all_of_key("member_role_id")
        for key in callsign_code_channels:
            if callsign_code_channels[key] and callsign_code_loop_enabled[key]:
                channel = self.get_channel(int(callsign_code_channels[key]))
                if channel:
                    member_role = discord.utils.get(self.get_guild(int(key)).roles, id=int(member_roles[key]))
                    await channel.send(f"# **__Daily code__**\n**Code: {callsign_code}**\n**Example:** `Tempest-#[140][{callsign_code}][IDF]`\n{member_role.mention}")

        


app = Flask(__name__)
@app.route("/callsign-changes", methods=["POST"])
def callsign_changes():
    data = request.json
    description = ""
    for user in data:
        account_id = user["acid"]
        old_callsign = user["old_callsign"]
        new_callsign = user["new_callsign"]
        description += escape_markdown(f"Account ID: {account_id} | Old Callsign: {old_callsign} | New Callsign: {new_callsign}\n")
        
    embed = discord.Embed(
        title="Callsign Changes",
        description=description,
        color=discord.Color.blurple()
    )
    keys = bot.configManager.get_all_of_key("callsign_change_channel_id")
    for key in keys:
        if keys[key]:
            channel = bot.get_channel(int(keys[key]))
            if channel:
                asyncio.run_coroutine_threadsafe(channel.send(embed=embed), bot.loop)
    return jsonify({"success": "ok"}), 200

@app.route("/player-activity-change", methods=["POST"])
def player_activity_change():
    data = request.json
    description = ""
    activity_channels = bot.configManager.get_all_of_key("player_activity_channel_id")
    guild_force_codes = bot.configManager.get_all_of_key("force_code")
    for force in list(guild_force_codes.keys()):
        description = ""
        for user in data:
            if user["force_code"] == guild_force_codes[force]:
                if user["is_online"]:
                    description += escape_markdown(f"{bot.get_user(user['discord_id']).mention} just came online to start their patrol!\n")
                else:
                    description += escape_markdown(f"{bot.get_user(user['discord_id']).mention} just went offline to end their patrol!\n")
        embed = discord.Embed(
            title="Pilot Activity Updates",
            description=description,
            color=discord.Color.blurple()
        )
        if (description != ""):
            channel = bot.get_channel(int(activity_channels[force]))
            if channel:
                asyncio.run_coroutine_threadsafe(channel.send(embed=embed), bot.loop)
    return jsonify({"success": "ok"}), 200

def run_flask():
    app.run(host='0.0.0.0', port=5001, debug=False)


def main():
    global bot
    bot = TreeDiagram(BOT_TOKEN)
    @bot.tree.command(name="ping", description="Get the bot's latency.")
    async def ping(interaction: discord.Interaction):
        await interaction.response.send_message("# Pong! :ping_pong:\nLatency: " + str(bot.latency*1000) + "ms")

    flask_thread = threading.Thread(target=run_flask, daemon=True)
    flask_thread.start()

    bot.run(BOT_TOKEN)

if __name__ == "__main__":
    main()