import discord
from discord import app_commands
from discord.ext import commands
from discord.utils import escape_markdown
import asyncio
import datetime
import pytz
from proto import database_service_pb2
import utils.validateUser as validateUser
import utils.GrpcClient as GrpcClient
import utils.handleProtobufUnpacking as handleProtobufUnpacking
import utils.paginationEmbed as paginationEmbed


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
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            request = database_service_pb2.InsertKillLogRequest(
                discord_id=interaction.user.id,
                guild_id=interaction.guild.id
            )
            response = self.grpc_client.call_method("DatabaseService", "InsertKillLog", request)
            config = self.bot.configManager.get_config(int(interaction.guild.id))
            if response.event_id != -1:
                kill_log_channel = self.bot.get_channel(config["kill_log_channel_id"])
                if kill_log_channel is None:
                    await interaction.followup.send("An error occurred. Contact High Command.")
                    return
                event_id = response.event_id
                kill_count = response.kill_count
                event_time = response.timestamp.ToDatetime()
                event_time = event_time.strftime("%Y-%m-%d %H:%M:%S")
                embed = discord.Embed(
                    title="Kill Event",
                    description=f"{interaction.user.mention} has earned a kill!",
                    color=discord.Color.blurple()
                )
                embed.add_field(name="Event ID", value=event_id)
                embed.add_field(name="Kill Count", value=kill_count)
                embed.add_field(name="Time", value=f"{event_time} UTC")

                asyncio.run_coroutine_threadsafe(kill_log_channel.send(embed=embed), self.bot.loop)
                await interaction.followup.send("Kill logged successfully.")
            else:
                await interaction.followup.send("An error occurred. Contact High Command.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

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
        await interaction.response.defer()
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
                await interaction.followup.send("Patrol logged successfully.")
            elif (patrol_report["response_code"] == 1):
                await interaction.followup.send("This patrol overlaps with another patrol.")
        else:
            if user_role_check[1] is None:
                await interaction.followup.send("You do not have permission to use this command.")
            else:
                await interaction.followup.send(user_role_check[1])

    @patrolling_group.command(name="online", description="Get online pilots for your force.")
    async def online_pilots(self, interaction: discord.Interaction):
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            request = database_service_pb2.GetAllOnlinePilotsRequest(
                guild_id=interaction.guild.id
            )
            response = self.grpc_client.call_method("DatabaseService", "GetAllOnlinePilots", request)
            if len(response.discord_ids) > 0:
                lines = []
                for discord_id in response.discord_ids:
                    discord_user = self.bot.get_user(discord_id)
                    lines.append(f"{response.discord_ids.index(discord_id) + 1}. {discord_user.mention}")
                embed = paginationEmbed.PaginatedEmbed(
                    items=lines,
                    title="Online Pilots"
                )
                await interaction.followup.send(embed=embed.embed, view=embed)
            else:
                await interaction.followup.send("No online pilots for your force were found.")

        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="patrol-delete", description="Delete a patrol.")
    @app_commands.describe(event_id="ID of the patrol to delete.")
    async def delete_patrol(self, interaction: discord.Interaction, event_id: int):
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            request = database_service_pb2.DeletePatrolLogRequest(
                event_id=event_id,
                guild_id=interaction.guild.id
            )
            response = self.grpc_client.call_method("DatabaseService", "DeletePatrolLog", request)
            if response.response_code == 0:
                await interaction.followup.send("Patrol deleted successfully.")
            elif response.response_code == 1:
                await interaction.followup.send("This patrol does not exist in your forces database.")
        else:
            if user_role_check[1] is None:
                await interaction.followup.send("You do not have permission to use this command.")
            else:
                await interaction.followup.send(user_role_check[1])

    @patrolling_group.command(
        name="patrol-report",
        description="Get a report on patrols done since a specified date.")
    @app_commands.describe(
        year="Year of start date for patrol acceptance.",
        month="Month of start date for patrol acceptance.",
        day="Day of start date for patrol acceptance."
    )
    async def patrol_report(self, interaction: discord.Interaction, year: int, month: int, day: int):
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            time_frame_start = datetime.datetime(year, month, day, 0, 0, 0)
            request = database_service_pb2.GetPatrolLogsByDateRequest(
                guild_id=interaction.guild.id,
                time_frame_start=time_frame_start
            )
            response = self.grpc_client.call_method("DatabaseService", "GetPatrolLogsByDate", request)
            if len(response.patrol_reports) > 0:
                lines = []
                for patrol_report in response.patrol_reports:
                    event_id = patrol_report.event_id
                    discord_user = self.bot.get_user(patrol_report.discord_id)

                    start_datetime = patrol_report.start_datetime.ToDatetime()
                    end_datetime = patrol_report.end_datetime.ToDatetime()

                    duration = round((end_datetime - start_datetime).total_seconds() / 60)
                    
                    start_datetime = start_datetime.astimezone(pytz.timezone("UTC"))
                    start_datetime = start_datetime.strftime("%Y-%m-%d %H:%M:%S")
                    end_datetime = end_datetime.astimezone(pytz.timezone("UTC"))
                    end_datetime = end_datetime.strftime("%Y-%m-%d %H:%M:%S")

                    lines.append(f"**{response.patrol_reports.index(patrol_report) + 1}.** **Event ID:** {event_id} \| **User:** {discord_user} \| **Start:** {start_datetime} UTC \| **End:** {end_datetime} UTC \| **Duration:** {duration} minutes")

                embed = paginationEmbed.PaginatedEmbed(
                    items=lines,
                    title="Configuration Keys"
                )
                await interaction.followup.send(embed=embed.embed, view=embed)
            else:
                await interaction.followup.send("No patrol logs for your force in the selected time frame were in the database.")
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