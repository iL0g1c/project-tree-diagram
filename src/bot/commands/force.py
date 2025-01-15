import discord
from discord import app_commands
from discord.ext import commands
import utils.validateUser as validateUser

class Force(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    force_group = app_commands.Group(name="force", description="Force management commands")

    @force_group.command(name="add-pilot", description="Register a pilot to your force.")
    async def add_pilot(self, interaction: discord.Interaction, geofs_acount_id: int, pilot: discord.Member):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.defer()
            await interaction.followup.send(f"This command has not yet been implemented.")
        else:
            if user_role_check[1] is None:
                await interaction.response.send_message("You do not have permission to use this command.")
            else:
                await interaction.response.send_message(user_role_check[1])

async def setup(bot):
    await bot.add_cog(Force(bot))