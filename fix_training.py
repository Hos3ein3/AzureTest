import re

path = "src/AzureTest.Application/Gym/TrainingCommandUnit.cs"
with open(path, "r") as f: c = f.read()
c = c.replace("double? BaseCaloriesBurn) : ICommand", "double? BaseCaloriesBurn, System.Collections.Generic.List<string> Equipments) : ICommand")
c = c.replace("double BaseCaloriesBurn) : ICommand", "double BaseCaloriesBurn, System.Collections.Generic.List<string> Equipments) : ICommand")

c = c.replace("BaseCaloriesBurn = request.BaseCaloriesBurn };", "BaseCaloriesBurn = request.BaseCaloriesBurn, Equipments = request.Equipments ?? new() };")
c = c.replace("training.BaseCaloriesBurn = request.BaseCaloriesBurn;", "training.BaseCaloriesBurn = request.BaseCaloriesBurn;\n        training.Equipments = request.Equipments ?? new();")
with open(path, "w") as f: f.write(c)

path2 = "src/AzureTest.MVC/Controllers/GymAdminController.cs"
with open(path2, "r") as f: c2 = f.read()
c2 = c2.replace("model.BaseCaloriesBurn ?? 0);", "model.BaseCaloriesBurn ?? 0, model.Equipments ?? new());")
c2 = c2.replace("model.BaseCaloriesBurn ?? 0);", "model.BaseCaloriesBurn ?? 0, model.Equipments ?? new());")
with open(path2, "w") as f: f.write(c2)

print("Updated Backend")
