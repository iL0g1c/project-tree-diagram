import discord
from discord import app_commands
from discord.ext import commands
import TreeDiagramBot as TreeDiagram
import utils.configManager as configManager
import utils.validateUser as validateUser
import utils.paginationEmbed as paginationEmbed

class Configuration(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    config_group = app_commands.Group(name="config", description="Bot Configuration Commands")

    @config_group.command(name="create", description="Create a new configuration key.")
    async def config_create(self, interaction: discord.Interaction, key: str, value: str):
        user_role_check = validateUser.validateUser(interaction.user, 1, self.bot.configManager.config)
        if user_role_check[0] or "developer_role" not in self.bot.configManager.config:
            await interaction.response.defer()
            success, error = self.bot.configManager.create_key(key, value)
            if not success:
                await interaction.followup.send(error)
            else:
                await interaction.followup.send(f"Configuration key `{key}` has been created with value `{value}`.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

    @config_group.command(name="delete", description="Delete a configuration key.")
    async def config_delete(self, interaction: discord.Interaction, key: str):
        user_role_check = validateUser.validateUser(interaction.user, 1, self.bot.configManager.config)
        if user_role_check[0] or "developer_role" not in self.bot.configManager.config:
            await interaction.response.defer()
            success, error = self.bot.configManager.destroy_key(key)
            if not success:
                await interaction.followup.send(error)
            else:
                await interaction.followup.send(f"Configuration key `{key}` has been deleted.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

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
            keys = list(self.bot.configManager.config.items())
            lines = [f"* {key}: `{value}`" for key, value in keys]

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

async def setup(bot: TreeDiagram):
    await bot.add_cog(Configuration(bot))