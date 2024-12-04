from setuptools import setup, find_packages

setup(
    name='project-tree-diagram',
    version='0.1.0-alpha',
    packages=find_packages(where='bot'),
    package_dir={'': 'bot'},
    install_requires=[
        "discord.py",
        "python-dotenv",
        "datetime"
    ],
)