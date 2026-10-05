import os
import re

domain_entities_path = "src/AzureTest.Domain/Entities"
identity_entities_path = "src/AzureTest.Domain/Entities/Identity"
configs_path = "src/AzureTest.Persistence/Configurations"

entities = []
for file in os.listdir(domain_entities_path):
    if file.endswith(".cs"):
        entities.append(file[:-3])

for file in os.listdir(identity_entities_path):
    if file.endswith(".cs"):
        entities.append(file[:-3])

print(f"Entities found: {entities}")
