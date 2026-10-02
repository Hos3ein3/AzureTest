import re

def append_to_file(path, code):
    with open(path, 'a') as f:
        f.write("\n" + code + "\n")

page_code = """
public record UpdateIdeaPageLayoutDto(Guid Id, int X, int Y, int Width, int Height);
public record UpdateIdeaPageLayoutCommand(Guid UserId, UpdateIdeaPageLayoutDto Dto) : ICommand<Result>;

public class UpdateIdeaPageLayoutCommandHandler : ICommandHandler<UpdateIdeaPageLayoutCommand, Result>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public UpdateIdeaPageLayoutCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(UpdateIdeaPageLayoutCommand command, CancellationToken ct = default)
    {
        var page = await _unitOfWork.IdeaPages.GetByIdAsync(command.Dto.Id, ct);
        if (page == null || page.IsDeleted) return Result.Failure("Page not found.");
        
        page.X = command.Dto.X;
        page.Y = command.Dto.Y;
        page.Width = command.Dto.Width;
        page.Height = command.Dto.Height;
        
        await _unitOfWork.IdeaPages.UpdateAsync(page.Id, page, ct);
        return Result.Success();
    }
}
"""

section_code = """
public record UpdateIdeaSectionLayoutDto(Guid Id, int X, int Y, int Width, int Height);
public record UpdateIdeaSectionLayoutCommand(Guid UserId, UpdateIdeaSectionLayoutDto Dto) : ICommand<Result>;

public class UpdateIdeaSectionLayoutCommandHandler : ICommandHandler<UpdateIdeaSectionLayoutCommand, Result>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public UpdateIdeaSectionLayoutCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(UpdateIdeaSectionLayoutCommand command, CancellationToken ct = default)
    {
        var sec = await _unitOfWork.IdeaSections.GetByIdAsync(command.Dto.Id, ct);
        if (sec == null || sec.IsDeleted) return Result.Failure("Section not found.");
        
        sec.X = command.Dto.X;
        sec.Y = command.Dto.Y;
        sec.Width = command.Dto.Width;
        sec.Height = command.Dto.Height;
        
        await _unitOfWork.IdeaSections.UpdateAsync(sec.Id, sec, ct);
        return Result.Success();
    }
}
"""

append_to_file('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/IdeaTracker/IdeaPageCommandUnit.cs', page_code)
append_to_file('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/IdeaTracker/IdeaSectionCommandUnit.cs', section_code)

