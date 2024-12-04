import discord
from discord import app_commands
from discord.ext import commands
import grpc
from proto import database_service_pb2_grpc
from proto import database_service_pb2
from google.protobuf.timestamp_pb2 import Timestamp
from datetime import datetime
import utils.validateUser as validateUser
import utils.paginationEmbed as paginationEmbed

class Intelligence(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    intelligence_group = app_commands.Group(name="intelligence", description="GeoFS Intelligence Commands")

    @intelligence_group.command(name="add-id", description="Add a pilot to the ID storage database.")
    @app_commands.describe(discord_id="Discord ID of the pilot.", geofs_id="GeoFS ID of the pilot.")
    # create a discord slash command that adds a pilot to the id storage database with their discord id and geofs id
    async def add_id_storage(self, interaction: discord.Interaction, discord_id: str, geofs_id: str):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.defer()
            with grpc.insecure_channel("localhost:50051") as channel:
                stub = database_service_pb2_grpc.DatabaseServiceStub(channel)
                request = database_service_pb2.InsertUserDiscordIdRequest(discord_id=int(discord_id), geofs_account_id=int(geofs_id))
                response = stub.InsertUserDiscordId(request)
                if response.success:
                    await interaction.followup.send(content="User added successfully.")
                else:
                    await interaction.followup.send(content="Failed to add user.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @intelligence_group.command(name="callsign-changes", description="Get callsign changes of a GeoFS pilot.")
    @app_commands.describe(geofs_id="GeoFS ID of the pilot.")
    async def get_ccls(self, interaction: discord.Interaction, geofs_id: str):
        # PARITY ISSUE
        # timestamp | Old Callsign | New Callsign
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.defer()
            with grpc.insecure_channel("localhost:50051") as channel:
                stub = database_service_pb2_grpc.DatabaseServiceStub(channel)
                request = database_service_pb2.UserCallsignChangesRequest(geofs_account_id=int(geofs_id))
                response = stub.GetUserCallsignChanges(request)
                if not response.events:
                    await interaction.followup.send(content="No callsign changes found.")
                else:
                    lines = []
                    for event in response.events:
                        timestamp = datetime.fromtimestamp(event.timestamp.seconds)
                        lines.append(f"**Detected At:** {timestamp} | **Old Callsign:** {discord.utils.escape_markdown(event.old_callsign)} | **New Callsign:** {discord.utils.escape_markdown(event.new_callsign)}")
                    embed = paginationEmbed.PaginatedEmbed(
                        items=lines,
                        title="Callsign Changes",
                    )
                    await interaction.followup.send(embed=embed.embed, view=embed)
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

async def setup(bot):
    await bot.add_cog(Intelligence(bot))