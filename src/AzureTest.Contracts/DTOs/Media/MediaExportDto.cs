using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Media;

public record MediaExportDto(
    string AppVersion,
    DateTime ExportedAt,
    List<WatchItemDto> WatchItems,
    List<FollowedPersonDto> FollowedPeople,
    List<CustomMediaListDto> CustomLists
);

public record MediaImportResultDto(
    bool Success,
    int WatchItemsImported,
    int FollowedPeopleImported,
    int CustomListsImported,
    List<string> Errors
);
