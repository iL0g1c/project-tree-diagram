import discord
from discord import app_commands
from discord import AllowedMentions
from discord.ext import commands
import grpc
from proto import database_service_pb2_grpc
from proto import database_service_pb2
import utils.validateUser as validateUser
import utils.configManager as configManager
import utils.paginationEmbed as paginationEmbed

class Force(commands.Cog):
    def __init__(self, bot):
        self.bot = bot
        self.configManager = configManager.ConfigManager()

    force_group = app_commands.Group(name="force", description="Force management commands")

    @force_group.command(name="register-pilot", description="Register a pilot to your force.")
    async def add_pilot(self, interaction: discord.Interaction, geofs_account_id: int, pilot: discord.Member):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.defer()
            with grpc.insecure_channel(self.configManager.host) as channel:
                stub = database_service_pb2_grpc.DatabaseServiceStub(channel)
                request = database_service_pb2.UpdateUserForceCodeRequest(geofs_account_id=int(geofs_account_id), discord_id=int(pilot.id), guild_id=int(interaction.guild.id))
                response = stub.UpdateUserForceCode(request)
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
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.get_config(int(interaction.guild.id)))
        if user_role_check[0]:
            await interaction.response.defer()
            with grpc.insecure_channel(self.configManager.host) as channel:
                stub = database_service_pb2_grpc.DatabaseServiceStub(channel)
                request = database_service_pb2.GetForceUsersRequest(guild_id=int(interaction.guild.id))
                response = stub.GetForceUsers(request)
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

async def setup(bot):
    await bot.add_cog(Force(bot))