import discord
from discord import app_commands
from discord.ext import commands
import random
import string
import datetime
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

    @force_group.command(name="info", description="Get information about your force.")
    async def force_info(self, interaction: discord.Interaction):
        await interaction.response.defer()
        # Force server image.
        # Number of pilots in the force.
        # Number of online pilots in the force.
        # Number of online discord users in the force.
        # Total hours in the last 30 days.
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            guild_icon_url = interaction.guild.icon.url if interaction.guild.icon else None

            request = database_service_pb2.GetForceUsersRequest(guild_id=int(interaction.guild.id))
            response = self.grpc_client.call_method("DatabaseService", "GetForceUsers", request)
            if response.users:
                total_pilots = len(response.users)
            else:
                await interaction.followup.send(content="Failed to get all pilots in your force.")
                return

            request = database_service_pb2.GetAllOnlinePilotsRequest(
                guild_id=interaction.guild.id
            )
            response = self.grpc_client.call_method("DatabaseService", "GetAllOnlinePilots", request)
            print(len(response.discord_ids))
            if len(response.discord_ids) > 0:
                online_users_count = len(response.discord_ids)
            else:
                online_users_count = 0

            time_frame_start = datetime.datetime.now() - datetime.timedelta(days=30)
            request = database_service_pb2.GetPatrolLogsByDateRequest(
                guild_id=interaction.guild.id,
                time_frame_start=time_frame_start
            )
            response = self.grpc_client.call_method("DatabaseService", "GetPatrolLogsByDate", request)
            patrol_hours = 0
            if len(response.patrol_reports):
                for patrol_report in response.patrol_reports:
                    start_datetime = patrol_report.start_datetime.ToDatetime()
                    end_datetime = patrol_report.end_datetime.ToDatetime()
                    duration = ((end_datetime - start_datetime).total_seconds() / 60) / 60
                    patrol_hours += duration
                patrol_hours = round(patrol_hours, 2)

            config = self.configManager.get_config(int(interaction.guild.id))
            member_role = discord.utils.get(interaction.guild.roles, id=int(config["member_role_id"]))

            embed = discord.Embed(
                title="Force Information",
                description=f"Force: {interaction.guild.name}",
                color=discord.Color.blurple()
            )
            embed.set_image(url=guild_icon_url)
            embed.add_field(name="Guild URL", value=interaction.guild.icon.url)
            embed.add_field(name="Total Pilots Registerd", value=total_pilots)
            embed.add_field(name="Online Pilots", value=online_users_count)
            embed.add_field(name="Total Discord Users", value=len(member_role.members))
            embed.add_field(name="Total Hours in the last 30 days", value=f"{patrol_hours} hours")
            await interaction.followup.send(embed=embed)
        else:
            if user_role_check[1] is None:
                await interaction.followup.send("You do not have permission to use this command.")
                return
            else:
                await interaction.followup.send(user_role_check[1])
                return

async def setup(bot):
    await bot.add_cog(Force(bot))