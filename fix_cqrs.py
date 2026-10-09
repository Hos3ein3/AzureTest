import os
import glob

files = glob.glob("src/AzureTest.Application/Gym/*CommandUnit.cs")

for file in files:
    with open(file, "r") as f:
        content = f.read()
    
    # We will replace BaseCommandHandler<..., ...> with ICommandHandler<..., ...>
    # And replace "protected override async Task<Result" with "public async Task<Result"
    # And remove ": base(logger)"
    
    lines = content.split('\n')
    new_lines = []
    for line in lines:
        if "class" in line and "CommandHandler :" in line:
            line = line.replace("BaseCommandHandler<", "ICommandHandler<")
        
        if "public" in line and "CommandHandler(" in line:
            # e.g. public CreateTrainingCommandHandler(INoSqlUnitOfWork uow, ILogger<CreateTrainingCommandHandler> logger) : base(logger) => _uow = uow;
            line = line.split(":")[0] + "{ _uow = uow; }"
            # remove ILogger parameter if it's there
            if ", ILogger<" in line:
                parts = line.split(", ILogger<")
                left = parts[0]
                right = parts[1].split("> logger)")[1]
                line = left + ")" + right
            
        if "protected override async Task<Result" in line:
            line = line.replace("protected override async Task<Result", "public async Task<Result")
            if "ExecuteAsync(" in line:
                line = line.replace("ExecuteAsync(", "HandleAsync(")
            if ", CancellationToken ct)" in line:
                line = line.replace(", CancellationToken ct)", ", CancellationToken ct = default)")

        new_lines.append(line)
        
    with open(file, "w") as f:
        f.write("\n".join(new_lines))

print("CQRS Commands Fixed")
