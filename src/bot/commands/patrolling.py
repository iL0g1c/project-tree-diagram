import discord
from discord import app_commands
from discord.ext import commands
from datetime import datetime
from TreeDiagramBot import TreeDiagram


class Patrolling(commands.Cog):
    def __init__(self, bot):
        self.bot = bot
    
    patrolling_group = app_commands.Group(name="patrolling", description="GeoFS Patrolling Commands")
    @patrolling_group.command(name="disable", description="Log a disable.")
    @app_commands.check(Check.check_member)
    async def disable(interaction: discord.Interaction):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)
    
    @patrolling_group.command(name="inactive", description="Get inactive pilots since a specified date.")
    @app_commands.describe(
        year="Year of start date for patrol acceptance.",
        month="Month of start date for patrol acceptance.",
        day="Day of start date for patrol acceptance."
    )
    @app_commands.check(Check.check_member)
    async def inactive_pilots(interaction: discord.Interaction, year: str, month: str, day: str):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="kill", description="Log a kill.")
    @app_commands.check(Check.check_member)
    async def kill(interaction: discord.Interaction):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="log", description="Manually log a patrol.")
    @app_commands.describe(start_year="Year of the start of the patrol.", start_month="Month of the start of the patrol.", start_day="Day of the start of the patrol.", start_hour="Hour of the start of the patrol.", start_minute="Minute of the start of the patrol.", patrol_duration="Duration of the patrol in MINUTES.")
    @app_commands.check(Check.check_member)
    async def log_patrol(interaction: discord.Interaction, start_year: int, start_month: int, start_day: int, start_hour: int, start_minute: int, patrol_duration: int):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="online", description="Get online pilots from discord.")
    @app_commands.check(Check.check_member)
    async def online_pilots(interaction: discord.Interaction):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="patrol-cancel", description="Cancel a patrol.")
    @app_commands.check(Check.check_member)
    async def cancel_patrol(interaction: discord.Interaction):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="patrol-delete", description="Delete a patrol.")
    @app_commands.describe(user="Pilot user.", number="Number of the Patrol to delete.")
    @app_commands.check(Check.check_HC)
    async def delete_patrol(interaction: discord.Interaction, user: discord.Member, number: int):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="patrol-report-role",
                        description="Get a report on patrols done by a specific role since a specified date.")
    @app_commands.describe(
        year="Year of start date for patrol acceptance.",
        month="Month of start date for patrol acceptance.",
        day="Day of start date for patrol acceptance."
    )
    @app_commands.describe(role="Discord role to get patrol report for.")
    @app_commands.check(Check.check_member)
    async def patrol_report_role(interaction: discord.Interaction, year: str, month: str, day: str, role: str):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(
        name="patrol-report",
        description="Get a report on patrols done since a specified date.")
    @app_commands.describe(
        year="Year of start date for patrol acceptance.",
        month="Month of start date for patrol acceptance.",
        day="Day of start date for patrol acceptance."
    )
    @app_commands.check(Check.check_member)
    async def patrol_report(interaction: discord.Interaction, year: str, month: str, day: str):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="pilots-add", description="Add a pilot to the ID database.")
    @app_commands.describe(user="Pilot.",
                        geofs_id="GeoFS ID of the pilot.")
    @app_commands.check(Check.check_HC)
    async def add_id(interaction: discord.Interaction, user: discord.Member, geofs_id: str):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="pilots-discord", description="Get a pilot's ID.")
    @app_commands.describe(user="The pilot.")
    @app_commands.check(Check.check_HC)
    async def get_id(interaction: discord.Interaction, user: discord.Member):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="pilots-id", description="Get a pilot's Discord User.")
    @app_commands.describe(geofs_id="GeoFS ID of the pilot.")
    @app_commands.check(Check.check_HC)
    async def get_discord_id(interaction: discord.Interaction, geofs_id: str):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="pilots-purge", description="Remove from the ID database and add them to the ID storage database.")
    @app_commands.check(Check.check_HC)
    async def remove_ids(interaction: discord.Interaction):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="pilots-remove", description="Remove a pilot from the ID database.")
    @app_commands.describe(user="Pilot.")
    @app_commands.check(Check.check_HC)
    async def remove_id(interaction: discord.Interaction, user: discord.Member):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)
    
    @patrolling_group.command(name="pilots", description="Get all pilots in the ID database.")
    @app_commands.check(Check.check_HC)
    async def get_ids(interaction: discord.Interaction):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="sar", description="Log a SAR.")
    @app_commands.check(Check.check_member)
    async def sar(interaction: discord.Interaction):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)
    
    @patrolling_group.command(name="top-disables", description="Get leaderboard of disables.")
    @app_commands.check(Check.check_member)
    async def top_disables(interaction: discord.Interaction):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="top-kills", description="Get leaderboard of kills.")
    @app_commands.check(Check.check_member)
    async def top_kills(interaction: discord.Interaction):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)
    
    @patrolling_group.command(name="top-sars", description="Get leaderboard of sars.")
    @app_commands.check(Check.check_member)
    async def top_sars(interaction: discord.Interaction):
        await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    async def setup(bot: TreeDiagram):
        bot.add_cog(Patrolling(bot))