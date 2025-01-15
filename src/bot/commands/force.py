import discord
from discord import app_commands
from discord.ext import commands
import utils.validateUser as validateUser
import grpc
from proto import database_service_pb2_grpc
from proto import database_service_pb2

class Force(commands.Cog):
    def __init__(self, bot):
        self.bot = bot

    force_group = app_commands.Group(name="force", description="Force management commands")

    @force_group.command(name="add-pilot", description="Register a pilot to your force.")
    async def add_pilot(self, interaction: discord.Interaction, geofs_acount_id: int, pilot: discord.Member):
        user_role_check = validateUser.validateUser(interaction.user, 3, self.bot.configManager.config)
        if user_role_check[0]:
            await interaction.response.defer()
            force_code = "IDF" # Temporary force code
            with grpc.insecure_channel("localhost:50051") as channel:
                stub = database_service_pb2_grpc.DatabaseServiceStub(channel)
                request = database_service_pb2.UpdateUserForceCodeRequest(geofs_account_id=int(geofs_acount_id), discord_id=int(pilot.id), force_code=str(force_code))
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

async def setup(bot):
    await bot.add_cog(Force(bot))