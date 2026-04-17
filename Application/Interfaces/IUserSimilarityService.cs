namespace Application.Interfaces;

using Application.DTOs.ML;

/// <summary>
/// Interface cho UserSimilarityService
/// </summary>
public interface IUserSimilarityService
{
    void Train(IEnumerable<UserProductRatingDto> trainingData);
    void LoadModel();
    float PredictRating(uint userId, uint productId);
    IEnumerable<(uint ProductId, float Score)> RecommendProducts(uint userId, IEnumerable<uint> productIds, int topN = 10);
    IEnumerable<(uint UserId, float SimilarityScore)> FindSimilarUsers(uint targetUserId, IEnumerable<uint> candidateUserIds, int topN = 10);
}
