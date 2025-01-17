import discord
from discord import app_commands
from discord.ext import commands
import grpc
import utils.validateUser as validateUser
import utils.paginationEmbed as paginationEmbed
import utils.configManager as configManager

class Configuration(commands.Cog):
    def __init__(self, bot):
        self.bot = bot
        self.configManager = configManager.ConfigManager()

    config_group = app_commands.Group(name="config", description="Bot Configuration Commands")

    @config_group.command(name="set", description="Change the bot's configuration.")
    @app_commands.choices(
        key=[
            app_commands.Choice(name="Force Identifier", value="force_code"),
            app_commands.Choice(name="Developer Role ID", value="developer_role_id"),
            app_commands.Choice(name="High Command Role ID", value="high_command_role_id"),
            app_commands.Choice(name="Member Role ID", value="member_role_id"),
            app_commands.Choice(name="Player Activity Channel ID", value="player_activity_channel_id"),
            app_commands.Choice(name="Patrol Log Channel ID", value="patrol_log_channel_id"),
            app_commands.Choice(name="Callsign Change Channel ID", value="callsign_change_channel_id"),
            app_commands.Choice(name="Callsign Code Channel ID", value="callsign_code_channel_id"),
            app_commands.Choice(name="Callsign Code Loop Enabled", value="callsign_code_loop_enabled"),
        ]
    )
    async def config_change(self, interaction: discord.Interaction, key: app_commands.Choice[str], value: str):
        if key.value == "developer_role_id":
            user_role_check = validateUser.validateUser(interaction.user, 1, self.bot.configManager.get_config(int(interaction.guild.id)), is_config_change=True)
        else:
            user_role_check = validateUser.validateUser(interaction.user, 2, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.defer()
            success = self.configManager.update_key(interaction.guild.id, key.value, value)
            if success:
                await interaction.followup.send(f"Configuration key `{key.name}` has been updated to `{value}`.")
            else:
                await interaction.followup.send(f"Failed to update configuration key `{key.name}`.")
                
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @config_group.command(name="keys", description="Get the list of the bot's configuration keys.")
    async def get_keys(self, interaction: discord.Interaction):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.defer()

            keys = self.configManager.get_config(int(interaction.guild.id))
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