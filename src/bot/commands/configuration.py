import discord
from discord import app_commands
from discord.ext import commands
import grpc
from proto import database_service_pb2_grpc
from proto import database_service_pb2
import utils.validateUser as validateUser
import utils.paginationEmbed as paginationEmbed
import utils.handleProtobufUnpacking as handleProtobufUnpacking

class Configuration(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    config_group = app_commands.Group(name="config", description="Bot Configuration Commands")

    @config_group.command(name="set", description="Change the bot's configuration.")
    async def config_change(self, interaction: discord.Interaction, key: str, value: str):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.defer()
            success, error = self.bot.configManager.update_key(key, value)
            if not success:
                await interaction.followup.send(error)
            else:
                await interaction.followup.send(f"Configuration key `{key}` has been updated to `{value}`.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @config_group.command(name="keys", description="Get the list of the bot's configuration keys.")
    async def get_keys(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.defer()

            with grpc.insecure_channel("localhost:50051") as channel:
                stub = database_service_pb2_grpc.DatabaseServiceStub(channel)
                request = database_service_pb2.GetConfigurationKeysRequest(guild_id=int(interaction.guild.id))
                keys = stub.GetConfigurationKeys(request)

            keys = handleProtobufUnpacking.unpack(keys)            
            lines = [f"* {key}: `{value}`" for key, value in keys.items()]

            embed = paginationEmbed.PaginatedEmbed(
                items=lines,
                title="Configuration Keys"
            )

            await interaction.followup.send(embed=embed.embed, view=embed)
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

async def setup(bot):
    await bot.add_cog(Configuration(bot))