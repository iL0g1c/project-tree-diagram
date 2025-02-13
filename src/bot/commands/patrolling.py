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

    @patrolling_group.command(name="kill-delete", description="Delete a kill.")
    @app_commands.describe(event_id="The event_id listed on the kill log.")
    async def delete_kill(self, interaction: discord.Interaction, event_id: int):
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            request = database_service_pb2.DeleteKillLogRequest(
                event_id=event_id,
                guild_id=interaction.guild.id
            )
            response = self.grpc_client.call_method("DatabaseService", "DeleteKillLog", request)
            if response.response_code == 0:
                await interaction.followup.send("Kill deleted successfully.")
            elif response.response_code == 1:
                await interaction.followup.send("This kill does not exist in your forces database.")
        else:
            if user_role_check[1] is None:
                await interaction.followup.send("You do not have permission to use this command.")
            else:
                await interaction.followup.send(user_role_check[1])

    @patrolling_group.command(name="manual_log", description="Manually log a patrol.")
    @app_commands.describe(
        start_date="Date to get all following patrols since then. (Format: YYYY-MM-DD HH:MM:SS)",
        patrol_duration="Duration of the patrol in MINUTES."
    )
    async def log_patrol(self, interaction: discord.Interaction, start_date: str, patrol_duration: int):
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            start_time = datetime.datetime.strptime(start_date, "%Y-%m-%d %H:%M:%S")
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

    @patrolling_group.command(name="get-patrols", description="Get patrols by date and/or user.")
    @app_commands.describe(
        date="Date to get all following patrols since then. (Format: YYYY-MM-DD HH:MM:SS)",
        discord_user="Discord user to get patrols for."
    )
    async def get_patrols(self, interaction: discord.Interaction, date: str=None, discord_user: discord.User=None):
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            discord_name_placeholder = "all users"
            if date is not None:
                minimum_date = datetime.datetime.strptime(date, "%Y-%m-%d %H:%M:%S")
            else:
                minimum_date = datetime.datetime.min
            if discord_user is not None:
                discord_id = discord_user.id
                discord_name_placeholder = discord_user.name
            else:
                discord_id = 0

            request = database_service_pb2.GetPatrolLogsRequest(
                guild_id=interaction.guild.id,
                minimum_date=minimum_date,
                discord_id=discord_id
            )
            response = self.grpc_client.call_method("DatabaseService", "GetPatrolLogs", request)

            if len(response.patrol_reports) > 0:
                lines = []
                for patrol in response.patrol_reports:
                    patrol_start_time = patrol.start_datetime.ToDatetime()
                    patrol_start_time_str = patrol_start_time.strftime("%Y-%m-%d %H:%M:%S")
                    patrol_end_time = patrol.end_datetime.ToDatetime()
                    patrol_end_time_str = patrol_end_time.strftime("%Y-%m-%d %H:%M:%S")
                    duration = round(((patrol_end_time - patrol_start_time).total_seconds() / 60) / 60, 2)
                    lines.append(f"**Event ID:** {patrol.event_id}\n**Start Time:** {patrol_start_time_str} UTC\n**End Time:** {patrol_end_time_str} UTC\n**Duration:** {duration} hours")
                embed = paginationEmbed.PaginatedEmbed(
                    items=lines,
                    title="Patrol's for " + discord_name_placeholder
                )
                await interaction.followup.send(embed=embed.embed, view=embed)
            else:
                await interaction.followup.send("No patrols found.")

    @patrolling_group.command(name="get-patrol-hours", description="Get patrol hours after a certain date and/or by user.")
    @app_commands.describe(
        date="Date to get all following patrols since then. (Format: YYYY-MM-DD HH:MM:SS)",
        discord_user="Discord user to get patrols for."
    )
    async def get_patrol_hours(self, interaction: discord.Interaction, date: str=None, discord_user: discord.User=None):
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            discord_name_placeholder = "all users"
            if date is not None:
                minimum_date = datetime.datetime.strptime(date, "%Y-%m-%d %H:%M:%S")
            else:
                minimum_date = datetime.datetime.min
            if discord_user is not None:
                discord_id = discord_user.id
            else:
                discord_id = 0

            request = database_service_pb2.GetPatrolHoursRequest(
                guild_id=interaction.guild.id,
                minimum_date=minimum_date,
                discord_id=discord_id
            )
            response = self.grpc_client.call_method("DatabaseService", "GetPatrolHours", request)

            if response.patrol_hours > 0:
                minimum_date_str = minimum_date.strftime("%Y-%m-%d %H:%M:%S")
                allowed_mentions = discord.AllowedMentions(everyone=False, users=False, roles=False)
                if discord_user is None:
                    await interaction.followup.send(f"Total patrol hours for the force since {minimum_date_str}: **{response.patrol_hours} hours**", allowed_mentions=allowed_mentions)
                else:
                    await interaction.followup.send(f"Total patrol hours for {discord_user.mention} since {minimum_date_str}: **{response.patrol_hours} hours**", allowed_mentions=allowed_mentions)
            else:
                await interaction.followup.send("No patrol hours found.")

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
                
    @patrolling_group.command(name="join-patrols", description="Join to patrols together into one patrol.")
    @app_commands.describe(
        first_event_id="ID of the patrol to join to.",
        second_event_id="ID of the patrol to join from."
    )
    async def join_patrols(self, interaction: discord.Interaction, first_event_id: int, second_event_id: int):
        await interaction.response.defer()
        user_role_check = validateUser.validateUser(interaction.user, 4, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            request = database_service_pb2.JoinPatrolsRequest(
                first_event_id=first_event_id,
                second_event_id=second_event_id,
                guild_id=interaction.guild.id
            )
            response = self.grpc_client.call_method("DatabaseService", "JoinPatrols", request)
            if response.response_code == 0:
                await interaction.followup.send("Patrols joined successfully.")
            elif response.response_code == 1:
                await interaction.followup.send("Could not join those two patrols. (Either they are do not exist, are not in your force, or patrol two was not done after patrol one.)")
            elif response.response_code == 2:
                await interaction.followup.send("An exception occurred while joining the patrols.")
        else:
            if user_role_check[1] is None:
                await interaction.followup.send("You do not have permission to use this command.")
            else:
                await interaction.followup.send(user_role_check[1])

async def setup(bot):
    await bot.add_cog(Patrolling(bot))