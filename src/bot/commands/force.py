import discord
from discord import app_commands
from discord.ext import commands
import random
import string
from proto import database_service_pb2
import utils.validateUser as validateUser
import utils.configManager as configManager
import utils.paginationEmbed as paginationEmbed
import utils.GrpcClient as GrpcClient

class Force(commands.Cog):
    def __init__(self, bot):
        self.bot = bot
        self.configManager = configManager.ConfigManager()
        self.grpc_client = GrpcClient.GrpcClient()

    force_group = app_commands.Group(name="force", description="Force management commands")

    @force_group.command(name="register-pilot", description="Register a pilot to your force.")
    async def add_pilot(self, interaction: discord.Interaction, geofs_account_id: int, pilot: discord.Member):
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            request = database_service_pb2.UpdateUserForceCodeRequest(
                geofs_account_id=int(geofs_account_id),
                discord_id=int(pilot.id),
                guild_id=int(interaction.guild.id)
            )
            response = self.grpc_client.call_method("DatabaseService", "UpdateUserForceCode", request)
            if response.success:
                await interaction.followup.send(content="User added to your force successfully.")
            else:
                await interaction.followup.send(content="Failed to add user to your force.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @force_group.command(name="get-all-pilots", description="Get all pilots in your force.")
    async def get_all_pilots(self, interaction: discord.Interaction):
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            request = database_service_pb2.GetForceUsersRequest(guild_id=int(interaction.guild.id))
            response = self.grpc_client.call_method("DatabaseService", "GetForceUsers", request)
            if response.users:
                lines = []
                allowed_mentions = discord.AllowedMentions.none()
                for user in response.users:
                    user_obj = self.bot.get_user(int(user.discord_id))
                    if user:
                        lines.append(f"{user_obj.mention} - {user.geofs_account_id}")
                embed = paginationEmbed.PaginatedEmbed(
                    items=lines,
                    title="All Pilots registered in your force",
                )
                await interaction.followup.send(embed=embed.embed, view=embed, allowed_mentions=allowed_mentions)

            else:
                await interaction.followup.send(
                    content="No pilots found for your force.", allowed_mentions=discord.AllowedMentions.none()
                )
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @force_group.command(name="update-code", description="Update the force code.")
    async def update_code(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            first_character = random.choice(string.ascii_uppercase)
            second_character = random.randint(0, 9)
            third_character = random.choice(string.ascii_uppercase)
            callsign_code = first_character + str(second_character) + third_character

            config = self.configManager.get_config(int(interaction.guild.id))
            channel = self.bot.get_channel(int(config["callsign_code_channel_id"]))

            if channel:
                member_role = discord.utils.get(interaction.guild.roles, id=int(config["member_role_id"]))
                await channel.send(f"# **__Daily code__**\n**Code: {callsign_code}**\n**Example:** `Tempest-#[140][{callsign_code}][IDF]`\n{member_role.mention}")
                await interaction.response.send_message("Force code updated successfully.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

async def setup(bot):
    await bot.add_cog(Force(bot))