import discord
from discord import app_commands
from discord.ext import commands
import TreeDiagramBot as TreeDiagram
import utils.validateUser as validateUser


class Patrolling(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    patrolling_group = app_commands.Group(name="patrolling", description="GeoFS Patrolling Commands")
    @patrolling_group.command(name="disable", description="Log a disable.")
    async def disable(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 5, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])
    
    @patrolling_group.command(name="inactive", description="Get inactive pilots since a specified date.")
    @app_commands.describe(
        year="Year of start date for patrol acceptance.",
        month="Month of start date for patrol acceptance.",
        day="Day of start date for patrol acceptance."
    )
    async def inactive_pilots(self, interaction: discord.Interaction, year: str, month: str, day: str):
        user_role_check = validateUser.validateUser(interaction.user, 5, self.bot.configManager.config)
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

    @patrolling_group.command(name="log", description="Manually log a patrol.")
    @app_commands.describe(
        start_year="Year of the start of the patrol.",
        start_month="Month of the start of the patrol.",
        start_day="Day of the start of the patrol.",
        start_hour="Hour of the start of the patrol.",
        start_minute="Minute of the start of the patrol.",
        patrol_duration="Duration of the patrol in MINUTES."
    )
    async def log_patrol(self, interaction: discord.Interaction, start_year: int, start_month: int, start_day: int, start_hour: int, start_minute: int, patrol_duration: int):
        user_role_check = validateUser.validateUser(interaction.user, 5, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="online", description="Get online pilots from discord.")
    async def online_pilots(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 5, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="patrol-cancel", description="Cancel a patrol.")
    async def cancel_patrol(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 5, self.bot.configManager.config)
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
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
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
        user_role_check = validateUser.validateUser(interaction.user, 5, self.bot.configManager.config)
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
        user_role_check = validateUser.validateUser(interaction.user, 5, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="pilots-add", description="Add a pilot to the ID database.")
    @app_commands.describe(user="Pilot.", geofs_id="GeoFS ID of the pilot.")
    async def add_id(self, interaction: discord.Interaction, user: discord.Member, geofs_id: str):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="pilots-discord", description="Get a pilot's ID.")
    @app_commands.describe(user="The pilot.")
    async def get_id(self, interaction: discord.Interaction, user: discord.Member):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="pilots-id", description="Get a pilot's Discord User.")
    @app_commands.describe(geofs_id="GeoFS ID of the pilot.")
    async def get_discord_id(self, interaction: discord.Interaction, geofs_id: str):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="pilots-purge", description="Remove from the ID database and add them to the ID storage database.")
    async def remove_ids(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="pilots-remove", description="Remove a pilot from the ID database.")
    @app_commands.describe(user="Pilot.")
    async def remove_id(self, interaction: discord.Interaction, user: discord.Member):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])
    
    @patrolling_group.command(name="pilots", description="Get all pilots in the ID database.")
    async def get_ids(self, interaction: discord.Interaction):
        if validateUser.validateUser(interaction.user, 3, self.self.bot.configManager.config):
            await interaction.response.send_message("This command has not been implemented.", ephemeral=True)

    @patrolling_group.command(name="sar", description="Log a SAR.")
    async def sar(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 5, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])
    
    @patrolling_group.command(name="top-disables", description="Get leaderboard of disables.")
    async def top_disables(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 5, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @patrolling_group.command(name="top-kills", description="Get leaderboard of kills.")
    async def top_kills(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 5, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])
    
    @patrolling_group.command(name="top-sars", description="Get leaderboard of sars.")
    async def top_sars(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 5, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.send_message("This command has not been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

async def setup(bot: TreeDiagram):
    await bot.add_cog(Patrolling(bot))