import discord
from discord import app_commands
from discord.ext import commands
import datetime
import pytz
from proto import database_service_pb2
import utils.validateUser as validateUser
import utils.GrpcClient as GrpcClient
import utils.handleProtobufUnpacking as handleProtobufUnpacking


class Patrolling(commands.Cog):
    def __init__(self, bot):
        self.bot = bot
        self.grpc_client = GrpcClient.GrpcClient()
    
    patrolling_group = app_commands.Group(name="patrolling", description="GeoFS Patrolling Commands")

    @patrolling_group.command(name="inactive", description="Get inactive pilots since a specified date.")
    @app_commands.describe(
        year="Year of start date for patrol acceptance.",
        month="Month of start date for patrol acceptance.",
        day="Day of start date for patrol acceptance."
    )
    async def inactive_pilots(self, interaction: discord.Interaction, year: str, month: str, day: str):
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="kill", description="Log a kill.")
    async def kill(self, interaction: discord.Interaction):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="manual_log", description="Manually log a patrol.")
    @app_commands.describe(
        start_year="Year of the start of the patrol.",
        start_month="Month of the start of the patrol.",
        start_day="Day of the start of the patrol.",
        start_hour="Hour of the start of the patrol.",
        start_minute="Minute of the start of the patrol.",
        patrol_duration="Duration of the patrol in MINUTES."
    )
    async def log_patrol(self, interaction: discord.Interaction, start_year: int, start_month: int, start_day: int, start_hour: int, start_minute: int, patrol_duration: int):
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            start_time = datetime.datetime(start_year, start_month, start_day, start_hour, start_minute)
            end_time = start_time + datetime.timedelta(minutes=patrol_duration)
            request = database_service_pb2.InsertPatrolLogRequest(
                discord_id=interaction.user.id,
                guild_id=interaction.guild.id,
                start_datetime=start_time,
                end_datetime=end_time
            )
            response = self.grpc_client.call_method("DatabaseService", "InsertPatrolLog", request)
            patrol_report = handleProtobufUnpacking.unpack(response.patrol_report)
            if patrol_report["response_code"] == 0:
                embed = discord.Embed(
                    title="Patrol Event",
                    description=f"{interaction.user.mention} has completed a patrol!",
                    color=discord.Color.blurple()
                )
                start_time = start_time.astimezone(pytz.timezone("UTC"))
                start_time = start_time.strftime("%Y-%m-%d %H:%M:%S")
                end_time = end_time.astimezone(pytz.timezone("UTC"))
                end_time = end_time.strftime("%Y-%m-%d %H:%M:%S")
                embed.add_field(name="Event ID", value=patrol_report["event_id"])
                embed.add_field(name="Patrol Count", value=patrol_report["patrol_count"])
                embed.add_field(name="Start Time", value=f"{start_time} UTC")
                embed.add_field(name="End Time", value=f"{end_time} UTC")
                embed.add_field(name="Duration", value=f"{patrol_duration} minutes")
                channel = self.bot.get_channel(patrol_report["patrol_log_channel_id"])
                if channel:
                    await channel.send(embed=embed)
                await interaction.response.send_message("Patrol logged successfully.")
            elif (patrol_report["response_code"] == 1):
                await interaction.response.send_message("This user is not part of your force.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="online", description="Get online pilots from discord.")
    async def online_pilots(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="patrol-delete", description="Delete a patrol.")
    @app_commands.describe(user="Pilot user.", number="Number of the Patrol to delete.")
    async def delete_patrol(self, interaction: discord.Interaction, user: discord.Member, number: int):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="patrol-report-role",
                        description="Get a report on patrols done by a specific role since a specified date.")
    @app_commands.describe(
        year="Year of start date for patrol acceptance.",
        month="Month of start date for patrol acceptance.",
        day="Day of start date for patrol acceptance."
    )
    @app_commands.describe(role="Discord role to get patrol report for.")
    async def patrol_report_role(self, interaction: discord.Interaction, year: str, month: str, day: str, role: str):
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(
        name="patrol-report",
        description="Get a report on patrols done since a specified date.")
    @app_commands.describe(
        year="Year of start date for patrol acceptance.",
        month="Month of start date for patrol acceptance.",
        day="Day of start date for patrol acceptance."
    )
    async def patrol_report(self, interaction: discord.Interaction, year: str, month: str, day: str):
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="sar", description="Log a SAR.")
    async def sar(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])
    
    @patrolling_group.command(name="top-disables", description="Get leaderboard of disables.")
    async def top_disables(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="top-kills", description="Get leaderboard of kills.")
    async def top_kills(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])
    
    @patrolling_group.command(name="top-sars", description="Get leaderboard of sars.")
    async def top_sars(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

async def setup(bot):
    await bot.add_cog(Patrolling(bot))