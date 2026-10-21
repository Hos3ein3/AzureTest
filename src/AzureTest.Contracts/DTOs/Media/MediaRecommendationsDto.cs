using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Media;

public record RecommendedGroupDto(
    string ReasonTitle,
    string ReasonSubtitle,
    string SourceBadge,
    List<TmdbSearchResultDto> Titles
);

public record MediaRecommendationsDto(
    List<RecommendedGroupDto> BecauseYouRewatched,
    List<RecommendedGroupDto> StarringFollowedTalent,
    List<RecommendedGroupDto> SimilarToWatched,
    List<TmdbSearchResultDto> TopPicksForYou
);
